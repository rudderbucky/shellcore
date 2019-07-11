using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOwnable
{
    void SetOwner(IOwner owner);
}

public class Drone : AirCraft, IOwnable {

    private IOwner owner;
    private AirCraftAI ai;
    private float time;
    private float initialzangle;
    private bool initialized = false;
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

    protected override void OnDeath()
    {
        if(owner != null && !(owner.Equals(null)))
            owner.GetUnitsCommanding().Remove(this);
        base.OnDeath();
    }

    public AirCraftAI getAI()
    {
        return ai;
    }

    protected override void Start()
    {
        if (!initialized)
            Init();
    }

    public void Init()
    {
        isDraggable = true;
        base.Start();
        ai = gameObject.AddComponent<AirCraftAI>();
        ai.Init(this, owner);
        ai.aggression = AirCraftAI.AIAggression.KeepMoving;
        ai.allowRetreat = false;
        ai.setPath(path);
        //ai.setMode(AirCraftAI.AIMode.Path);
        initialized = true;
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
