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

    public InputField worldName;
    void WriteWorld() 
    {
        string wName = worldName.text;
        if(wName == null || wName == "")
        {
            Debug.Log("Name your damn world!");
            return;
        }

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

        foreach(var sector in sectors)
        {
            var intermediateName = "";
            if(sector.sectorName != null && sector.sectorName != "")
            {
                intermediateName = sector.sectorName;
            }
            else
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

                intermediateName = typeRep + " " + x + "-" + y;
                sector.sectorName = intermediateName;
            }

            //sct.entities = ents.ToArray();
            //sct.targets = targetIDS.ToArray();
            sector.backgroundColor = SectorColors.colors[(int)sector.type];

            SectorData data = new SectorData();
            data.sectorjson = JsonUtility.ToJson(sector);
            data.platformjson = JsonUtility.ToJson(sector.platform);

            string output = JsonUtility.ToJson(data);

            string path = Application.streamingAssetsPath + "\\Sectors\\" + wName + "\\" + intermediateName;
            System.IO.File.WriteAllText(path, output);
            System.IO.Path.ChangeExtension(path, ".json");
        }



		Debug.Log("JSON written to location: " + Application.streamingAssetsPath + "\\Sectors\\" + wName);
    }

    void Update() 
    {
        if(Input.GetKeyUp(KeyCode.X))
        {
            WriteWorld();
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
    void ReadWorld() 
    {

    }
}
