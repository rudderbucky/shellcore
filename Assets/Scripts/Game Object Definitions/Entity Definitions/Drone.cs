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

    protected override void Update()
    {
        base.Update();
        if (!draggable || (draggable && !draggable.dragging))
        {
            if (time != 0)
            {
                time = 0;
            }
        } else
        {
            if(ai.getMode() != AirCraftAI.AIMode.Inactive)
            {
                time = Time.time;
                initialzangle = transform.localEulerAngles.z;
            }
            ai.setMode(AirCraftAI.AIMode.Inactive);
            transform.localEulerAngles = new Vector3(0, 0, initialzangle + (Time.time - time) * -180f);
        }
    }
}
