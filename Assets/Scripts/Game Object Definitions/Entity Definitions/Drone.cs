using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOwnable
{
    void SetOwner(IOwner owner);
}

public class Drone : AirCraft, IOwnable {

    private IOwner owner;
    private DroneAI ai;
    private float time;
    private float initialzangle;
    private bool initialized = false;

    public Path path;

    public void SetOwner(IOwner owner)
    {
        this.owner = owner;
        ai.owner = owner;
        owner.GetUnitsCommanding().Add(this);
    }

    protected override void OnDeath()
    {
        if(owner != null)
            owner.GetUnitsCommanding().Remove(this);
        base.OnDeath();
    }

    public DroneAI getAI()
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
        ai = gameObject.AddComponent<DroneAI>();
        ai.craft = this;
        ai.path = path;
        ai.Mode = path == null ? DroneAI.AIMode.AutoPath : DroneAI.AIMode.Path;
        initialized = true;
    }

    public void CommandMovement(Vector3 pos)
    {
        ai.currentTargetPos = pos;
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
            if(ai.Mode != DroneAI.AIMode.Inactive)
            {
                time = Time.time;
                initialzangle = transform.localEulerAngles.z;
            }
            ai.Mode = DroneAI.AIMode.Inactive;
            transform.localEulerAngles = new Vector3(0, 0, initialzangle + (Time.time - time) * -180f);
        }
    }
}
