using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns a drone (ShellCore-exclusive)
/// </summary>
public class SpawnDrone : ActiveAbility
{
    public DroneSpawnData spawnData;
    ShellCore craft;
    public void Init()
    {
        ID = spawnData.abilitySpriteID;
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
        craft = Core as ShellCore;
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
        drone.faction = craft.faction;
        drone.transform.position = part.transform.position;
        drone.spawnPoint = part.transform.position;
        drone.enginePower = 100;
        drone.Init();
        drone.SetOwner(craft as ShellCore);
        drone.getAI().Mode = DroneAI.AIMode.Follow;
        drone.getAI().followTarget = craft.transform;

        ToggleIndicator();
    }

    /// <summary>
    /// Starts the spawning coutdonwn
    /// </summary>
    protected override void Execute()
    {
        if (craft.unitsCommanding.Count < craft.GetTotalCommandLimit())
        {
            isActive = true; // set to active
            isOnCD = true; // set to on cooldown
            ToggleIndicator();
        }
        else Core.TakeEnergy(-energyCost);
    }
}
