using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCraft : Craft
{
    float time = 0f;
    protected bool isOnGround = false;
    protected Draggable draggable; //TODO: draggable in children that need it

    protected override void Start()
    {
        base.Start();

        if (!GetComponent<Draggable>())
        {
            draggable = gameObject.AddComponent<Draggable>();
        }
    }

    protected override void Update()
    {
        if (LandPlatformGenerator.CheckOnGround(transform.position) && !draggable.dragging)
        {
            isOnGround = true;
            transform.rotation = Quaternion.identity;
            base.Update();
        }
        else
        {
            if (isOnGround)
                time = Time.time;
            isOnGround = false;

            transform.localEulerAngles = new Vector3(0, 0, (Time.time - time) * -180f);
        }
    }
}
