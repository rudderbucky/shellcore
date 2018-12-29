using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LandPlatformGenerator))]
public class SectorManager : MonoBehaviour
{
    public List<Sector> sectors; //TODO: RM: load sectors from files
    public PlayerCore player;
    public Sector current;
    public BackgroundScript background;
    public InfoText info;

    private Dictionary<int, ICarrier> carriers = new Dictionary<int, ICarrier>();
    private BattleZoneManager battleZone;
    private Dictionary<string, GameObject> objects;
    private LandPlatformGenerator lpg;
    private LineRenderer sectorBorders;

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
        if(current == null || !current.bounds.contains(player.transform.position))
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

    private void Start()
    {
        loadSector();
    }

    void loadSector()
    {
        //unload previous sector
        foreach(var obj in objects)
        {
            if(obj.Value != player.GetTractorTarget().gameObject)
            {
                Destroy(obj.Value);
            }
        }
        objects.Clear();

        //load new sector
        objects.Add("player", player.gameObject);

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
                            gObj.AddComponent<ShellCore>();
                            break;
                        }
                    case EntityBlueprint.IntendedType.PlayerCore:
                        {
                            if (player == null)
                            {
                                player = gObj.AddComponent<PlayerCore>();
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
                            bunker.vendingBlueprint = ResourceManager.GetAsset<VendingBlueprint>(current.entities[i].vendingID); //TODO: RM: load vending blueprints from files
                            break;
                        }
                    case EntityBlueprint.IntendedType.Outpost:
                        {
                            Outpost outpost = gObj.AddComponent<Outpost>();
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
        lpg.Init(current.platform);

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
        }
        else
        {
            battleZone.enabled = false;
        }

        info.showMessage("Entering sector '" + current.sectorName + "'");
    }

}
