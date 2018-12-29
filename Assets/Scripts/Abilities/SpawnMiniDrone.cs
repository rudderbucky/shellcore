using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns a mini drone
/// </summary>
public class SpawnMiniDrone : ActiveAbility
{
    Craft craft;
    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        ID = 10;
        cooldownDuration = 5;
        CDRemaining = cooldownDuration;
        activeDuration = 3;
        activeTimeRemaining = activeDuration;
        energyCost = 100;
    }

    private void Start()
    {
        craft = Core as Craft;
    }
    /// <summary>
    /// Creates a mini drone
    /// </summary>
    protected override void Deactivate()
    {
        // Spawn drone
        GameObject go = new GameObject("MiniDrone");
        Drone drone = go.AddComponent<Drone>();
        drone.blueprint = ResourceManager.GetAsset<EntityBlueprint>("mini_drone");
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
