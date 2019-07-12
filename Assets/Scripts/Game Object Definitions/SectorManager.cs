using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

[RequireComponent(typeof(LandPlatformGenerator))]
public class SectorManager : MonoBehaviour
{
    public delegate void SectorLoadDelegate(string sectorName);
    public static SectorLoadDelegate OnSectorLoad;
    public static SectorManager instance;

    public bool jsonMode;
    public List<Sector> sectors; //TODO: RM: load sectors from files (already done elsewhere; would it make sense to move it to RM?)
    public PlayerCore player;
    public Sector current;
    public BackgroundScript background;
    public InfoText info;
    public List<ShellPart> strayParts = new List<ShellPart>();

    private Dictionary<int, int> stationsCount = new Dictionary<int, int>();
    private Dictionary<int, ICarrier> carriers = new Dictionary<int, ICarrier>();
    private List<IVendor> stations = new List<IVendor>();
    private BattleZoneManager battleZone;
    private Dictionary<string, GameObject> objects;
    private Dictionary<string, GameObject> persistentObjects;
    private LandPlatformGenerator lpg;
    private LineRenderer sectorBorders;
    private int uniqueIDInt;

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
        if (instance != null)
        {
            Debug.LogWarning("There should be only one sector manager!");
            Destroy(gameObject);
            return;
        }
        instance = this;

