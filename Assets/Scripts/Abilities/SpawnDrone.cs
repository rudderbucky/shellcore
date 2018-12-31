using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IOwner
{
    int GetFaction();
    Transform GetTransform();
    List<IOwnable> GetUnitsCommanding();
    int GetTotalCommandLimit();
}

/// <summary>
/// Spawns a drone (ShellCore-exclusive)
/// </summary>
public class SpawnDrone : ActiveAbility
{
    public DroneSpawnData spawnData;
    IOwner craft;
    public void Init()
    {
        ID = spawnData.abilitySpriteID;
        cooldownDuration = spawnData.cooldown;
        CDRemaining = cooldownDuration;
        activeDuration = spawnData.delay; 
        activeTimeRemaining = activeDuration;
        energyCost = spawnData.energyCost;
        abilityName = spawnData.abilityName;
        description = spawnData.description;
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
        if(craft as ICarrier != null)
        {
            drone.getAI().Mode = DroneAI.AIMode.AutoPath;
        } else drone.getAI().Mode = DroneAI.AIMode.Follow;
        drone.getAI().followTarget = craft.GetTransform();

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
