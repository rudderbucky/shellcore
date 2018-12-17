using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundConstruct : Construct
{
    float time = 0f;
    protected bool onGround = false;
    protected Draggable draggable;

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
        if(LandPlatformGenerator.CheckOnGround(transform.position) && !draggable.dragging)
        {
            onGround = true;
            transform.rotation = Quaternion.identity;
            base.Update();
        }
        else
        {
            if(onGround)
                time = Time.time;
            onGround = false;

            transform.localEulerAngles = new Vector3(0, 0, (Time.time - time) * -180f);
        }
	}
}