        objects = new Dictionary<string, GameObject>();
        persistentObjects = new Dictionary<string, GameObject>();
        battleZone = gameObject.AddComponent<BattleZoneManager>();
        lpg = GetComponent<LandPlatformGenerator>();
        sectorBorders = new GameObject("SectorBorders").AddComponent<LineRenderer>();
        sectorBorders.enabled = false;
        sectorBorders.positionCount = 4;
        sectorBorders.startWidth = 0.1f;
        sectorBorders.endWidth = 0.1f;
        sectorBorders.loop = true;
    }

    private void Update()
    {
        if(jsonMode) player.SetIsInteracting(true);
        if(!jsonMode && player && (current == null || !current.bounds.contains(player.transform.position)))
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

    public void TryGettingJSON() {
        string path = GameObject.Find("Path Input").GetComponent<UnityEngine.UI.InputField>().text;
        GameObject.Find("Path Input").transform.parent.gameObject.SetActive(false);
        if(System.IO.Directory.Exists(path)) {
            try {
            string[] files = Directory.GetFiles(path);
            current = null;
            sectors = new List<Sector>();
            foreach (string file in files)
            {
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
                sectors.Add(curSect);
            }
            Debug.Log("worked");
            jsonMode = false;
            return;
            } catch(System.Exception){
            };
        }
        else if(System.IO.File.Exists(path)) {
            try {
                string sectorjson = System.IO.File.ReadAllText(path);
                SectorCreatorMouse.SectorData data = JsonUtility.FromJson<SectorCreatorMouse.SectorData>(sectorjson);
                Debug.Log("Platform JSON: " + data.platformjson);
                Debug.Log("Sector JSON: " + data.sectorjson);
                Sector curSect = ScriptableObject.CreateInstance<Sector>();
                JsonUtility.FromJsonOverwrite(data.sectorjson, curSect);
                LandPlatform plat = ScriptableObject.CreateInstance<LandPlatform>();
                JsonUtility.FromJsonOverwrite(data.platformjson, plat);
                plat.name = curSect.name + "Platform";
                curSect.platform = plat;
                current = curSect;
                sectors = new List<Sector>();
                sectors.Add(curSect);
                Debug.Log("Success! File loaded from " + path);
                jsonMode = false;
                player.SetIsInteracting(false);
                loadSector();
                return;
            } catch(System.Exception e) {
                Debug.Log(e);
            }
        } 
        jsonMode = false;
        player.SetIsInteracting(false);
        loadSector();
    }
    private void Start()
    {
        if(ResourceManager.Instance)sectorBorders.material = ResourceManager.GetAsset<Material>("white_material");
        background.setColor(SectorColors.colors[4]);
        if(!jsonMode) loadSector();
    }

    public Entity SpawnEntity(EntityBlueprint blueprint, Sector.LevelEntity data)
    {
        GameObject gObj = new GameObject(data.name);
        switch (blueprint.intendedType)
        {
            case EntityBlueprint.IntendedType.ShellCore:
                {
                    ShellCore shellcore = gObj.AddComponent<ShellCore>();
                    try
                    {
                        string json = data.blueprintJSON;
                        if (json != null && json != "")
                        {
                            blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
                            JsonUtility.FromJsonOverwrite(json, blueprint);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log(e.Message);
                        //blueprint = obj as EntityBlueprint;
                    }
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
                        return null;
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
                    bunker.vendingBlueprint =
                        blueprint.dialogue != null
                        ? blueprint.dialogue.vendingBlueprint
                        : ResourceManager.GetAsset<VendingBlueprint>(data.vendingID);
                    break;
                }
            case EntityBlueprint.IntendedType.Outpost:
                {
                    Outpost outpost = gObj.AddComponent<Outpost>();
                    stations.Add(outpost);
                    outpost.vendingBlueprint = 
                        blueprint.dialogue != null 
                        ? blueprint.dialogue.vendingBlueprint 
                        : ResourceManager.GetAsset<VendingBlueprint>(data.vendingID);
                    break;
                }
            case EntityBlueprint.IntendedType.Tower:
                {
                    break;
                }
            case EntityBlueprint.IntendedType.Drone:
                {
                    Drone drone = gObj.AddComponent<Drone>();
                    drone.path = ResourceManager.GetAsset<Path>(data.pathID);
                    break;
                }
            case EntityBlueprint.IntendedType.AirCarrier:
                AirCarrier carrier = gObj.AddComponent<AirCarrier>();
                if (!carriers.ContainsKey(data.faction))
                {
                    carriers.Add(data.faction, carrier);
                }
                carrier.sectorMngr = this;
                break;
            case EntityBlueprint.IntendedType.GroundCarrier:
                GroundCarrier gcarrier = gObj.AddComponent<GroundCarrier>();
                if (!carriers.ContainsKey(data.faction))
                {
                    carriers.Add(data.faction, gcarrier);
                }
                gcarrier.sectorMngr = this;
                break;
            case EntityBlueprint.IntendedType.Yard:
                Yard yard = gObj.AddComponent<Yard>();
                yard.mode = BuilderMode.Yard;
                break;
            case EntityBlueprint.IntendedType.WeaponStation:
                gObj.AddComponent<WeaponStation>();
                break;
            case EntityBlueprint.IntendedType.CoreUpgrader:
                gObj.AddComponent<CoreUpgrader>();
                break;
            case EntityBlueprint.IntendedType.Trader:
                Yard trade = gObj.AddComponent<Yard>();
                trade.mode = BuilderMode.Trader;
                blueprint.dialogue.traderInventory =
                    JsonUtility.FromJson<List<EntityBlueprint.PartInfo>>(data.blueprintJSON);
                break;
            case EntityBlueprint.IntendedType.DroneWorkshop:
                Yard workshop = gObj.AddComponent<Yard>();
                workshop.mode = BuilderMode.Workshop;
                break;
            default:
                break;
        }
        Entity entity = gObj.GetComponent<Entity>();
        entity.sectorMngr = this;
        entity.faction = data.faction;
        entity.spawnPoint = data.position;
        entity.blueprint = blueprint;

        if(data.ID == "" || data.ID == null)
        {
            data.ID = objects.Count.ToString();
        }
        entity.ID = data.ID;

        // TODO:
        // I think we should move dialogue setting to BuildEntity() since each entity's
        // dialogue should vary in the blueprint rather than using the resource manager
        // since that allows for more custom dialogue using the sector creator
        // (I already sort of did this but didn't remove the setting here)
        if (data.dialogueID != "")
        {
            entity.dialogue = ResourceManager.GetAsset<Dialogue>(data.dialogueID);
        }

        objects.Add(data.ID, gObj);
        return entity;
    }

    void loadSector()
    {
        #if UNITY_EDITOR
        if(Input.GetKey(KeyCode.LeftShift)) {
            SectorCreatorMouse.SectorData data = new SectorCreatorMouse.SectorData();
            data.platformjson = JsonUtility.ToJson(current.platform);
            data.sectorjson = JsonUtility.ToJson(current);
            current.name = "SavedSector";
            current.platform.name = "SavedSectorPlatform";
            UnityEditor.AssetDatabase.CreateAsset(current, "Assets/SavedSector.asset");
            UnityEditor.AssetDatabase.CreateAsset(current.platform, "Assets/SavedSectorPlatform.asset");
        }
        #endif

        //unload previous sector
        foreach(var obj in objects)
        {
            if(player && (!player.GetTractorTarget() || (player.GetTractorTarget() && obj.Value != player.GetTractorTarget().gameObject))
                && obj.Value != player.gameObject)
            {
                Destroy(obj.Value);
            }
        }

        Dictionary<string, GameObject> tmp = new Dictionary<string, GameObject>();
        foreach(var obj in persistentObjects)
        {
            if(player && (!player.GetTractorTarget() || (player.GetTractorTarget() && obj.Value != player.GetTractorTarget().gameObject))
                && obj.Value != player.gameObject)
            {
                Destroy(obj.Value);
            } else tmp.Add(obj.Key, obj.Value);
        }

        persistentObjects = tmp;

        foreach(ShellPart part in strayParts) {
            if(part && !(player && player.GetTractorTarget() && player.GetTractorTarget().GetComponent<ShellPart>() == part)) {
                Destroy(part.gameObject);
            }
        }
        strayParts.Clear(); 

        // Add the player's tractored part back so it gets deleted if the player doesn't tractor it through
        // to another sector
        if((player && player.GetTractorTarget() && player.GetTractorTarget().GetComponent<ShellPart>()))
            strayParts.Add(player.GetTractorTarget().GetComponent<ShellPart>());
        objects.Clear();

        // reset stations and carriers

        stations.Clear();
        carriers.Clear();

        //load new sector
        if(player) {
            player.ResetPower();
            objects.Add("player", player.gameObject);
            player.sectorMngr = this;
            player.alerter.showMessage("ENTERING SECTOR: " + current.sectorName);
        }


        for(int i = 0; i < current.entities.Length; i++)
        {
            Object obj = ResourceManager.GetAsset<Object>(current.entities[i].assetID);

            if(obj is GameObject)
            {
                GameObject gObj = Instantiate(obj as GameObject);
                if(!gObj.GetComponent<EnergyRock>())
                    gObj.GetComponent<SpriteRenderer>().color = FactionColors.colors[current.entities[i].faction];
                gObj.transform.position = current.entities[i].position;
                gObj.name = current.entities[i].name;

                objects.Add(current.entities[i].ID, gObj);
            }
            else if(obj is EntityBlueprint)
            {
                SpawnEntity(obj as EntityBlueprint, current.entities[i]);
            }
        }

        //land platforms
        lpg.SetColor(current.backgroundColor + new Color(0.5F, 0.5F, 0.5F));
        lpg.BuildTiles(current.platform, new Vector2(current.bounds.x + current.bounds.w / 2, current.bounds.y + current.bounds.h / 2));

        //sector color
        background.setColor(current.backgroundColor);
        Camera.main.backgroundColor = current.backgroundColor / 2F;
        //sector borders
        sectorBorders.enabled = true;
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
            battleZone.sectorName = current.sectorName;
            if(player) {
                var playerComp = player.GetComponent<PlayerCore>();
                battleZone.AddTarget(playerComp);
                if(carriers.ContainsKey(playerComp.faction))
                    playerComp.SetCarrier(carriers[playerComp.faction]);
            }
            for (int i = 0; i < current.targets.Length; i++)
            {
                if(objects[current.targets[i]].GetComponent<ShellCore>())
                {
                    // set the carrier of the shellcore to the associated faction's carrier
                    ShellCore shellcore = objects[current.targets[i]].GetComponent<ShellCore>();
                    if(carriers.ContainsKey(shellcore.faction))
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
        if (OnSectorLoad != null)
            OnSectorLoad.Invoke(current.sectorName);
    }

    public void InsertPersistentObject(string key, GameObject gameObject) {
        persistentObjects.Add(key + uniqueIDInt++, gameObject);
    }
}
