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
    [HideInInspector]
    public string resourcePath = "";
    private Dictionary<int, int> stationsCount = new Dictionary<int, int>();
    private Dictionary<int, ICarrier> carriers = new Dictionary<int, ICarrier>();
    private List<IVendor> stations = new List<IVendor>();
    private BattleZoneManager battleZone;
    private Dictionary<string, GameObject> objects;
    private Dictionary<string, GameObject> persistentObjects;
    private LandPlatformGenerator lpg;
    private LineRenderer sectorBorders;
    private List<LineRenderer> minimapSectorBorders;
    private int uniqueIDInt;
    private bool sectorLoaded = false;
    public Vector2 spawnPoint;
    public WorldData.CharacterData[] characters; // Unity initializes public arrays, remember!

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

    string jsonPath = Application.streamingAssetsPath + "\\Sectors\\testchars";
    public void Initialize()
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
        OnSectorLoad = null;

        if(jsonMode) LoadSectorFile(jsonPath);
        jsonMode = false;
    }

    private float dangerZoneTimer;
    public GameObject damagePrefab;
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

        // deadzone damage
        if(current && current.type == Sector.SectorType.DangerZone)
        {
            if(dangerZoneTimer >= 5)
            {
                dangerZoneTimer = 0;
                Instantiate(damagePrefab, player.transform.position, Quaternion.identity);
                player.TakeCoreDamage(0.2F * player.GetMaxHealth()[1]);
                player.alerter.showMessage("WARNING: Leave Sector!", "clip_stationlost");
            } else dangerZoneTimer += Time.deltaTime;
        } else
        {
            dangerZoneTimer = 0;
        }
    }

    public void TryGettingJSON() {
        string path = GameObject.Find("Path Input").GetComponent<UnityEngine.UI.InputField>().text;
        GameObject.Find("Path Input").transform.parent.gameObject.SetActive(false);
        LoadSectorFile(path);
    }

    public TaskManager taskManager;
    public void LoadSectorFile(string path)
    {
        resourcePath = path;
        if (System.IO.Directory.Exists(path))
        {
            try
            {
                string[] files = Directory.GetFiles(path);
                current = null;
                sectors = new List<Sector>();
                minimapSectorBorders = new List<LineRenderer>();
                foreach (string file in files)
                {
                    if(file.Contains(".meta")) continue;

                    // parse world data
                    if(file.Contains(".worlddata"))
                    {
                        string worlddatajson = System.IO.File.ReadAllText(file);
                        WorldData wdata = ScriptableObject.CreateInstance<WorldData>();
                        JsonUtility.FromJsonOverwrite(worlddatajson, wdata);
                        spawnPoint = wdata.initialSpawn;
                        if(player.cursave == null || player.cursave.timePlayed == 0)
                            player.transform.position = player.spawnPoint = spawnPoint;
                        if(characters == null || characters.Length == 0) characters = wdata.defaultCharacters;
                        continue;
                    }

                    if(file.Contains(".taskdata"))
                    {
                        taskManager.SetCanvasPath(file);
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

                    // render the borders on the minimap
                    var border = new GameObject("MinimapSectorBorder - " + curSect.sectorName).AddComponent<LineRenderer>();
                    border.material = ResourceManager.GetAsset<Material>("white_material");
                    border.gameObject.layer = 8;
                    border.enabled = true;
                    border.positionCount = 4;
                    border.startWidth = 0.5f;
                    border.endWidth = 0.5f;
                    border.loop = true;
                    border.SetPositions(new Vector3[]{
                        new Vector3(curSect.bounds.x, curSect.bounds.y, 0),
                        new Vector3(curSect.bounds.x + curSect.bounds.w, curSect.bounds.y, 0),
                        new Vector3(curSect.bounds.x + curSect.bounds.w, curSect.bounds.y + curSect.bounds.h, 0),
                        new Vector3(curSect.bounds.x, curSect.bounds.y + curSect.bounds.h, 0)
                    });
                    minimapSectorBorders.Add(border);

                    sectors.Add(curSect);
                }
                player.SetIsInteracting(false);
                Debug.Log("worked");
                jsonMode = false;
                sectorLoaded = true;
                return;
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            };
        }
        else if (System.IO.File.Exists(path))
        {
            try
            {
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
                sectorLoaded = true;
                player.SetIsInteracting(false);
                loadSector();
                return;
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }
        Debug.Log("Could not find valid sector in that path");
        jsonMode = false;
        player.SetIsInteracting(false);
        loadSector();
        sectorLoaded = true;
    }

    private void Start()
    {
        if(ResourceManager.Instance)sectorBorders.material = ResourceManager.GetAsset<Material>("white_material");

        if(!sectorLoaded)
        {
            background.setColor(SectorColors.colors[5]);
            if(!jsonMode) loadSector();
        }
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
                        // Check if data has blueprint JSON, if it does override the current blueprint
                        string json = data.blueprintJSON;
                        if (json != null && json != "")
                        {
                            blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
                            JsonUtility.FromJsonOverwrite(json, blueprint);
                            Debug.Log(data.name);
                            blueprint.entityName = data.name;
                            if(data.name == "Clearly Delusional")
                                blueprint.dialogue = ResourceManager.GetAsset<Dialogue>("default_dialogue");
                            // hack for now, TODO: implement JSON dialogue
                            // also TODO: dialogue editor (or allow multiple starting points in quest graphs to create multiple permanent "dialogue overrides")
                        } else shellcore.entityName = blueprint.entityName = data.name;
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
            // var x = JsonUtility.ToJson(data);
		    // string path = Application.streamingAssetsPath + "\\Sectors\\" + "SavedSector";
		    // System.IO.File.WriteAllText(path, x);
		    // System.IO.Path.ChangeExtension(path, ".json");            
            UnityEditor.AssetDatabase.CreateAsset(current, "Assets/SavedSector.asset");
            UnityEditor.AssetDatabase.CreateAsset(current.platform, "Assets/SavedSectorPlatform.asset");
        }
        #endif

        //unload previous sector
        var characterObjects = new Dictionary<string, GameObject>();
        foreach(var obj in objects)
        {
            if(player && (!player.GetTractorTarget() || (player.GetTractorTarget() && obj.Value != player.GetTractorTarget().gameObject))
                && obj.Value != player.gameObject)
            {
                var skipTag = false;
                if(obj.Value.GetComponentInChildren<Entity>())
                {
                    foreach(var ch in characters)
                    {
                        if(obj.Value.GetComponentInChildren<Entity>().ID == ch.ID)
                        {
                            skipTag = true;
                            break;
                        }
                    }
                }
                if(!skipTag)
                    Destroy(obj.Value);
                else characterObjects.Add(obj.Key, obj.Value);
            }
        }

        Dictionary<string, GameObject> tmp = new Dictionary<string, GameObject>();
        foreach(var obj in persistentObjects)
        {
            if(player && obj.Value && (!player.GetTractorTarget() || (player.GetTractorTarget() && obj.Value != player.GetTractorTarget().gameObject))
                && obj.Value != player.gameObject && !(player.unitsCommanding.Contains(obj.Value.GetComponent<Drone>() as IOwnable)
                && Vector3.SqrMagnitude(obj.Value.transform.position - player.transform.position) < 100))
            {
                Destroy(obj.Value);
            } else if(obj.Value) tmp.Add(obj.Key, obj.Value);
        }

        persistentObjects = tmp;

        foreach(ShellPart part in AIData.strayParts) {
            if(part && !(player && player.GetTractorTarget() && player.GetTractorTarget().GetComponent<ShellPart>() == part)) {
                Destroy(part.gameObject);
            }
        }
        AIData.strayParts.Clear(); 

        // Add the player's tractored part back so it gets deleted if the player doesn't tractor it through
        // to another sector
        if((player && player.GetTractorTarget() != null && player.GetTractorTarget().GetComponent<ShellPart>()))
            AIData.strayParts.Add(player.GetTractorTarget().GetComponent<ShellPart>());
        objects = characterObjects;

        // reset stations and carriers

        stations.Clear();
        carriers.Clear();

        //load new sector
        if(player) {
            // player has seen this sector now
            if(!player.cursave.sectorsSeen.Contains(current.sectorName))
                player.cursave.sectorsSeen.Add(current.sectorName);
            player.ResetPower();
            objects.Add("player", player.gameObject);
            player.sectorMngr = this;
            if(player.alerter) player.alerter.showMessage("Entering sector: " + current.sectorName);
        }


        for(int i = 0; i < current.entities.Length; i++)
        {
            // check if it is a character
            foreach(var ch in characters)
            {
                if(ch.ID == current.entities[i].ID)
                {
                    var skipTag = false;
                    foreach(var oj in objects)
                    {
                        if(oj.Value.GetComponentInChildren<Entity>() && oj.Value.GetComponentInChildren<Entity>().ID == ch.ID)
                        {
                            skipTag = true;
                            break;
                        }
                    }

                    if(skipTag) continue;
                    var print = ScriptableObject.CreateInstance<EntityBlueprint>();
                    JsonUtility.FromJsonOverwrite(ch.blueprintJSON, print);
                    print.intendedType = EntityBlueprint.IntendedType.ShellCore;
                    current.entities[i].name = ch.name;
                    current.entities[i].faction = ch.faction;
                    SpawnEntity(print, current.entities[i]);
                    continue;
                }
            }
            Object obj = ResourceManager.GetAsset<Object>(current.entities[i].assetID);

            if(obj is GameObject)
            {
                GameObject gObj = Instantiate(obj as GameObject);
                if(!gObj.GetComponent<EnergyRock>())
                    gObj.GetComponent<SpriteRenderer>().color = FactionColors.colors[current.entities[i].faction];
                gObj.transform.position = current.entities[i].position;
                gObj.name = current.entities[i].name;
                if(gObj.GetComponent<ShardRock>()) {
                    gObj.GetComponent<ShardRock>().tier = int.Parse(current.entities[i].vendingID);
                }
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
        //Camera.main.backgroundColor = current.backgroundColor / 2F;
        //sector borders
        sectorBorders.enabled = true;
        sectorBorders.SetPositions(new Vector3[]{
            new Vector3(current.bounds.x, current.bounds.y, 0),
            new Vector3(current.bounds.x + current.bounds.w, current.bounds.y, 0),
            new Vector3(current.bounds.x + current.bounds.w, current.bounds.y + current.bounds.h, 0),
            new Vector3(current.bounds.x, current.bounds.y + current.bounds.h, 0)
        });

        battleZone.enabled = false;
        // sector type things
        switch(current.type)
        {
            case Sector.SectorType.BattleZone:
                //battle zone things
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
                break;
            case Sector.SectorType.Haven:
            case Sector.SectorType.Capitol:
                player.spawnPoint = new Vector2(current.bounds.x + current.bounds.w / 2, current.bounds.y + current.bounds.h / 2);
                break;
            default:
                break;
        }



        // music
        PlayCurrentSectorMusic();

        if(info) info.showMessage("Entering sector '" + current.sectorName + "'");
        if (OnSectorLoad != null)
            OnSectorLoad.Invoke(current.sectorName);
    }

    public void PlayCurrentSectorMusic() 
    {
        if(current.hasMusic)
        {
            ResourceManager.PlayMusic(current.musicID);
        }
        else 
        {
            ResourceManager.StopMusic();
        }
    }

    public void InsertPersistentObject(string key, GameObject gameObject) {
        persistentObjects.Add(key + uniqueIDInt++, gameObject);
    }

    public GameObject GetObject(string name) {
        Debug.Log("Getting object: '" + name + "'");
        foreach(var pair in objects) {
            if(pair.Value == null)
            {
                continue;
            }
            if(pair.Value.name == name) return pair.Value;
        }
        return null;
    }

    public void RemoveObject(string name)
    {
        if(objects.ContainsKey(name))
            objects.Remove(name);
    }
}
