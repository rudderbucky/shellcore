using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WCGeneratorHandler : MonoBehaviour
{
    public struct SectorData {
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
    public void WriteWorld() 
    {
        string wName = worldName.text;
        if(wName == null || wName == "")
        {
            Debug.Log("Name your damn world!");
            return;
        }

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

        foreach(var item in items)
        {
            Sector container = GetSurroundingSector(item.pos);
            if(container == null)
            {
                Debug.Log("No container for item. Abort.");
                return;
            }
            switch(item.type)
            {
                case ItemType.Platform:
                    var index = GetPlatformIndices(container, item.pos);
                    container.platform.tilemap[index.Item1 * container.platform.columns + index.Item2] = item.placeablesIndex;
                    container.platform.rotations[index.Item1 * container.platform.columns + index.Item2] = ((int)item.obj.transform.rotation.eulerAngles.z / 90) % 4;
                    break;
                case ItemType.Other:
                    Sector.LevelEntity ent = new Sector.LevelEntity();
                    ent.ID = ID++ + "";
                    ent.faction = item.faction;
                    ent.position = item.pos;
                    ent.assetID = item.assetID;
                    ent.vendingID = item.vendingID;
                    if(item.isTarget) sectTargetIDS[container].Add(ent.ID);
                    ent.name = item.obj.name;
                    if(ent.assetID == "shellcore_blueprint") {
                        sectTargetIDS[container].Add(ent.ID);
                        ent.blueprintJSON = item.shellcoreJSON;
                    }
                    sectEnts[container].Add(ent);
                    break;
                default:
                    break;
            }
        }
        
        // write all sectors into a file
		if(!System.IO.Directory.Exists(Application.streamingAssetsPath + "\\Sectors\\")) {
			System.IO.Directory.CreateDirectory(Application.streamingAssetsPath + "\\Sectors\\");
		}

		System.IO.Directory.CreateDirectory(Application.streamingAssetsPath + "\\Sectors\\" + wName);

        // create world data
        WorldData wdata = ScriptableObject.CreateInstance<WorldData>();
        wdata.defaultCharacters = cursor.characters.ToArray();
        string wdjson = JsonUtility.ToJson(wdata);
        System.IO.File.WriteAllText(Application.streamingAssetsPath + "\\Sectors\\" + wName + "\\" + wName + ".worlddata", wdjson);
        nodeEditor.ExportData(Application.streamingAssetsPath + "\\Sectors\\" + wName + "\\" + wName + ".taskdata");        

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
            sector.backgroundColor = SectorColors.colors[(int)sector.type];

            SectorData data = new SectorData();
            data.sectorjson = JsonUtility.ToJson(sector);
            data.platformjson = JsonUtility.ToJson(sector.platform);

            string output = JsonUtility.ToJson(data);

            string path = Application.streamingAssetsPath + "\\Sectors\\" + wName + "\\" + sector.sectorName + ".json";
            System.IO.File.WriteAllText(path, output);
        }

		Debug.Log("JSON written to location: " + Application.streamingAssetsPath + "\\Sectors\\" + wName);
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
        int row = sector.platform.rows - 1 - ((int)pos.y - sector.bounds.y) / (int)cursor.tileSize;
        int col = ((int)pos.x - sector.bounds.x) / (int)cursor.tileSize;

        return (row, col);
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

                        // add characters into character handler
                        foreach(var ch in wdata.defaultCharacters)
                        {
                            characterHandler.AddCharacter(ch);
                        }
                        continue;
                    }

                    string sectorjson = System.IO.File.ReadAllText(file);
                    SectorCreatorMouse.SectorData data = JsonUtility.FromJson<SectorCreatorMouse.SectorData>(sectorjson);
                    Debug.Log("Platform JSON: " + data.platformjson);
                    Debug.Log("Sector JSON: " + data.sectorjson);
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
                            new Vector2(curSect.bounds.x, curSect.bounds.y + curSect.bounds.h),
                            new Vector2(curSect.bounds.x + curSect.bounds.w, curSect.bounds.y + curSect.bounds.h),
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
                            if(ent.assetID == item.assetID)
                            {
                                Item copy = itemHandler.CopyItem(item);
                                copy.faction = ent.faction;
                                copy.ID = ent.ID;
                                copy.name = ent.name;
                                copy.pos = copy.obj.transform.position = ent.position;
                                copy.vendingID = ent.vendingID;
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
                                        = new Vector2(cursor.cursorOffset.x + curSect.bounds.x + j * cursor.tileSize, -cursor.cursorOffset.y + curSect.bounds.y + curSect.bounds.h - i * cursor.tileSize);
                                    copy.rotation = plat.rotations[plat.columns * i + j];
                                    copy.obj.transform.RotateAround(copy.pos, Vector3.forward, 90 * copy.rotation);
                                    cursor.placedItems.Add(copy);
                                }
                            }
                        }
                    }
                }
                Debug.Log("worked");
                return;
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            };
        }
    }
}
