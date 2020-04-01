using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOwnable
{
    void SetOwner(IOwner owner);
}

public class Drone : AirCraft, IOwnable {

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
        ai.owner = owner;
        owner.GetUnitsCommanding().Add(this);
        if(owner as AirCarrier) 
        {
            // GET THE DRONES TO MOVE
            ai.setMode(AirCraftAI.AIMode.Path);
            var path = ScriptableObject.CreateInstance<Path>();
            path.waypoints = new List<Path.Node>();
            var vec = Vector2.zero;
            foreach(var ent in BattleZoneManager.getTargets())
            {
                if(ent && ent is ICarrier && ent.faction != owner.GetFaction() && ent.transform)
                {
                    vec = ent.transform.position;
                }
            }
            // TODO: jank, fix this eventually
            var node = new Path.Node();
            node.position = vec;
            node.ID = 0;
            node.children = new List<int>();
            if(vec != Vector2.zero) path.waypoints.Add(node);
            ai.setPath(path);
        }
    }

    protected override void OnDestroy() {
        if(owner != null && !(owner.Equals(null)) && owner.GetUnitsCommanding().Contains(this))
            owner.GetUnitsCommanding().Remove(this);
        base.OnDestroy();
        if(GetComponentInChildren<TractorBeam>()) GetComponentInChildren<TractorBeam>().SetTractorTarget(null);
    }

    public AirCraftAI getAI()
    {
        return ai;
    }

    protected override void Start()
    {
        if(blueprint.useCustomDroneType)
            type = blueprint.customDroneType;
        if (!initialized)
            Init();
    }

    public void Init()
    {
        isDraggable = true;
        base.Start();
        ai = gameObject.AddComponent<AirCraftAI>();
        ai.Init(this, owner);
        //ai.aggression = AirCraftAI.AIAggression.KeepMoving;
        ai.allowRetreat = false;
        //ai.setPath(path);
        //ai.setMode(AirCraftAI.AIMode.Path);
        initialized = true;
        enginePower = 200;
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
        } else
        {
            if(ai.getMode() != AirCraftAI.AIMode.Inactive)
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
