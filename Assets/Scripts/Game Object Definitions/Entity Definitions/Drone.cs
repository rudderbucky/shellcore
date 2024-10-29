using System.Collections.Generic;
using UnityEngine;

public interface IOwnable
{
    void SetOwner(IOwner owner);
    IOwner GetOwner();
}

public class Drone : AirCraft, IOwnable
{
    private IOwner owner;
    private float time;
    private float initialzangle;
    public DroneType type;
    public Path path;
    
    private float aiReenableTime;

    public void DisableAITemporarily(float timeToEnableAt)
    {
        if (!ai) return;
        ai.enabled = false;
        aiReenableTime = timeToEnableAt;
    }


    public IOwner GetOwner()
    {
        return owner;
    }

    public void SetOwner(IOwner owner)
    {
        this.owner = owner;
        if (ai) ai.owner = owner;
        owner.GetUnitsCommanding().Add(this);
    }

    public void GeneratePath()
    {
        var path = ScriptableObject.CreateInstance<Path>();
        path.waypoints = new List<Path.Node>();

        List<Vector2> waypoints = new();

        // Drones are defensive for all carriers outside battlezones, or ground carriers anywhere

        // Attack enemy
        if ((owner is AirCarrier || owner is AirWeaponStation) && SectorManager.instance.GetCurrentType() == Sector.SectorType.BattleZone)
        {
            // Get valid targets
            List<Vector2> validTargets = new List<Vector2>();
            foreach (var ent in BattleZoneManager.getTargets())
            {
                if (ent && !FactionManager.IsAllied(ent.faction, owner.GetFaction()) && ent.transform)
                {
                    validTargets.Add(ent.transform.position);
                }
            }
            // Add valid targets in random order
            while (validTargets.Count > 0)
            {
                int r = Random.Range(0, validTargets.Count);
                waypoints.Add(validTargets[r]);
                validTargets.RemoveAt(r);
            }
        }
        // Form a circle around the station
        else
        {
            var angle = Random.Range(0F, 360);
            waypoints.Add(owner.GetTransform().position + new Vector3(5 * Mathf.Sin(angle), 5 * Mathf.Cos(angle)));
        }

        // Construct the path
        for (int i = 0; i < waypoints.Count; i++)
        {
            var node = new Path.Node();
            node.position = waypoints[i];
            node.ID = i;
            node.children = new List<int>();
            if (i < waypoints.Count - 1)
                node.children.Add(i + 1);
            path.waypoints.Add(node);
        }

        //Set the path
        if (owner as AirCarrier)
        {
            ai.setPath(path);
        }
        else
        {
            // Convert path to another identical data type (why??)
            NodeEditorFramework.Standard.PathData data = new NodeEditorFramework.Standard.PathData();
            data.waypoints = new List<NodeEditorFramework.Standard.PathData.Node>();
            // TODO: LOL THESE TWO ARE DIFFERENT, unify them!
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
    protected override void Awake()
    {
        base.Awake();
        isStandardTractorTarget = true;
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
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client && networkAdapter)
        {
            networkAdapter.CommandMovementServerRpc(pos);
            return;
        }
        ai.moveToPosition(pos);
    }

    public void CommandFollowOwner()
    {
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client && networkAdapter)
        {
            networkAdapter.CommandFollowOwnerServerRpc();
            return;
        }
        ai.follow(owner.GetTransform());
    }

    private AirCraftAI.AIMode? lastMode;

    protected override void Update()
    {
        if (Time.time > aiReenableTime && ai) ai.enabled = true;
        base.Update();
        if (!draggable || !draggable.Dragging)
        {
            if (lastMode.HasValue)
            {
                ai.setMode(lastMode.Value);
                lastMode = null;
            }
            time = Time.time;
            initialzangle = transform.localEulerAngles.z;
        }
        else
        {
            if (ai.getMode() != AirCraftAI.AIMode.Inactive)
            {
                lastMode = ai.getMode();
            }

            ai.setMode(AirCraftAI.AIMode.Inactive);
            transform.localEulerAngles = new Vector3(0, 0, initialzangle + (Time.time - time) * -180f);
        }
    }
}
