using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IOwner
{
    int GetFaction();
    Transform GetTransform();
    List<IOwnable> GetUnitsCommanding();
    int GetTotalCommandLimit();
    SectorManager GetSectorManager();
}

/// <summary>
/// Spawns a drone 
/// </summary>
public class SpawnDrone : ActiveAbility
{
    public DroneSpawnData spawnData;
    IOwner craft;
    public void Init()
    {
        ID = 10;
        cooldownDuration = spawnData.cooldown;
        CDRemaining = cooldownDuration;
        activeDuration = spawnData.delay; 
        activeTimeRemaining = activeDuration;
        energyCost = spawnData.energyCost;
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
        if (spawnData)
            Init();
    }

    private void Start()
    {
        craft = Core as IOwner;
    }
    /// <summary>
    /// Creates a drone
    /// </summary>
    protected override void Deactivate()
    {
        // Spawn the drone
        GameObject go = new GameObject(spawnData.drone.name);
        Drone drone = go.AddComponent<Drone>();
        drone.blueprint = spawnData.drone;
        drone.faction = craft.GetFaction();
        drone.transform.position = part.transform.position;
        drone.spawnPoint = part.transform.position;
        drone.enginePower = 100;
        drone.Init();
        drone.SetOwner(craft);
        craft.GetSectorManager().InsertPersistentObject(drone.blueprint.name, go);
        if (craft as ICarrier != null)
        {
            drone.getAI().setMode(AirCraftAI.AIMode.Path);
        }
        else
        {
            drone.getAI().follow(craft.GetTransform());
        }

        ToggleIndicator();
    }

    /// <summary>
    /// Starts the spawning countdown
    /// </summary>
    protected override void Execute()
    {
        if (craft != null && craft.GetUnitsCommanding().Count < craft.GetTotalCommandLimit())
        {
            isActive = true; // set to active
            isOnCD = true; // set to on cooldown
            ToggleIndicator();
        }
        else Core.TakeEnergy(-energyCost);
    }
}
