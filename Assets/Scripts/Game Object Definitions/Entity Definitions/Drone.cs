﻿using System.Collections.Generic;
using UnityEngine;

public interface IOwnable
{
    void SetOwner(IOwner owner);
}

public class Drone : AirCraft, IOwnable
{
    private IOwner owner;
    private float time;
    private float initialzangle;
    public DroneType type;
    public Path path;

    public IOwner GetOwner()
    {
        return owner;
    }

    public void SetOwner(IOwner owner)
    {
        this.owner = owner;
        if (ai) ai.owner = owner;
        owner.GetUnitsCommanding().Add(this);
        if (owner is AirCarrier || owner is GroundCarrier || owner is AirWeaponStation || owner is GroundWeaponStation)
        {
            // GET THE DRONES TO MOVE
            ai.setMode(AirCraftAI.AIMode.Path);
            var path = ScriptableObject.CreateInstance<Path>();
            path.waypoints = new List<Path.Node>();
            var vec = Vector2.zero;
            if ((owner is AirCarrier || owner is AirWeaponStation) && SectorManager.instance?.current?.type == Sector.SectorType.BattleZone)
            {
                List<Vector2> validTargets = new List<Vector2>();
                foreach (var ent in BattleZoneManager.getTargets())
                {
                    if (ent && !FactionManager.IsAllied(ent.faction, owner.GetFaction()) && ent.transform)
                    {
                        validTargets.Add(ent.transform.position);
                    }
                }
                vec = validTargets[Random.Range(0, validTargets.Count)];
            }
            // drones are defensive for all carriers outside battlezones, or ground carriers anywhere,
            // so set a path to the drone position currently
            else
            {
                var angle = Random.Range(0F, 360);
                vec = owner.GetTransform().position + new Vector3(5 * Mathf.Sin(angle), 5 * Mathf.Cos(angle));
            }

            // TODO: jank, fix this eventually
            var node = new Path.Node();
            node.position = vec;
            node.ID = 0;
            node.children = new List<int>();
            if (vec != Vector2.zero)
            {
                path.waypoints.Add(node);
            }

            if (owner as AirCarrier)
            {
                ai.setPath(path);
            }
            else
            {

                NodeEditorFramework.Standard.PathData data = new NodeEditorFramework.Standard.PathData();
                data.waypoints = new List<NodeEditorFramework.Standard.PathData.Node>();
                // TODO: LOL THESE TWO ARE DIFFERENT, unify them
                foreach (var point in path.waypoints)
                {
                    var node2 = new NodeEditorFramework.Standard.PathData.Node();
                    node2.ID = point.ID;
                    node2.children = point.children;
                    node2.position = point.position;
                    data.waypoints.Add(node2);
                }

                ai.setPath(data, null, true);
            }
        }
    }

    protected override void OnDeath()
    {
        if (owner != null && !(owner.Equals(null)) && owner.GetUnitsCommanding().Contains(this))
        {
            owner.GetUnitsCommanding().Remove(this);
        }

        base.OnDeath();
    }

    protected override void OnDestroy()
    {
        if (owner != null && !(owner.Equals(null)) && owner.GetUnitsCommanding().Contains(this))
        {
            owner.GetUnitsCommanding().Remove(this);
        }

        base.OnDestroy();
        if (GetComponentInChildren<TractorBeam>())
        {
            GetComponentInChildren<TractorBeam>().SetTractorTarget(null);
        }
    }

    public AirCraftAI getAI()
    {
        return ai;
    }

    protected override void Start()
    {
        if (blueprint.useCustomDroneType)
        {
            type = blueprint.customDroneType;
        }

        if (!initialized)
        {
            Init();
        }
    }

    public void Init()
    {
        base.Start();
        ai = GetAI();
        ai.owner = owner;
        if (type == DroneType.Worker)
        {
            ai.aggression = AirCraftAI.AIAggression.KeepMoving;
        }

        ai.allowRetreat = false;
        initialized = true;
    }

    public void CommandMovement(Vector3 pos)
    {
        ai.moveToPosition(pos);
    }

    private AirCraftAI.AIMode lastMode;

    protected override void Update()
    {
        base.Update();
        if (!draggable || (draggable && !draggable.dragging))
        {
            if (time != 0)
            {
                ai.setMode(lastMode);
                time = 0;
            }
        }
        else
        {
            if (ai.getMode() != AirCraftAI.AIMode.Inactive)
            {
                time = Time.time;
                initialzangle = transform.localEulerAngles.z;
                lastMode = ai.getMode();
            }

            ai.setMode(AirCraftAI.AIMode.Inactive);
            transform.localEulerAngles = new Vector3(0, 0, initialzangle + (Time.time - time) * -180f);
        }
    }
}
