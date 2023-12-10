using System.Collections.Generic;
using UnityEngine;
using static Entity;

public interface IOwner
{
    EntityFaction GetFaction();
    Transform GetTransform();
    List<IOwnable> GetUnitsCommanding();
    int GetIntrinsicCommandLimit();
    void SetIntrinsicCommandLimit(int val);
    int GetTotalCommandLimit();
    SectorManager GetSectorManager();
    Draggable GetTractorTarget();
}

/// <summary>
/// Spawns a drone 
/// </summary>
public class SpawnDrone : ActiveAbility
{
    public DroneSpawnData spawnData;
    EntityBlueprint blueprint;
    IOwner craft;

    public void Init()
    {
        ID = AbilityID.SpawnDrone;
        cooldownDuration = spawnData.cooldown;
        chargeDuration = spawnData.delay;
        activeDuration = spawnData.delay + 0.1f;
        energyCost = spawnData.energyCost;
        // create blueprint from string json in spawn data
        blueprint = SectorManager.TryGettingEntityBlueprint(spawnData.drone);
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
        if (spawnData)
        {
            Init();
        }
    }

    protected override void Start()
    {
        if (GetComponentInParent<Turret>())
        {
            foreach (AirCarrier aircarrier in GameObject.FindObjectsOfType<AirCarrier>())
            {
                if (aircarrier.name == "Air Carrier" && aircarrier.faction.factionID == GetComponentInParent<Turret>().faction.factionID)
                {
                    craft = aircarrier;
                }
            }

            if (craft == null)
            {
                craft = PlayerCore.Instance;
            }
        }

        if (craft == null)
        {
            craft = Core as IOwner;
        }
        base.Start();
    }

    /// <summary>
    /// Creates a drone
    /// </summary>
    protected override void Execute()
    {
        AudioManager.PlayClipByID("clip_respawn", transform.position);

        // Spawn the drone
        GameObject go = new GameObject(blueprint.name);
        Drone drone = go.AddComponent<Drone>();
        drone.blueprint = blueprint;
        drone.faction = craft.GetFaction();
        drone.transform.position = part.transform.position;
        drone.spawnPoint = part.transform.position;
        drone.type = spawnData.type;
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off)
            drone.blueprintString = JsonUtility.ToJson(blueprint);
        drone.SetOwner(craft);
        drone.Init();
        craft.GetSectorManager().InsertPersistentObject(drone.blueprint.name, go);
        if (SectorManager.instance && SectorManager.instance.GetComponentInChildren<BattleZoneManager>())
        {
            var stats = SectorManager.instance.GetComponentInChildren<BattleZoneManager>().stats.Find(s => s.faction == Core.faction.factionID);
            if (stats == null)
            {
                stats = new BattleZoneManager.Stats(Core.faction.factionID);
                SectorManager.instance.GetComponentInChildren<BattleZoneManager>().stats.Add(stats);
            }
            stats.droneSpawns++;
        }

        if (craft is ICarrier || craft is AirWeaponStation || craft is GroundWeaponStation)
        {
            drone.getAI().setMode(AirCraftAI.AIMode.Path);
            drone.GeneratePath();
        }
        else
        {
            if (drone.type != DroneType.Worker)
            {
                drone.getAI().follow(craft.GetTransform());
            }
            else
            {
                drone.getAI().setMode(AirCraftAI.AIMode.Tractor);
                drone.getAI().owner = craft;
            }
        }
    }

    protected override bool ExtraCriteriaToActivate()
    {
        return craft != null && !craft.Equals(null) && craft.GetUnitsCommanding().Count < craft.GetTotalCommandLimit();
    }

    protected override void ExtraCriteriaFailureEvent()
    {
        if (craft is PlayerCore player)
            player.alerter.showMessage("Unit limit reached!", "clip_alert");
    }

    /// <summary>
    /// Starts the spawning countdown
    /// </summary>
    public override void Activate()
    {
        if (craft != null && ExtraCriteriaToActivate())
        {
            base.Activate();
        }
        else if (!ExtraCriteriaToActivate())
        {
            ExtraCriteriaFailureEvent();
        }
    }
}
