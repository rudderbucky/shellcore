using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorAI : AIModule
{
    // Follow mode:
    public Transform followTarget;
    TractorBeam beam;
    float beamUpdateTimer = 0.5f;

    public override void Init()
    {
        initialized = true;
        beam = craft.gameObject.AddComponent<TractorBeam>();
        beam.owner = craft;
        beam.SetEnergyEnabled(false);
        beam.BuildTractor();
    }

    public override void ActionTick()
    {
        Transform target;
        if(owner != null)
        {
            target = owner.GetTransform();
        }
        else
        {
            target = followTarget;
        }
        if (target != null)
        {
            Vector2 direction = (target.position - craft.transform.position).magnitude > 5 ? target.position - craft.transform.position : Vector3.zero;
            craft.MoveCraft(direction.normalized);
        }

        if (beamUpdateTimer > 0f)
        {
            beamUpdateTimer -= Time.deltaTime;
            if(beamUpdateTimer <= 0f)
            {
                beamUpdateTimer += 0.5f;
                updateBeam();
            }
        }
    }

    private void updateBeam()
    {
        if(beam.GetTractorTarget() == null)
        {
            Draggable part = null;
            float dist = 400f; //Max distance of new tractor beam
            for (int i = 0; i < AIData.strayParts.Count; i++)
            {
                float d = (AIData.strayParts[i].transform.position - craft.transform.position).sqrMagnitude;
                Draggable target = AIData.strayParts[i].GetComponent<Draggable>();
                if (d < dist && target && !target.dragging)
                {
                    dist = d;
                    part = target;
                }
            }
            if (part != null)
            {
                beam.SetTractorTarget(part);
            }
        }
    }

    public override void StateTick()
    {

    }
}
