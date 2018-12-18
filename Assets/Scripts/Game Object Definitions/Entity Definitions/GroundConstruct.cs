using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundConstruct : Construct
{
    float time = 0f;
    protected bool onGround = false;
    private float initialzangle;

    protected override void Start()
    {
        base.Start();

        if(!GetComponent<Draggable>())
        {
            draggable = gameObject.AddComponent<Draggable>();
        }
    }

    protected override void Update ()
    {
        base.Update();
        if (LandPlatformGenerator.CheckOnGround(transform.position) && !draggable.dragging)
        {
            onGround = true;
        }
        else
        {
            if (onGround)
            {
                time = Time.time;
                initialzangle = transform.localEulerAngles.z;
            }
            onGround = false;

            transform.localEulerAngles = new Vector3(0, 0, initialzangle + (Time.time - time) * -180f);
        }
	}
}
