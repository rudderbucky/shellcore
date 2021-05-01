using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NodeEditorFramework.IO;
using UnityEngine.Events;
using NodeEditorFramework.Standard;
using NodeEditorFramework;
using System.Linq;

public class WCGeneratorHandler : MonoBehaviour
{
    public struct SectorData 
    {
		public string sectorjson;
		public string platformjson;
	}
    public WorldCreatorCursor cursor;
    List<Sector> sectors = new List<Sector>();
    public GameObject sectorPrefab;
    public ItemHandler itemHandler;
    public Text invalidNameWarning;
    public InputField worldName;
    public InputField worldReadPath;
    public InputField blueprintField;
    public InputField authorField;
    public InputField descriptionField;
    public WCCharacterHandler characterHandler;
    public NodeEditorFramework.Standard.RTNodeEditor nodeEditor;
    public Item characterItem;
    public GameObject savingLevelScreen;
    public UnityEvent OnSectorSaved;

    public int saveState = 0; // 0 : not saving, 1 : saving, 2 : completed successfully, > 2 : something went wrong

    private static string testPath = Application.streamingAssetsPath + "\\Sectors\\TestWorld";
    List<WorldData.PartIndexData> partData = new List<WorldData.PartIndexData>();
    

    public static void DeleteTestWorld()
    {
        if(System.IO.Directory.Exists(testPath))
        {
            foreach(var dir in System.IO.Directory.GetDirectories(testPath))
            {
                foreach(var file in System.IO.Directory.GetFiles(dir))
                {
                    System.IO.File.Delete(file);
                }
                System.IO.Directory.Delete(dir);
            }

            foreach(var file in System.IO.Directory.GetFiles(testPath))
            {
                System.IO.File.Delete(file);
            }
            System.IO.Directory.Delete(testPath);
        }

        WCWorldIO.DeletePlaceholderDirectories();
    }

    ///<summary>
    /// path1 => path2
    ///</summary>
    private void TryCopy(string path1, string path2)
    {
        if(System.IO.Directory.Exists(path2))
        {
            foreach(var file in System.IO.Directory.GetFiles(path2))
            {
                #if UNITY_EDITOR
                if(!file.Contains(".meta"))
                {
                    System.IO.File.Delete(file);
                }
                #else
                System.IO.File.Delete(file);
                #endif
            }
        }
        else System.IO.Directory.CreateDirectory(path2);

        System.IO.Directory.CreateDirectory(path1);
        string[] files = System.IO.Directory.GetFiles(path1);
        foreach(string file in files)
        {
            if(!file.Contains(".meta"))
                System.IO.File.Copy(file, path2 + "\\" + System.IO.Path.GetFileName(file));
            else
                System.IO.File.Move(file, path2 + "\\" + System.IO.Path.GetFileName(file));
        }
    }

