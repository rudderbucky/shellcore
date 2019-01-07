using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LandPlatformGenerator))]
public class SectorManager : MonoBehaviour
{
    public bool jsonMode;
    public List<Sector> sectors; //TODO: RM: load sectors from files
    public PlayerCore player;
    public Sector current;
    public BackgroundScript background;
    public InfoText info;

    private Dictionary<int, int> stationsCount = new Dictionary<int, int>();
    private Dictionary<int, ICarrier> carriers = new Dictionary<int, ICarrier>();
    private List<IVendor> stations = new List<IVendor>();
    private BattleZoneManager battleZone;
    private Dictionary<string, GameObject> objects;
    private LandPlatformGenerator lpg;
    private LineRenderer sectorBorders;

    public int GetExtraCommandUnits(int faction) {
        stationsCount.Clear();
        foreach(IVendor vendor in stations)
        {
            int stationFaction = (vendor as Entity).faction;
            if(!stationsCount.ContainsKey(stationFaction))
            {
                stationsCount.Add(stationFaction, 0);
            }
            stationsCount[stationFaction]++;
        }
        return stationsCount.ContainsKey(faction) ? stationsCount[faction] * 3 : 0; 
    }

    private void Awake()
    {
        objects = new Dictionary<string, GameObject>();
        battleZone = gameObject.AddComponent<BattleZoneManager>();
        lpg = GetComponent<LandPlatformGenerator>();

        sectorBorders = new GameObject("SectorBorders").AddComponent<LineRenderer>();
        sectorBorders.positionCount = 4;
        sectorBorders.startWidth = 0.1f;
        sectorBorders.endWidth = 0.1f;
        sectorBorders.material = ResourceManager.GetAsset<Material>("white_material");
        sectorBorders.loop = true;
    }

    private void Update()
    {
        if(!jsonMode && (current == null || !current.bounds.contains(player.transform.position)))
        {
            // load sector
            for(int i = 0; i < sectors.Count; i++)
            {
                if(sectors[i].bounds.contains(player.transform.position))
                {
                    current = sectors[i];
                    loadSector();
                    break;
                }
            }
        }
    }

    public void UpdateStations()
    {
        
    }

    private void Start()
    {
            if(jsonMode) {
                string sectorfile = System.IO.Directory.GetFiles(Application.dataPath + "\\..\\Sectors\\")[0];
                string sectorjson = System.IO.File.ReadAllText(sectorfile);
                SectorCreatorMouse.SectorData data = JsonUtility.FromJson<SectorCreatorMouse.SectorData>(sectorjson);
                Debug.Log("Platform JSON: " + data.platformjson);
                LandPlatformDataWrapper platform = JsonUtility.FromJson<LandPlatformDataWrapper>(data.platformjson);
                Debug.Log("Sector JSON: " + data.sectorjson);
                SectorDataWrapper sector = JsonUtility.FromJson<SectorDataWrapper>(data.sectorjson);
                Sector curSect = ScriptableObject.CreateInstance<Sector>();
                curSect.SetViaWrapper(sector);
                LandPlatform plat = ScriptableObject.CreateInstance<LandPlatform>();
                plat.SetViaWrapper(platform);
                curSect.platform = plat;
                current = curSect;
                loadSector();
            } else loadSector();
    }

