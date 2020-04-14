using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NodeEditorFramework.IO;
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
    public InputField worldName;
    public InputField worldReadPath;
    public WCCharacterHandler characterHandler;
    public NodeEditorFramework.Standard.RTNodeEditor nodeEditor;
    public Item characterItem;

    private static string testPath = Application.streamingAssetsPath + "\\Sectors\\TestWorld";

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
                System.IO.File.Delete(file);
            }
        }
        else System.IO.Directory.CreateDirectory(path2);

        System.IO.Directory.CreateDirectory(path1);
        string[] files = System.IO.Directory.GetFiles(path1);
        foreach(string file in files)
        {
            if(!file.Contains(".meta"))
                System.IO.File.Copy(file, path2 + "\\" + System.IO.Path.GetFileName(file));
        }
    }

    public bool WriteWorld(string path) 
    {
        if(path == null || path == "")
        {
            Debug.LogError("Path your damn world! Abort.");
            return false;
        }

        #if UNITY_EDITOR
        #else
        if(path.Contains("main")) return false;
        #endif

        sectors = new List<Sector>();
        var items = cursor.placedItems;
        var wrappers = cursor.sectors;
        foreach(var wrapper in wrappers) 
        {
            sectors.Add(wrapper.sector);
        }

        int minX = int.MaxValue;
        int maxY = int.MinValue;
        // create land platforms for each sector
        foreach(var sector in sectors) 
        {
            if(sector.bounds.x < minX) minX = sector.bounds.x;
            if(sector.bounds.y > maxY) maxY = sector.bounds.y;
            LandPlatform platform = ScriptableObject.CreateInstance<LandPlatform>();
            sector.platform = platform;
            platform.rows = sector.bounds.h / (int)cursor.tileSize;
            platform.columns = sector.bounds.w / (int)cursor.tileSize;
            platform.tilemap = new int[platform.rows * platform.columns];
            platform.rotations = new int[platform.rows * platform.columns];
            for(int i = 0; i < platform.rows * platform.columns; i++) 
            {
                platform.tilemap[i] = -1;
            }

            platform.prefabs = new string[] {
                "New Junction",
                "New 1 Entry",
                "New 2 Entry",
            };
        }

        // set up items and platforms
        int ID = 0;
        Dictionary<Sector, List<Sector.LevelEntity>> sectEnts = new Dictionary<Sector, List<Sector.LevelEntity>>();
        Dictionary<Sector, List<string>> sectTargetIDS = new Dictionary<Sector, List<string>>();
        foreach(var sector in sectors)
        {
            sectEnts.Add(sector, new List<Sector.LevelEntity>());
            sectTargetIDS.Add(sector, new List<string>());
        }

        Dictionary<string, string> itemSectorsByID = new Dictionary<string, string>();
        foreach(var item in items)
        {
            Sector container = GetSurroundingSector(item.pos);
            if(container == null)
            {
                Debug.LogError("No container for item. Abort.");
                return false;
            }
            switch(item.type)
            {
                case ItemType.Platform:
                    var index = GetPlatformIndices(container, item.pos);
                    container.platform.tilemap[index.Item1 * container.platform.columns + index.Item2] = item.placeablesIndex;
                    container.platform.rotations[index.Item1 * container.platform.columns + index.Item2] = ((int)item.obj.transform.rotation.eulerAngles.z / 90) % 4;
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
                                Debug.LogError("Two items in sectors " + container.sectorName + " and " 
                                    + itemSectorsByID[ent.ID] + " were issued the same custom ID. Abort.");
                                return false;
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
                    if((item.isTarget && container.type != Sector.SectorType.SiegeZone) || (container.type == Sector.SectorType.SiegeZone && item.assetID == "outpost_blueprint")) sectTargetIDS[container].Add(ent.ID);
                    if(ent.assetID == "shellcore_blueprint") 
                    {
                        sectTargetIDS[container].Add(ent.ID);
                        ent.blueprintJSON = item.shellcoreJSON;
                    }
                    if(ent.assetID == "trader_blueprint")
                    {
                        ent.blueprintJSON = item.shellcoreJSON;
                    }

                    sectEnts[container].Add(ent);
                    break;   
                default:
                    break;
            }
        }
        
        // write all sectors into a file
		if(!System.IO.Directory.Exists(path)) {
			System.IO.Directory.CreateDirectory(path);
		}
        
        if(System.IO.Directory.Exists(path))
        {
            string[] directories = System.IO.Directory.GetDirectories(path);
            foreach(var dir in directories)
            {
                foreach(var f in System.IO.Directory.GetFiles(dir))
                {
                    System.IO.File.Delete(f);
                }
                System.IO.Directory.Delete(dir);
            }

            string[] files = System.IO.Directory.GetFiles(path);
            foreach(var file in files)
            {
                System.IO.File.Delete(file);
            }
        }

		System.IO.Directory.CreateDirectory(path);

        // create world data
        WorldData wdata = ScriptableObject.CreateInstance<WorldData>();
        wdata.initialSpawn = cursor.spawnPoint.position;
        wdata.defaultCharacters = cursor.characters.ToArray();
        string wdjson = JsonUtility.ToJson(wdata);
        System.IO.File.WriteAllText(path + "\\world.worlddata", wdjson);

        TryCopy(Application.streamingAssetsPath + "\\CanvasPlaceholder", path + "\\Canvases\\");
        TryCopy(Application.streamingAssetsPath + "\\EntityPlaceholder", path + "\\Entities\\");
        TryCopy(Application.streamingAssetsPath + "\\WavePlaceholder", path + "\\Waves\\");

        foreach(var sector in sectors)
        {
            if(sector.sectorName == null || sector.sectorName == "")
            {
                sector.sectorName = GetDefaultName(sector, minX, maxY);
            }

            if(sector.hasMusic && (sector.musicID == null || sector.musicID == ""))
            {
                sector.musicID = GetDefaultMusic(sector);
            }
            
            sector.entities = sectEnts[sector].ToArray();
            sector.targets = sectTargetIDS[sector].ToArray();
            // sector.backgroundColor = SectorColors.colors[(int)sector.type];

            SectorData data = new SectorData();
            data.sectorjson = JsonUtility.ToJson(sector);
            data.platformjson = JsonUtility.ToJson(sector.platform);

            string output = JsonUtility.ToJson(data);

            string sectorPath = path + "\\" + sector.sectorName + ".json";                
            System.IO.File.WriteAllText(sectorPath, output);
        }

		Debug.Log("JSON written to location: " + path);
        return true;
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

    string GetDefaultMusic(Sector sector)
    {
        switch(sector.type)
        {
            case Sector.SectorType.BattleZone:
                return "music_fast";
            case Sector.SectorType.Capitol:
                return "music_funktify"; // Funktify made by Mr Spastic, website - http://www.mrspastic.com
            case Sector.SectorType.SiegeZone:
                return "music_siege_1";
            default:
                return "music_overworld";
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
        Debug.Log(pos + " " + (sector.bounds.y - (int)pos.y) / (int)cursor.tileSize);
        int row = (sector.bounds.y - (int)pos.y) / (int)cursor.tileSize;
        int col = ((int)pos.x - sector.bounds.x) / (int)cursor.tileSize;
        Debug.Log((row, col));
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
                        continue;
                    }

                    if(file.Contains(".taskdata") || file.Contains(".dialoguedata"))
                    {
                        continue;
                    }

                    string sectorjson = System.IO.File.ReadAllText(file);
                    SectorCreatorMouse.SectorData data = JsonUtility.FromJson<SectorCreatorMouse.SectorData>(sectorjson);
                    // Debug.Log("Platform JSON: " + data.platformjson);
                    // Debug.Log("Sector JSON: " + data.sectorjson);
                    Sector curSect = ScriptableObject.CreateInstance<Sector>();
                    JsonUtility.FromJsonOverwrite(data.sectorjson, curSect);
                    LandPlatform plat = ScriptableObject.CreateInstance<LandPlatform>();
                    JsonUtility.FromJsonOverwrite(data.platformjson, plat);
                    plat.name = curSect.name + "Platform";
                    curSect.platform = plat;
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
                                cursor.placedItems.Add(copy);
                            }              
                        }
                    }

                    for(int i = 0; i < plat.rows; i++)
                    {
                        for(int j = 0; j < plat.columns; j++)
                        {
                            int placeablesIndex = plat.tilemap[plat.columns * i + j];
                            foreach(Item item in itemHandler.itemPack.items)
                            {
                                if(item.type == ItemType.Platform && item.placeablesIndex == placeablesIndex)
                                {
                                    Item copy = itemHandler.CopyItem(item);
                                    copy.pos = copy.obj.transform.position 
                                        = new Vector2(cursor.cursorOffset.x + curSect.bounds.x + j * cursor.tileSize, -cursor.cursorOffset.y + curSect.bounds.y - i * cursor.tileSize);
                                    copy.rotation = plat.rotations[plat.columns * i + j];
                                    copy.obj.transform.RotateAround(copy.pos, Vector3.forward, 90 * copy.rotation);
                                    cursor.placedItems.Add(copy);
                                }
                            }
                        }
                    }
                }

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
}
