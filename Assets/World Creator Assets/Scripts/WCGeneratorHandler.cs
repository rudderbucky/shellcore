using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NodeEditorFramework;
using NodeEditorFramework.IO;
using NodeEditorFramework.Standard;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

    private static string testPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", "TestWorld");
    List<WorldData.PartIndexData> partData = new List<WorldData.PartIndexData>();

    [SerializeField]
    FactionManager factionManager;

    public static void DeleteTestWorld()
    {
        FactionManager.RemoveExtraFactions();
        if (System.IO.Directory.Exists(testPath))
        {
            foreach (var dir in System.IO.Directory.GetDirectories(testPath))
            {
                foreach (var file in System.IO.Directory.GetFiles(dir))
                {
                    System.IO.File.Delete(file);
                }

                System.IO.Directory.Delete(dir);
            }

            foreach (var file in System.IO.Directory.GetFiles(testPath))
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
        if (System.IO.Directory.Exists(path2))
        {
            foreach (var file in System.IO.Directory.GetFiles(path2))
            {
#if UNITY_EDITOR
                if (!file.Contains(".meta"))
                {
                    System.IO.File.Delete(file);
                }
#else
                System.IO.File.Delete(file);
#endif
            }
        }
        else
        {
            System.IO.Directory.CreateDirectory(path2);
        }

        System.IO.Directory.CreateDirectory(path1);
        string[] files = System.IO.Directory.GetFiles(path1);
        foreach (string file in files)
        {
            if (!file.Contains(".meta"))
            {
                System.IO.File.Copy(file, System.IO.Path.Combine(path2, System.IO.Path.GetFileName(file)));
            }
            else
            {
                System.IO.File.Move(file, System.IO.Path.Combine(path2, System.IO.Path.GetFileName(file)));
            }
        }
    }

    List<string> legacyFactionFilesToDelete = new List<string>();

    // This method helps clear out legacy faction files, which were allowed to be anywhere in the world folder.
    // The game does not actually delete the legacy files until the folder is written to (with the appropriate method)
    private void ReadFactionsIntoFactionPlaceholder(string path)
    {
        legacyFactionFilesToDelete.Clear();
        string resourceTxtPath = System.IO.Path.Combine(path, "ResourceData.txt");
        string factionPlaceholderPath = System.IO.Path.Combine(Application.streamingAssetsPath, "FactionPlaceholder");
        if (!Directory.Exists(factionPlaceholderPath))
        {
            Directory.CreateDirectory(factionPlaceholderPath);
        }

        using (StreamReader sr = File.OpenText(resourceTxtPath))
        {
            bool onFactions = false;
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                if (ResourceManager.resourceHeaders.Any(header => s.ToLower().StartsWith(header)))
                {
                    if (s.ToLower().StartsWith("factions:"))
                    {
                        onFactions = true;
                        continue;
                    }
                    else if (onFactions)
                    {
                        break;
                    }
                }

                if (onFactions)
                {
                    string[] names = s.Split(':');
                    string resPath = System.IO.Path.Combine(path, names[1]);

                    // try grabbing the faction name
                    var faction = ScriptableObject.CreateInstance<Faction>();
                    try
                    {
                        JsonUtility.FromJsonOverwrite(File.ReadAllText(resPath), faction);
                    }
                    catch
                    {
                        Debug.LogError("One of your factions is invalid. Abort.");
                        return;
                    }

                    // make sure the faction was not already copied in
                    if (!File.Exists(System.IO.Path.Combine(factionPlaceholderPath, faction.factionName + ".json")))
                    {
                        File.Copy(resPath, System.IO.Path.Combine(factionPlaceholderPath, faction.factionName + ".json"));
                        legacyFactionFilesToDelete.Add(resPath);
                    }
                }
            }
        }
    }

    public void OnNameEdit(string tmpWworldName)
    {
        invalidNameWarning.enabled =
            string.IsNullOrEmpty(tmpWworldName)
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
        var canvasPlaceholderPath = System.IO.Path.Combine(Application.streamingAssetsPath, "CanvasPlaceholder");
        var entityPlaceholderPath = System.IO.Path.Combine(Application.streamingAssetsPath, "EntityPlaceholder");
        var wavePlaceholderPath = System.IO.Path.Combine(Application.streamingAssetsPath, "WavePlaceholder");
        var factionPlaceholderPath = System.IO.Path.Combine(Application.streamingAssetsPath, "FactionPlaceholder");
        var resourcePlaceholderPath = System.IO.Path.Combine(Application.streamingAssetsPath, "ResourcePlaceholder");

        // Reinitialize node editor
        NodeEditor.ReInit(false);

        saveState = 1;
        yield return null;
        sectors = new List<Sector>();
        var items = cursor.placedItems;
        var wrappers = cursor.sectors;
        foreach (var wrapper in wrappers)
        {
            sectors.Add(wrapper.sector);
        }

        int minX = int.MaxValue;
        int maxY = int.MinValue;
        // Get the world bounds
        foreach (var sector in sectors)
        {
            if (sector.bounds.x < minX)
            {
                minX = sector.bounds.x;
            }

            if (sector.bounds.y > maxY)
            {
                maxY = sector.bounds.y;
            }
        }

        // ensure spawn point in some sector
        if (sectors.TrueForAll(sector => !sector.bounds.contains(cursor.spawnPoint.position)))
        {
            Debug.LogError("Spawn point not in sector bounds. Abort.");
            yield break;
        }

        // set up items and platforms
        int ID = 0;
        Dictionary<Sector, List<Sector.LevelEntity>> sectEnts = new Dictionary<Sector, List<Sector.LevelEntity>>();
        Dictionary<Sector, List<string>> sectTargetIDS = new Dictionary<Sector, List<string>>();
        foreach (var sector in sectors)
        {
            sectEnts.Add(sector, new List<Sector.LevelEntity>());
            sectTargetIDS.Add(sector, new List<string>());
            sector.tiles = new List<GroundPlatform.Tile>();
        }

        // Add background spawns to part index
        partData.Clear();
        foreach (var sector in sectors)
        {
            if (sector.backgroundSpawns != null)
            {
                foreach (var spawn in sector.backgroundSpawns)
                {
                    AttemptAddShellCoreParts(spawn.entity, sector.sectorName, path);
                }
            }
        }

        Dictionary<string, string> itemSectorsByID = new Dictionary<string, string>();

        foreach (var item in items)
        {
            Sector container = GetSurroundingSector(item.pos, item.dimension);
            if (container == null)
            {
                savingLevelScreen.SetActive(false);
                saveState = 3;
                Debug.LogError("No container for item. Abort.");
                yield break;
            }

            switch (item.type)
            {
                case ItemType.Platform:
                    var index = GetPlatformIndices(container, item.pos);
                    container.tiles.Add(new GroundPlatform.Tile()
                    {
                        pos = new Vector2Int(index.Item2, index.Item1),
                        type = (byte)item.placeablesIndex,
                        rotation = (byte)(((int)item.obj.transform.rotation.eulerAngles.z / 90) % 4)
                    });
                    break;
                case ItemType.Other:
                case ItemType.Decoration:
                case ItemType.DecorationWithMetadata:
                case ItemType.Flag:
                    Sector.LevelEntity ent = new Sector.LevelEntity();
                    if (cursor.characters.TrueForAll((WorldData.CharacterData x) => { return x.ID != item.ID; }))
                    {
                        // Debug.Log(item.ID + " is not a character. " + ID);
                        if (item.type == ItemType.DecorationWithMetadata)
                        {
                            int parsedId;
                            if (item.assetID == "shard_rock" && int.TryParse(item.ID, out parsedId))
                            {
                                Debug.LogError($"Shard in sector {container.sectorName} has a numeric ID. Abort.");
                                yield break;
                            }

                            ent.blueprintJSON = item.shellcoreJSON;
                        }

                        int test;
                        if (string.IsNullOrEmpty(item.ID) || int.TryParse(item.ID, out test))
                        {
                            ent.ID = (ID++).ToString();
                        }
                        else
                        {
                            ent.ID = item.ID;
                            if (itemSectorsByID.ContainsKey(ent.ID))
                            {
                                savingLevelScreen.SetActive(false);
                                saveState = 4;
                                Debug.LogError($"Two items in sectors {container.sectorName} and {itemSectorsByID[ent.ID]} were issued the same custom ID ({ent.ID}). Abort.");
                                yield break;
                            }
                            else
                            {
                                itemSectorsByID.Add(ent.ID, container.sectorName);
                            }
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
                    if (!string.IsNullOrEmpty(item.name))
                    {
                        ent.name = item.name;
                    }
                    else
                    {
                        ent.name = item.obj.name;
                    }

                    ent.faction = item.faction;
                    ent.position = item.pos;
                    ent.assetID = item.assetID;
                    ent.vendingID = item.vendingID;
                    ent.patrolPath = item.patrolPath;
                    if ((item.isTarget && container.type != Sector.SectorType.SiegeZone)
                        || (container.type == Sector.SectorType.SiegeZone && item.assetID == "outpost_blueprint" && item.faction == 0)
                        || (container.type == Sector.SectorType.SiegeZone && item.assetID == "bunker_blueprint" && item.faction == 0))
                    {
                        sectTargetIDS[container].Add(ent.ID);
                    }

                    var charExists = cursor.characters.Exists(ch => ch.ID == ent.ID);
                    if (ent.assetID == "shellcore_blueprint" || charExists)
                    {
                        if (container.type != Sector.SectorType.SiegeZone && !sectTargetIDS[container].Contains(ent.ID))
                        {
                            sectTargetIDS[container].Add(ent.ID);
                        }

                        ent.blueprintJSON = item.shellcoreJSON;
                        if (!charExists)
                        {
                            AttemptAddShellCoreParts(ent, container.sectorName, path);
                        }
                    }
                    else if (ent.assetID == "trader_blueprint")
                    {
                        ent.blueprintJSON = item.shellcoreJSON;

                        // Attempt to add trader parts into index.
                        if (string.IsNullOrEmpty(ent.blueprintJSON))
                        {
                            var dialogueDataPath = System.IO.Path.Combine(canvasPlaceholderPath, ent.ID, ".dialoguedata");

                            if (System.IO.File.Exists(dialogueDataPath))
                            {
                                var XMLImport = new XMLImportExport();
                                var canvas = XMLImport.Import(dialogueDataPath) as DialogueCanvas;
                                foreach (var node in canvas.nodes)
                                {
                                    if (node is EndDialogue endDialogue && endDialogue.openTrader)
                                    {
                                        ShipBuilder.TraderInventory traderInventory = JsonUtility.FromJson<ShipBuilder.TraderInventory>(endDialogue.traderJSON);
                                        AttemptAddPartArray(traderInventory.parts, container.sectorName);
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
                    else if (ent.assetID == "groundcarrier_blueprint" || ent.assetID == "carrier_blueprint" || ent.assetID == "outpost_blueprint"
                             || ent.assetID == "bunker_blueprint" || ent.assetID == "missile_station" || ent.assetID == "air_weapon_station")
                    {
                        ent.blueprintJSON = item.shellcoreJSON;
                    }

                    sectEnts[container].Add(ent);
                    break;
                default:
                    break;
            }
        }

        if (!System.IO.Directory.Exists(canvasPlaceholderPath))
        {
            System.IO.Directory.CreateDirectory(canvasPlaceholderPath);
        }

        // create world data
        WorldData wdata = ScriptableObject.CreateInstance<WorldData>();
        wdata.sectorMappings = new List<WorldData.OffloadMappings>();
        wdata.dialogueMappings = new List<WorldData.OffloadMappings>();
        // Add reward parts from tasks.
        if (System.IO.Directory.Exists(canvasPlaceholderPath))
        {
            foreach (var canvasPath in System.IO.Directory.GetFiles(canvasPlaceholderPath))
            {
                var pathWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(canvasPath);
                var XMLImport = new XMLImportExport();
                switch (System.IO.Path.GetExtension(canvasPath))
                {
                    case ".taskdata":
                        var questCanvas = XMLImport.Import(canvasPath) as QuestCanvas;

                        string missionName = null;
                        foreach (var node in questCanvas.nodes)
                        {
                            if (node is StartMissionNode startMission)
                            {
                                missionName = startMission.missionName;
                            }
                        }

                        foreach (var node in questCanvas.nodes)
                        {
                            if (node is StartTaskNode startTask && startTask.partReward)
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
                        if (missionName != null)
                        {
                            File.Move(canvasPath, System.IO.Path.Combine(System.IO.Path.GetDirectoryName(canvasPath), missionName + ".taskdata"));
                        }
                        break;
                    case ".sectordata":
                        var sectorCanvas = XMLImport.Import(canvasPath) as SectorCanvas;
                        var sectorName = new SectorTraverser(sectorCanvas).findRoot().sectorName;
                        wdata.sectorMappings.Add(new WorldData.OffloadMappings(sectorName, pathWithoutExtension));
                        //var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(canvasPath), sectorName + ".sectordata");
                        //if (!File.Exists(newPath))
                        //    File.Move(canvasPath, newPath);
                        break;
                    case ".dialoguedata":
                        var dialogueCanvas = XMLImport.Import(canvasPath) as DialogueCanvas;
                        var entityID = new DialogueTraverser(dialogueCanvas).findRoot().EntityID;
                        wdata.dialogueMappings.Add(new WorldData.OffloadMappings(entityID, pathWithoutExtension));

                        //File.Move(canvasPath, System.IO.Path.Combine(System.IO.Path.GetDirectoryName(canvasPath), entityID + ".dialoguedata"));
                        break;
                }
            }
        }

        // try to write out resources. Factions are obtained from the FactionManager
        if (!System.IO.Directory.Exists(factionPlaceholderPath))
        {
            System.IO.Directory.CreateDirectory(factionPlaceholderPath);
        }

        var resourceTxtPath = System.IO.Path.Combine(Application.streamingAssetsPath, "ResourceDataPlaceholder.txt");
        if (System.IO.File.Exists(resourceTxtPath))
        {
            // first, extract all the lines without the factions.
            List<string> lines = new List<string>();
            using (StreamReader sr = File.OpenText(resourceTxtPath))
            {
                string s;
                bool onFactions = false;
                while ((s = sr.ReadLine()) != null)
                {
                    if (ResourceManager.resourceHeaders.Any(header => s.ToLower().StartsWith(header)))
                    {
                        if (s.ToLower().StartsWith("factions:"))
                        {
                            onFactions = true;
                        }
                        else
                        {
                            onFactions = false;
                        }
                    }

                    if (!onFactions)
                    {
                        lines.Add(s);
                    }
                }
            }

            //  we then reconstruct the factions tab with FM data
            lines.Add("factions:");
            foreach (var faction in factionManager.factions)
            {
                // avoid default factions
                if (FactionManager.defaultFactions.Contains(faction))
                {
                    continue;
                }

                lines.Add($"{faction.factionName}:Factions/{faction.factionName}.json");
            }

            File.WriteAllLines(resourceTxtPath, lines);
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
        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }

        // Delete all unnecessary files
        if (System.IO.Directory.Exists(path))
        {
            string[] resPaths = ResourceManager.Instance.GetFileNames(path);

            for (int i = 0; i < resPaths.Length; i++)
            {
                resPaths[i] = resPaths[i].Replace('\\', '/');
                Debug.Log("Res path: " + resPaths[i]);
            }

            string[] directories = System.IO.Directory.GetDirectories(path);
            foreach (var dir in directories)
            {
                bool del = true;
                foreach (var f in System.IO.Directory.GetFiles(dir))
                {
                    Debug.Log("File in dir: " + System.IO.Path.Combine(dir, f));
                    if (!resPaths.Contains(System.IO.Path.Combine(dir, f).Replace('\\', '/')))
                    {
                        System.IO.File.Delete(f);
                    }

                    del = false;
                }

                if (del)
                {
                    System.IO.Directory.Delete(dir);
                }
            }

            string[] files = System.IO.Directory.GetFiles(path);
            foreach (var file in files)
            {
                string f = file.Replace('\\', '/');
                if ((!resPaths.Contains(f) && f != System.IO.Path.Combine(path, "ResourceData.txt").Replace('\\', '/'))
                    || legacyFactionFilesToDelete.Contains(file))
                {
                    System.IO.File.Delete(file);
                }
            }
        }

        wdata.initialSpawn = cursor.spawnPoint.position;
        wdata.defaultCharacters = cursor.characters.ToArray();
        wdata.defaultBlueprintJSON = blueprintField.text;
        wdata.author = authorField.text;
        wdata.description = descriptionField.text;
        wdata.partIndexDataArray = partData.ToArray();

        string wdjson = JsonUtility.ToJson(wdata);
        System.IO.File.WriteAllText(System.IO.Path.Combine(path, "world.worlddata"), wdjson);
        if (File.Exists(System.IO.Path.Combine(path, "ResourceData.txt")))
        {
            File.Delete(System.IO.Path.Combine(path, "ResourceData.txt"));
        }

        if (File.Exists(resourceTxtPath))
        {
            File.Copy(resourceTxtPath, System.IO.Path.Combine(path, "ResourceData.txt"));
        }

        TryCopy(canvasPlaceholderPath, System.IO.Path.Combine(path, "Canvases"));
        TryCopy(entityPlaceholderPath, System.IO.Path.Combine(path, "Entities"));
        TryCopy(wavePlaceholderPath, System.IO.Path.Combine(path, "Waves"));
        TryCopy(factionPlaceholderPath, System.IO.Path.Combine(path, "Factions"));
        TryCopy(resourcePlaceholderPath, System.IO.Path.Combine(path, "Resources"));

        foreach (var sector in sectors)
        {
            if (string.IsNullOrEmpty(sector.sectorName))
            {
                sector.sectorName = GetDefaultName(sector, minX, maxY);
            }

            if (sector.hasMusic && string.IsNullOrEmpty(sector.musicID))
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

            string sectorPath = System.IO.Path.Combine(path, sector.sectorName + ".json");
            System.IO.File.WriteAllText(sectorPath, output);
        }

        Debug.Log("JSON written to location: " + path);
        Debug.Log($"Index size: {partData.Count}");
        savingLevelScreen.SetActive(false);
        saveState = 2;
        if (OnSectorSaved != null)
        {
            OnSectorSaved.Invoke();
        }
    }

    string GetDefaultName(Sector sector, int minX, int maxY)
    {
        int x = sector.bounds.x - minX;
        int y = maxY - sector.bounds.y;
        string typeRep;
        switch (sector.type)
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

        return $"{typeRep} {x}-{y}{(sector.dimension > 0 ? $" - Dimension {sector.dimension}" : "")}";
    }

    public static string GetDefaultMusic(Sector.SectorType type)
    {
        switch (type)
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

    Sector GetSurroundingSector(Vector2 pos, int dim)
    {
        foreach (var sector in sectors)
        {
            if (sector.bounds.contains(pos) && sector.dimension == dim)
            {
                return sector;
            }
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
            System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors"), "DefaultWorldName");
        WriteWorld(str);
#endif
    }

    public void ReadWorldFromEditorPrompt()
    {
#if UNITY_EDITOR
        var str = UnityEditor.EditorUtility.OpenFolderPanel("Read World (Folder)", System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors"), "");
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
                // resource pack loading
                if (!ResourceManager.Instance.LoadResources(path) && SectorManager.testResourcePath != null)
                {
                    ResourceManager.Instance.LoadResources(SectorManager.testResourcePath);
                }

                // copying canvases
                TryCopy(System.IO.Path.Combine(path, "Canvases"), System.IO.Path.Combine(Application.streamingAssetsPath, "CanvasPlaceholder"));

                // copying entities
                TryCopy(System.IO.Path.Combine(path, "Entities"), System.IO.Path.Combine(Application.streamingAssetsPath, "EntityPlaceholder"));

                // copying waves
                TryCopy(System.IO.Path.Combine(path, "Waves"), System.IO.Path.Combine(Application.streamingAssetsPath, "WavePlaceholder"));

                // copying factions
                TryCopy(System.IO.Path.Combine(path, "Factions"), System.IO.Path.Combine(Application.streamingAssetsPath, "FactionPlaceholder"));

                // copying resources
                TryCopy(System.IO.Path.Combine(path, "Resources"), System.IO.Path.Combine(Application.streamingAssetsPath, "ResourcePlaceholder"));

                var resourcePlaceholderPath = System.IO.Path.Combine(Application.streamingAssetsPath, "ResourceDataPlaceholder.txt");
                if (File.Exists(resourcePlaceholderPath))
                {
                    File.Delete(resourcePlaceholderPath);
                }

                // reading sectors
                string[] files = System.IO.Directory.GetFiles(path);

                cursor.placedItems = new List<Item>();
                cursor.sectors = new List<WorldCreatorCursor.SectorWCWrapper>();
                cursor.DimensionCount = 1;

                foreach (string file in files)
                {
                    if (file.Contains(".meta") || file.Contains(".DS_Store"))
                    {
                        continue;
                    }

                    // parse world data
                    if (file.Contains(".worlddata"))
                    {
                        string worlddatajson = System.IO.File.ReadAllText(file);
                        WorldData wdata = ScriptableObject.CreateInstance<WorldData>();
                        JsonUtility.FromJsonOverwrite(worlddatajson, wdata);
                        cursor.spawnPoint.position = wdata.initialSpawn;
                        // add characters into character handler
                        foreach (var ch in wdata.defaultCharacters)
                        {
                            cursor.characters.Add(ch);
                        }

                        blueprintField.text = wdata.defaultBlueprintJSON;
                        // authorField.text = wdata.author;
                        // descriptionField.text = wdata.description;
                        continue;
                    }

                    if (file.Contains(".taskdata") || file.Contains(".dialoguedata") || file.Contains(".sectordata"))
                    {
                        continue;
                    }

                    if (file.Contains("ResourceData.txt"))
                    {
                        File.Copy(file, System.IO.Path.Combine(Application.streamingAssetsPath, "ResourceDataPlaceholder.txt"), true);
                        ReadFactionsIntoFactionPlaceholder(path);
                        continue;
                    }

                    if (ResourceManager.fileNames.Contains(file))
                    {
                        continue;
                    }

                    string sectorjson = System.IO.File.ReadAllText(file);
                    Sector.SectorData data = JsonUtility.FromJson<Sector.SectorData>(sectorjson);
                    // Debug.Log("Platform JSON: " + data.platformjson);
                    // Debug.Log("Sector JSON: " + data.sectorjson);
                    Sector curSect = ScriptableObject.CreateInstance<Sector>();
                    JsonUtility.FromJsonOverwrite(data.sectorjson, curSect);

                    cursor.DimensionCount = Mathf.Max(cursor.DimensionCount, curSect.dimension + 1);

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
                                        copy.dimension = curSect.dimension;
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
                                        copy.dimension = curSect.dimension;
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

                    foreach (Sector.LevelEntity ent in curSect.entities)
                    {
                        foreach (Item item in itemHandler.itemPack.items)
                        {
                            if (ent.assetID == item.assetID && ent.assetID != "")
                            {
                                Item copy = itemHandler.CopyItem(item);
                                copy.dimension = curSect.dimension;
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

                ImportExportFormat.RuntimeIOPath = System.IO.Path.Combine(Application.streamingAssetsPath, "CanvasPlaceholder");
                Debug.Log("World loaded");
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }

            ;
            Input.ResetInputAxes(); // clear the copy paste ctrl press if there was one
        }
    }

    public void AttemptAddPartArray(List<EntityBlueprint.PartInfo> parts, string sectorName)
    {
        foreach (var part in parts)
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
                (System.IO.Path.Combine(Application.streamingAssetsPath, "EntityPlaceholder", entity.blueprintJSON + ".json")), blueprint);
        }


        if (blueprint.intendedType == EntityBlueprint.IntendedType.ShellCore && entity.faction != 0)
        {
            if (blueprint.parts != null)
            {
                foreach (var part in blueprint.parts)
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
        if (data == null)
        {
            data = new WorldData.PartIndexData();
            data.part = part;
            data.origins = new List<string>();
            partData.Add(data);
        }

        if (!data.origins.Contains(origin))
        {
            data.origins.Add(origin);
        }
    }
}