    void loadSector()
    {
        //unload previous sector
        foreach(var obj in objects)
        {
            if(player.GetTractorTarget() && obj.Value != player.GetTractorTarget().gameObject)
            {
                Destroy(obj.Value);
            }
        }
        objects.Clear();

        // reset stations and carriers

        stations.Clear();
        carriers.Clear();

        //load new sector
        objects.Add("player", player.gameObject);
        player.sectorMngr = this;


        for(int i = 0; i < current.entities.Length; i++)
        {
            Object obj = ResourceManager.GetAsset<Object>(current.entities[i].assetID);

            if(obj is GameObject)
            {
                GameObject gObj = Instantiate(obj as GameObject);
                gObj.transform.position = current.entities[i].position;
                gObj.name = current.entities[i].name;
                objects.Add(current.entities[i].ID, gObj);
            }
            else if(obj is EntityBlueprint)
            {
                GameObject gObj = new GameObject(current.entities[i].name);
                EntityBlueprint blueprint = obj as EntityBlueprint;
                switch (blueprint.intendedType)
                {
                    case EntityBlueprint.IntendedType.ShellCore:
                        {
                            ShellCore shellcore = gObj.AddComponent<ShellCore>();
                            shellcore.sectorMngr = this;
                            break;
                        }
                    case EntityBlueprint.IntendedType.PlayerCore:
                        {
                            if (player == null)
                            {
                                player = gObj.AddComponent<PlayerCore>();
                                player.sectorMngr = this;
                            }
                            else
                            {
                                Destroy(gObj);
                                continue;
                            }

                            break;
                        }
                    case EntityBlueprint.IntendedType.Turret:
                        {
                            gObj.AddComponent<Turret>();
                            break;
                        }
                    case EntityBlueprint.IntendedType.Tank:
                        {
                            gObj.AddComponent<Tank>();
                            break;
                        }
                    case EntityBlueprint.IntendedType.Bunker:
                        {
                            Bunker bunker = gObj.AddComponent<Bunker>();
                            stations.Add(bunker);
                            bunker.vendingBlueprint = ResourceManager.GetAsset<VendingBlueprint>(current.entities[i].vendingID); //TODO: RM: load vending blueprints from files
                            break;
                        }
                    case EntityBlueprint.IntendedType.Outpost:
                        {
                            Outpost outpost = gObj.AddComponent<Outpost>();
                            stations.Add(outpost);
                            outpost.vendingBlueprint = ResourceManager.GetAsset<VendingBlueprint>(current.entities[i].vendingID); //TODO: RM: load vending blueprints from files
                            break;
                        }
                    case EntityBlueprint.IntendedType.Tower:
                        {
                            break;
                        }
                    case EntityBlueprint.IntendedType.Drone:
                        {
                            Drone drone = gObj.AddComponent<Drone>();
                            drone.path = ResourceManager.GetAsset<Path>(current.entities[i].pathID); //TODO: RM: load paths from files
                            break;
                        }
                    case EntityBlueprint.IntendedType.AirCarrier:
                        AirCarrier carrier = gObj.AddComponent<AirCarrier>();
                        if(!carriers.ContainsKey(current.entities[i].faction))
                        {
                            carriers.Add(current.entities[i].faction, carrier);
                        }
                        carrier.sectorMngr = this;
                        break;
                    default:
                        break;
                }
                Entity entity = gObj.GetComponent<Entity>();
                entity.faction = current.entities[i].faction;
                entity.spawnPoint = current.entities[i].position;
                entity.blueprint = blueprint;
                if (current.entities[i].dialogueID != "")
                {
                    entity.dialogue = ResourceManager.GetAsset<Dialogue>(current.entities[i].dialogueID);
                }

                objects.Add(current.entities[i].ID, gObj);
            }
        }

        //land platforms
        lpg.SetColor(current.backgroundColor + new Color(0.5F, 0.5F, 0.5F));
        var tiles = new GameObject[1];
        tiles[0] = ResourceManager.GetAsset<GameObject>("4entry");
        current.platform.prefabs = tiles;
        lpg.BuildTiles(current.platform);

        //sector color
        background.setColor(current.backgroundColor);

        //sector borders
        sectorBorders.SetPositions(new Vector3[]{
            new Vector3(current.bounds.x, current.bounds.y, 0),
            new Vector3(current.bounds.x + current.bounds.w, current.bounds.y, 0),
            new Vector3(current.bounds.x + current.bounds.w, current.bounds.y + current.bounds.h, 0),
            new Vector3(current.bounds.x, current.bounds.y + current.bounds.h, 0)
        });

        //battle zone things
        if (current.type == Sector.SectorType.BattleZone)
        {
            battleZone.enabled = true;
            var playerComp = player.GetComponent<PlayerCore>();
            battleZone.AddTarget(playerComp);
            playerComp.SetCarrier(carriers[playerComp.faction]);
            for (int i = 0; i < current.targets.Length; i++)
            {
                if(objects[current.targets[i]].GetComponent<ShellCore>())
                {
                    // set the carrier of the shellcore to the associated faction's carrier
                    ShellCore shellcore = objects[current.targets[i]].GetComponent<ShellCore>();
                    shellcore.SetCarrier(carriers[shellcore.faction]);
                }
                battleZone.AddTarget(objects[current.targets[i]].GetComponent<Entity>());
            }
            battleZone.UpdateCounters();
        }
        else
        {
            battleZone.enabled = false;
        }

        if(info) info.showMessage("Entering sector '" + current.sectorName + "'");
    }
}
