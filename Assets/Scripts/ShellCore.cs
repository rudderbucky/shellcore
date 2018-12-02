using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All "human-like" craft are considered ShellCores. These crafts are intelligent and all air-borne. This includes player ShellCores.
/// </summary>
public class ShellCore : AirCraft {

    public LineRenderer lineRenderer;
    Draggable target;

    // TODO: these will be either enemies or allies, most allies and a few enemies can be interacted with.
    protected override void Start()
    {
        base.Start(); // base start
        // initialize instance fields
        respawns = true;
        transform.position = new Vector3(10, 0, 0);
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
    }

    protected override void Update() {
        base.Update(); // base update

        if(target)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new Vector3[] { transform.position, target.transform.position});
            Rigidbody2D rigidbody = target.GetComponent<Rigidbody2D>();

            if(rigidbody)
            {
                //get direction
                Vector3 dir = transform.position - target.transform.position;
                //get distance
                float dist = dir.magnitude;

                if(dist > 2f)
                {
                    rigidbody.AddForce(dir.normalized * (dist - 2f) * 50f);
                }
            }
        }
    }

    public void SetTractorTarget(Draggable newTarget)
    {
        lineRenderer.enabled = (newTarget != null);
        target = newTarget;
    }

    public Draggable GetTractorTarget()
    {
        return target;
    }
}
