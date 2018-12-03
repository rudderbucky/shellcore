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

        EnergySphereScript[] energies = FindObjectsOfType<EnergySphereScript>();

        Transform closest = null;
        float closestD = float.MaxValue;

        for (int i = 0; i < energies.Length; i++)
        {
            float sqrD = Vector3.SqrMagnitude(transform.position - energies[i].transform.position);
            if (closest == null || sqrD < closestD)
            {
                closestD = sqrD;
                closest = energies[i].transform;
            }
        }
        if(closest && closestD < 160) SetTractorTarget(closest.gameObject.GetComponent<Draggable>());
        if (target)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new Vector3[] { transform.position, target.transform.position });
            Rigidbody2D rigidbody = target.GetComponent<Rigidbody2D>();

            if (rigidbody)
            {
                //get direction
                Vector3 dir = transform.position - target.transform.position;
                //get distance
                float dist = dir.magnitude;

                if (target.GetComponent<EnergySphereScript>())
                {
                    rigidbody.AddForce(dir.normalized * 100f);
                }
                else if (dist > 2f)
                {
                    rigidbody.AddForce(dir.normalized * (dist - 2f) * 50f);
                }
            }
        }
        else lineRenderer.positionCount = 0;
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