    public void OnNameEdit(string tmpWworldName)
    {
        invalidNameWarning.enabled = 
            tmpWworldName == null 
            || tmpWworldName == "" 
            || tmpWworldName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) > -1;
    }

    public bool WriteWorld(string path)
    {
        if (invalidNameWarning.enabled)
        {
            Debug.LogError("Path your damn world! Abort.");
            return false;
        }

#if UNITY_EDITOR
#else
        if(path.Contains("main")) return false;
#endif
        StartCoroutine(WriteWorldCo(path));

        // assuming it was a success for now...
        return true;
    }

    IEnumerator WriteWorldCo(string path)
    {
        Debug.Log("Writing world...");

        // Folder paths
        var canvasPlaceholderPath = Application.streamingAssetsPath + "\\CanvasPlaceholder";
        var entityPlaceholderPath = Application.streamingAssetsPath + "\\EntityPlaceholder";
        var wavePlaceholderPath = Application.streamingAssetsPath + "\\WavePlaceholder";

        // Reinitialize node editor
        NodeEditor.ReInit(false);

        saveState = 1;
        yield return null;
        sectors = new List<Sector>();
        var items = cursor.placedItems;
        var wrappers = cursor.sectors;
        foreach(var wrapper in wrappers) 
        {
            sectors.Add(wrapper.sector);
        }

        int minX = int.MaxValue;
        int maxY = int.MinValue;
        // Get the world bounds
        foreach(var sector in sectors) 
        {
            if(sector.bounds.x < minX) minX = sector.bounds.x;
            if(sector.bounds.y > maxY) maxY = sector.bounds.y;
        }

        // ensure spawn point in some sector
        if(sectors.TrueForAll(sector => !sector.bounds.contains(cursor.spawnPoint.position)))
        {
            Debug.LogError("Spawn point not in sector bounds. Abort.");
            yield break;
        }

        // set up items and platforms
        int ID = 0;
        Dictionary<Sector, List<Sector.LevelEntity>> sectEnts = new Dictionary<Sector, List<Sector.LevelEntity>>();
        Dictionary<Sector, List<string>> sectTargetIDS = new Dictionary<Sector, List<string>>();
        foreach(var sector in sectors)
        {
            sectEnts.Add(sector, new List<Sector.LevelEntity>());
            sectTargetIDS.Add(sector, new List<string>());
            sector.tiles = new List<GroundPlatform.Tile>();
        }

        // Add background spawns to part index
        partData.Clear();
        foreach(var sector in sectors)
        {
            if (sector.backgroundSpawns != null)
                foreach(var spawn in sector.backgroundSpawns)
                {
                    AttemptAddShellCoreParts(spawn.entity, sector.sectorName, path);
                }
        }

        Dictionary<string, string> itemSectorsByID = new Dictionary<string, string>();
        
        foreach(var item in items)
        {
            Sector container = GetSurroundingSector(item.pos);
            if(container == null)
            {
                savingLevelScreen.SetActive(false);
                saveState = 3;
                Debug.LogError("No container for item. Abort.");
                yield break;
            }
            switch(item.type)
            {
                case ItemType.Platform:
                    var index = GetPlatformIndices(container, item.pos);
                    container.tiles.Add(new GroundPlatform.Tile()
                    {
                        pos = new Vector2Int(index.Item2, index.Item1),
                        type = (byte)item.placeablesIndex,
                        rotation = (byte)(((int)item.obj.transform.rotation.eulerAngles.z / 90) % 4),
                        directions = new Dictionary<Vector2Int, byte>()
                    });
                    break;
                case ItemType.Other:
                case ItemType.Decoration:
                case ItemType.Flag:
                    Sector.LevelEntity ent = new Sector.LevelEntity();
                    if(cursor.characters.TrueForAll((WorldData.CharacterData x) => {return x.ID != item.ID;})) 
                    {
                        // Debug.Log(item.ID + " is not a character. " + ID);
                        int test;
                        if(item.ID == null || item.ID == "" || int.TryParse(item.ID, out test))
                        {
                            ent.ID = ID++ + "";
                        }
                        else 
                        {
                            ent.ID = item.ID;
                            if(itemSectorsByID.ContainsKey(ent.ID))
                            {
                                savingLevelScreen.SetActive(false);
                                saveState = 4;
                                Debug.LogError("Two items in sectors " + container.sectorName + " and " 
                                    + itemSectorsByID[ent.ID] + " were issued the same custom ID. Abort.");
                                yield break;
                            }
                            else itemSectorsByID.Add(ent.ID, container.sectorName);
                        }

                        // Debug.Log(container.sectorName + " " + ent.ID);
                    }
                    else 
                    {
                        // TODO: adjust faction
                        Debug.Log("Character found. Adjusting ID and name");
                        ent.ID = item.ID;
                    }
                    // you can choose to give any object a custom name
                    if(item.name != null && item.name != "")
                        ent.name = item.name;
                    else ent.name = item.obj.name;
                    ent.faction = item.faction;
                    ent.position = item.pos;
                    ent.assetID = item.assetID;
                    ent.vendingID = item.vendingID;
                    ent.patrolPath = item.patrolPath;
                    if((item.isTarget && container.type != Sector.SectorType.SiegeZone)
                        || (container.type == Sector.SectorType.SiegeZone && item.assetID == "outpost_blueprint" && item.faction == 0)
                        || (container.type == Sector.SectorType.SiegeZone && item.assetID == "bunker_blueprint" && item.faction == 0)) 
                            sectTargetIDS[container].Add(ent.ID);
                    var charExists = cursor.characters.Exists(ch => ch.ID == ent.ID );
                    if(ent.assetID == "shellcore_blueprint" || charExists)
                {
                        if(container.type != Sector.SectorType.SiegeZone && !sectTargetIDS[container].Contains(ent.ID))
                            sectTargetIDS[container].Add(ent.ID);
                        ent.blueprintJSON = item.shellcoreJSON;
                        if(!charExists)
                        {
                            AttemptAddShellCoreParts(ent, container.sectorName, path);
                        }
                    }
                    else if(ent.assetID == "trader_blueprint")
                    {
                        ent.blueprintJSON = item.shellcoreJSON;

                        // Attempt to add trader parts into index.
                        if(ent.blueprintJSON == null || ent.blueprintJSON == "")
                        {
                            var dialogueDataPath = $"{canvasPlaceholderPath}\\{ent.ID}.dialoguedata";
                            
                            if(System.IO.File.Exists(dialogueDataPath))
                            {
                                var XMLImport = new XMLImportExport();
                                var canvas = XMLImport.Import(dialogueDataPath) as DialogueCanvas;
                                foreach(var node in canvas.nodes)
                                {
                                    if(node is EndDialogue)
                                    {
                                        var endDialogue = node as EndDialogue;
                                        if(endDialogue.openTrader)
                                        {
                                            ShipBuilder.TraderInventory traderInventory = 
                                                JsonUtility.FromJson<ShipBuilder.TraderInventory>(endDialogue.traderJSON);
                                            Debug.LogError(container.sectorName + "end dialog");
                                            AttemptAddPartArray(traderInventory.parts, container.sectorName);
                                        }
                                        
                                    }
                                }
                            }
                            else 
                            {
                                ent.blueprintJSON = JsonUtility.ToJson(new ShipBuilder.TraderInventory());
                                // Maybe make this error message more descriptive.
                                Debug.LogWarning($"Trader has neither default trader JSON nor an associated dialogue file named '{ent.ID}.dialoguedata'. Replacing with empty trader inventory.");
                            }
                        }
                        else
                        {
                            ShipBuilder.TraderInventory traderInventory = 
                                JsonUtility.FromJson<ShipBuilder.TraderInventory>(ent.blueprintJSON);
                            AttemptAddPartArray(traderInventory.parts, container.sectorName);
                        }
                    }
                    else if(ent.assetID == "groundcarrier_blueprint" || ent.assetID == "carrier_blueprint" || ent.assetID == "outpost_blueprint"
                        || ent.assetID == "bunker_blueprint")
                    {
                        ent.blueprintJSON = item.shellcoreJSON;
                    }

                    sectEnts[container].Add(ent);
                    break;   
                default:
                    break;
            }
        }

        if(!System.IO.Directory.Exists(canvasPlaceholderPath)) System.IO.Directory.CreateDirectory(canvasPlaceholderPath);
        // Add reward parts from tasks.
        if (System.IO.Directory.Exists(canvasPlaceholderPath))
            foreach(var canvasPath in System.IO.Directory.GetFiles(canvasPlaceholderPath))
            {
                if(System.IO.Path.GetExtension(canvasPath) == ".taskdata")
                {
                    var XMLImport = new XMLImportExport();
                    var canvas = XMLImport.Import(canvasPath) as QuestCanvas;

                    string missionName = null;
                    foreach(var node in canvas.nodes)
                    {
                        if(node is StartMissionNode)
                        {
                            var startMission = node as StartMissionNode;
                            missionName = startMission.missionName;
                        }
                    }

                    foreach(var node in canvas.nodes)
                    {
                        if(node is StartTaskNode)
                        {
                            var startTask = node as StartTaskNode;
                            if(startTask.partReward)
                            {
                                EntityBlueprint.PartInfo part = new EntityBlueprint.PartInfo();
                                part.partID = startTask.partID;
                                part.abilityID = startTask.partAbilityID;
                                part.tier = startTask.partTier;
                                part.secondaryData = startTask.partSecondaryData;
                                part = PartIndexScript.CullToPartIndexValues(part);

                                AddPart(part, missionName);
                            }
                        
                        }
                    }
                }
            }

        // calculate land platform pathfinding directions
        foreach (var sector in sectors)
        {
            if (sector.tiles != null && sector.tiles.Count > 0)
            {
                sector.platforms = LandPlatformGenerator.DivideToPlatforms(sector.tiles);
                List<string> data = new List<string>();
                foreach (var plat in sector.platforms)
                {
                    plat.GenerateDirections();
                    data.Add(plat.Encode());
                }
                sector.platformData = data.ToArray();
            }
            else 
            {
                sector.platforms = new GroundPlatform[0];
                sector.platformData = new string[0];
            }
        }

        // write all sectors into a file
        if (!System.IO.Directory.Exists(path)) {
			System.IO.Directory.CreateDirectory(path);
		}
        
        // Delete all unnecessary files
        if(System.IO.Directory.Exists(path))
        {
            string[] resPaths = ResourceManager.Instance.GetFileNames(path);

            for (int i = 0; i < resPaths.Length; i++)
            {
                resPaths[i] = resPaths[i].Replace('\\', '/');
                Debug.Log("Res path: " + resPaths[i]);
            }

            string[] directories = System.IO.Directory.GetDirectories(path);
            foreach(var dir in directories)
            {
                bool del = true;
                foreach(var f in System.IO.Directory.GetFiles(dir))
                {
                    Debug.Log("File in dir: " + System.IO.Path.Combine(dir, f));
                    if (!resPaths.Contains(System.IO.Path.Combine(dir, f).Replace('\\', '/')))
                    {
                        System.IO.File.Delete(f);
                    }
                    del = false;
                }
                if (del)
                    System.IO.Directory.Delete(dir);
            }

            string[] files = System.IO.Directory.GetFiles(path);
            foreach(var file in files)
            {
                string f = file.Replace('\\', '/');
                if (!resPaths.Contains(f) && f != System.IO.Path.Combine(path, "ResourceData.txt").Replace('\\', '/'))
                    System.IO.File.Delete(file);
            }
        }   

        // create world data
        WorldData wdata = ScriptableObject.CreateInstance<WorldData>();
        wdata.initialSpawn = cursor.spawnPoint.position;
        wdata.defaultCharacters = cursor.characters.ToArray();
        wdata.defaultBlueprintJSON = blueprintField.text;
        wdata.author = authorField.text;
        wdata.description = descriptionField.text;
        wdata.partIndexDataArray = partData.ToArray();

        string wdjson = JsonUtility.ToJson(wdata);
        System.IO.File.WriteAllText(path + "\\world.worlddata", wdjson);

        TryCopy(canvasPlaceholderPath, path + "\\Canvases\\");
        TryCopy(entityPlaceholderPath, path + "\\Entities\\");
        TryCopy(wavePlaceholderPath, path + "\\Waves\\");

        foreach(var sector in sectors)
        {
            if(sector.sectorName == null || sector.sectorName == "")
            {
                sector.sectorName = GetDefaultName(sector, minX, maxY);
            }

            if(sector.hasMusic && (sector.musicID == null || sector.musicID == ""))
            {
                sector.musicID = GetDefaultMusic(sector.type);
            }
            
            sector.entities = sectEnts[sector].ToArray();
            sector.targets = sectTargetIDS[sector].ToArray();
            // sector.backgroundColor = SectorColors.colors[(int)sector.type];

            SectorData data = new SectorData();
            data.sectorjson = JsonUtility.ToJson(sector);
            data.platformjson = ""; // For backwards compatibility...

            string output = JsonUtility.ToJson(data);

            string sectorPath = path + "\\." + sector.sectorName + ".json";                
            System.IO.File.WriteAllText(sectorPath, output);
        }

		Debug.Log("JSON written to location: " + path);
        Debug.Log($"Index size: {partData.Count}");
        savingLevelScreen.SetActive(false);
        saveState = 2;
        if (OnSectorSaved != null)
            OnSectorSaved.Invoke();
    }

    string GetDefaultName(Sector sector, int minX, int maxY)
    {
        int x = sector.bounds.x - minX;
        int y = maxY - sector.bounds.y;
        string typeRep;
        switch(sector.type)
        {
            case Sector.SectorType.BattleZone:
                typeRep = "Battle Zone";
                break;
            case Sector.SectorType.DangerZone:
                typeRep = "Danger Zone";
                break;
            case Sector.SectorType.Haven:
                typeRep = "Haven";
                break;
            case Sector.SectorType.Capitol:
                typeRep = "Capitol";
                break;
            case Sector.SectorType.SiegeZone:
                typeRep = "Siege Zone";
                break;
            default:
                typeRep = "Sector";
                break;
        } 

        return typeRep + " " + x + "-" + y;
    }

    public static string GetDefaultMusic(Sector.SectorType type)
    {
        switch(type)
        {
            case Sector.SectorType.BattleZone:
                return PlayerPrefs.GetString($"WCSectorPropertyDisplay_defaultMusic{(int)type}", "music_fast");
            case Sector.SectorType.Capitol:
                return PlayerPrefs.GetString($"WCSectorPropertyDisplay_defaultMusic{(int)type}", "music_funktify");
                // Funktify made by Mr Spastic, website - http://www.mrspastic.com
            case Sector.SectorType.SiegeZone:
                return PlayerPrefs.GetString($"WCSectorPropertyDisplay_defaultMusic{(int)type}", "music_siege_1");
            default:
                return PlayerPrefs.GetString($"WCSectorPropertyDisplay_defaultMusic{(int)type}", "music_overworld");
        } 
    }

    Sector GetSurroundingSector(Vector2 pos) {
        foreach(var sector in sectors)
        {
            if(sector.bounds.contains(pos)) return sector;
        }
        return null;
    }

    (int, int) GetPlatformIndices(Sector sector, Vector2 pos) 
    {
        int row = (sector.bounds.y - (int)pos.y) / (int)cursor.tileSize;
        int col = ((int)pos.x - sector.bounds.x) / (int)cursor.tileSize;
        return (row, col);
    }

    public void WriteWorldFromEditorPrompt()
    {
        #if UNITY_EDITOR
        var str = UnityEditor.EditorUtility.SaveFolderPanel(
        "Write World (You must create the folder you want to save into) ", 
        Application.streamingAssetsPath + "\\Sectors", "DefaultWorldName");
        WriteWorld(str);
        #endif
    }

    public void ReadWorldFromEditorPrompt()
    {
        #if UNITY_EDITOR
        var str = UnityEditor.EditorUtility.OpenFolderPanel("Read World (Folder)", Application.streamingAssetsPath + "\\Sectors", "");
        ReadWorld(str);
        #endif
    }

    public void ReadWorldFromField()
    {
        string path = worldReadPath.text;
        ReadWorld(path);
    }
    public void ReadWorld(string path) 
    {
        if (System.IO.Directory.Exists(path))
        {
            try
            {
                //cursor.Clear();
                // resource pack loading
                if (!ResourceManager.Instance.LoadResources(path) && SectorManager.testResourcePath != null)
                {
                    ResourceManager.Instance.LoadResources(SectorManager.testResourcePath);
                }

                // copying canvases
                TryCopy(path + "\\Canvases\\", Application.streamingAssetsPath + "\\CanvasPlaceholder");

                // copying entities
                TryCopy(path + "\\Entities\\", Application.streamingAssetsPath + "\\EntityPlaceholder");

                // copying waves
                TryCopy(path + "\\Waves\\", Application.streamingAssetsPath + "\\WavePlaceholder");

                // reading sectors
                string[] files = System.IO.Directory.GetFiles(path);

                cursor.placedItems = new List<Item>();
                cursor.sectors = new List<WorldCreatorCursor.SectorWCWrapper>();
                
                foreach (string file in files)
                {
                    if(file.Contains(".meta")) continue;
                    
                    // parse world data
                    if(file.Contains(".worlddata"))
                    {
                        string worlddatajson = System.IO.File.ReadAllText(file);
                        WorldData wdata = ScriptableObject.CreateInstance<WorldData>();
                        JsonUtility.FromJsonOverwrite(worlddatajson, wdata);

                        cursor.spawnPoint.position = wdata.initialSpawn;
                        // add characters into character handler
                        foreach(var ch in wdata.defaultCharacters)
                        {
                            characterHandler.AddCharacter(ch);
                        }

                        blueprintField.text = wdata.defaultBlueprintJSON;
                        // authorField.text = wdata.author;
                        // descriptionField.text = wdata.description;
                        continue;
                    }

                    if(file.Contains(".taskdata") || file.Contains(".dialoguedata") || file.Contains(".sectordata"))
                    {
                        continue;
                    }

                    if (file.Contains("ResourceData.txt"))
                    {
                        continue;
                    }

                    if (ResourceManager.fileNames.Contains(file))
                    {
                        continue;
                    }

                    string sectorjson = System.IO.File.ReadAllText(file);
                    SectorCreatorMouse.SectorData data = JsonUtility.FromJson<SectorCreatorMouse.SectorData>(sectorjson);
                    // Debug.Log("Platform JSON: " + data.platformjson);
                    // Debug.Log("Sector JSON: " + data.sectorjson);
                    Sector curSect = ScriptableObject.CreateInstance<Sector>();
                    JsonUtility.FromJsonOverwrite(data.sectorjson, curSect);

                    // Try to load old land platform
                    if (data.platformjson != "" && curSect.platformData == null)
                    {
                        Debug.Log("Loading OLD platforms!");
                        LandPlatform plat = ScriptableObject.CreateInstance<LandPlatform>();
                        JsonUtility.FromJsonOverwrite(data.platformjson, plat);
                        plat.name = curSect.name + "Platform";
                        for (int i = 0; i < plat.rows; i++)
                        {
                            for (int j = 0; j < plat.columns; j++)
                            {
                                int placeablesIndex = plat.tilemap[plat.columns * i + j];
                                foreach (Item item in itemHandler.itemPack.items)
                                {
                                    if (item.type == ItemType.Platform && item.placeablesIndex == placeablesIndex)
                                    {
                                        Item copy = itemHandler.CopyItem(item);
                                        copy.pos = copy.obj.transform.position
                                            = new Vector2(cursor.cursorOffset.x + curSect.bounds.x + j * cursor.tileSize,
                                                -cursor.cursorOffset.y + curSect.bounds.y - i * cursor.tileSize);
                                        copy.rotation = plat.rotations[plat.columns * i + j];
                                        copy.obj.transform.RotateAround(copy.pos, Vector3.forward, 90 * copy.rotation);
                                        cursor.placedItems.Add(copy);
                                    }
                                }
                            }
                        }
                        Destroy(plat);
                        curSect.platform = null;
                    }
                    else
                    {
                        foreach (var platData in curSect.platformData)
                        {
                            GroundPlatform plat = new GroundPlatform(platData);

                            GroundPlatform.Tile[] tiles = plat.tiles.ToArray();
                            for (int i = 0; i < tiles.Length; i++)
                            {
                                int placeablesIndex = tiles[i].type;
                                foreach (Item item in itemHandler.itemPack.items)
                                {
                                    if (item.type == ItemType.Platform && item.placeablesIndex == placeablesIndex)
                                    {
                                        Item copy = itemHandler.CopyItem(item);
                                        copy.pos = copy.obj.transform.position
                                            = new Vector2(cursor.cursorOffset.x + curSect.bounds.x + tiles[i].pos.x * cursor.tileSize,
                                                -cursor.cursorOffset.y + curSect.bounds.y - tiles[i].pos.y * cursor.tileSize);
                                        copy.rotation = tiles[i].rotation;
                                        copy.obj.transform.RotateAround(copy.pos, Vector3.forward, 90 * copy.rotation);
                                        cursor.placedItems.Add(copy);
                                    }
                                }
                            }
                        }
                    }

                    LineRenderer renderer = Instantiate(sectorPrefab).GetComponent<LineRenderer>();
                    renderer.SetPositions(new Vector3[] 
                        {
                            new Vector2(curSect.bounds.x, curSect.bounds.y),
                            new Vector2(curSect.bounds.x, curSect.bounds.y - curSect.bounds.h),
                            new Vector2(curSect.bounds.x + curSect.bounds.w, curSect.bounds.y - curSect.bounds.h),
                            new Vector2(curSect.bounds.x + curSect.bounds.w, curSect.bounds.y)
                        }
                    );
                    var wrapper = new WorldCreatorCursor.SectorWCWrapper();
                    wrapper.sector = curSect;
                    wrapper.renderer = renderer;
                    wrapper.renderer.GetComponentInChildren<WorldCreatorSectorRepScript>().sector = wrapper.sector;
                    cursor.sectors.Add(wrapper);

                    foreach(Sector.LevelEntity ent in curSect.entities)
                    {
                        foreach(Item item in itemHandler.itemPack.items)
                        {
                            if(ent.assetID == item.assetID && ent.assetID != "")
                            {
                                Item copy = itemHandler.CopyItem(item);
                                copy.faction = ent.faction;
                                copy.ID = ent.ID;
                                copy.name = ent.name;
                                copy.pos = copy.obj.transform.position = ent.position;
                                copy.vendingID = ent.vendingID;
                                copy.shellcoreJSON = ent.blueprintJSON;
                                copy.patrolPath = ent.patrolPath;
                                cursor.placedItems.Add(copy);
                            }              
                        }
                    }
                }

                /*
                // now create the character items
                foreach(var sector in cursor.sectors)
                {
                    foreach(var ent in sector.sector.entities)
                    {
                        if(cursor.characters.Exists((WorldData.CharacterData x) => {return x.ID == ent.ID;})) 
                        {
                            Debug.Log("Character found. Creating new item.");
                            Item copy = itemHandler.CopyItem(characterItem);
                            copy.faction = ent.faction;
                            copy.ID = ent.ID;
                            copy.name = ent.name;
                            copy.pos = copy.obj.transform.position = ent.position;
                            copy.vendingID = ent.vendingID;
                            cursor.placedItems.Add(copy);
                        }
                    }
                }
                */

                ImportExportFormat.RuntimeIOPath = Application.streamingAssetsPath + "\\CanvasPlaceholder";
                Debug.Log("World loaded");
                return;
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            };
            Input.ResetInputAxes(); // clear the copy paste ctrl press if there was one
        }
    }

    public void AttemptAddPartArray(List<EntityBlueprint.PartInfo> parts, string sectorName)
    {
        
        foreach(var part in parts)
        {
            AddPart(part, sectorName);
        }
    }

    public void AttemptAddShellCoreParts(Sector.LevelEntity entity, string sectorName, string path)
    {
        EntityBlueprint blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();

        // try parsing directly, if that fails try fetching the entity file
        try
        {
            JsonUtility.FromJsonOverwrite(entity.blueprintJSON, blueprint);
        }
        catch
        {
            JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText
                (Application.streamingAssetsPath + "\\EntityPlaceholder\\" + entity.blueprintJSON + ".json"), blueprint);
        }

        
        if(blueprint.intendedType == EntityBlueprint.IntendedType.ShellCore && entity.faction == 1)
        {
            if(blueprint.parts != null)
            {
                foreach(var part in blueprint.parts)
                {
                    AddPart(part, sectorName);
                }
            }       
        }
    }


    ///
    /// Attempt to add a part into the index, check if the player obtained/saw it
    ///
    public void AddPart(EntityBlueprint.PartInfo part, string origin)
    {
        part = PartIndexScript.CullToPartIndexValues(part);
        WorldData.PartIndexData data = partData.Find((pData) => pData.part.Equals(part));
        if(data == null)
        {
            data = new WorldData.PartIndexData();
            data.part = part;
            data.origins = new List<string>();
            partData.Add(data);
        }
        if(!data.origins.Contains(origin)) data.origins.Add(origin);
    }
}
