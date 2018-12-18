using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : AirCraft {

    private DroneAI ai;
    private float time;
    private float initialzangle;

    protected override void Start()
    {
        isDraggable = true;
        base.Start();
        ai = gameObject.AddComponent<DroneAI>();
        ai.craft = this;
        ai.mode = DroneAI.Mode.Follow;
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
            if(ai.mode != DroneAI.Mode.Inactive)
            {
                time = Time.time;
                initialzangle = transform.localEulerAngles.z;
            }
            ai.mode = DroneAI.Mode.Inactive;
            transform.localEulerAngles = new Vector3(0, 0, initialzangle + (Time.time - time) * -180f);
        }
    }
}
