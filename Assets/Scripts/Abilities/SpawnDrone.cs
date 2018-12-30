using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns a drone
/// </summary>
public class SpawnDrone : ActiveAbility
{
    public DroneSpawnData spawnData;
    Craft craft;
    public void Init()
    {
        ID = spawnData.abilitySpriteID;
        cooldownDuration = spawnData.cooldown;
        CDRemaining = cooldownDuration;
        activeDuration = (int)spawnData.delay; //why int?
        activeTimeRemaining = activeDuration;
        energyCost = (int)spawnData.energyCost; //why int?
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
        if (spawnData)
            Init();
    }

    private void Start()
    {
        craft = Core as Craft;
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
        drone.transform.position = craft.transform.position;
        drone.spawnPoint = craft.transform.position;
        drone.enginePower = 100;
        drone.Init();
        drone.getAI().Mode = DroneAI.AIMode.Follow;
        drone.getAI().followTarget = craft.transform;

        ToggleIndicator();
    }

    /// <summary>
    /// Starts the spawning coutdonwn
    /// </summary>
    protected override void Execute()
    {
        isActive = true; // set to active
        isOnCD = true; // set to on cooldown
        ToggleIndicator();
    }
}
