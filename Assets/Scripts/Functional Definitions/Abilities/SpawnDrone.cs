﻿using System.Collections.Generic;
using UnityEngine;

public interface IOwner
{
    int GetFaction();
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
        blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
        JsonUtility.FromJsonOverwrite(spawnData.drone, blueprint);
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
        if (spawnData)
        {
            Init();
        }
    }

    private void Start()
    {
        if (GetComponentInParent<Turret>())
        {
            foreach (AirCarrier aircarrier in GameObject.FindObjectsOfType<AirCarrier>())
            {
                if (aircarrier.name == "Air Carrier" && aircarrier.faction == GetComponentInParent<Turret>().faction)
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
        drone.Init();
        drone.SetOwner(craft);
        craft.GetSectorManager().InsertPersistentObject(drone.blueprint.name, go);
        if (SectorManager.instance && SectorManager.instance.GetComponentInChildren<BattleZoneManager>())
        {
            var stats = SectorManager.instance.GetComponentInChildren<BattleZoneManager>().stats.Find(s => s.faction == Core.faction);
            if (stats == null)
            {
                stats = new BattleZoneManager.Stats(Core.faction);
                SectorManager.instance.GetComponentInChildren<BattleZoneManager>().stats.Add(stats);
            }
            stats.droneSpawns++;
        }

        if (craft as ICarrier != null)
        {
            drone.getAI().setMode(AirCraftAI.AIMode.Path);
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

    /// <summary>
    /// Starts the spawning countdown
    /// </summary>
    public override void Activate()
    {
        if (craft != null && craft.GetUnitsCommanding().Count < craft.GetTotalCommandLimit())
        {
            //Core.TakeEnergy(-energyCost); i'm not sure what this line of code does, but it seemed to make drones take energy from the user's max energy instead of their current energy
            base.Activate();
        }
        else if (craft as PlayerCore && craft.GetUnitsCommanding().Count >= craft.GetTotalCommandLimit())
        {
            (craft as PlayerCore).alerter.showMessage("Unit limit reached!", "clip_alert");
        }
    }
}
