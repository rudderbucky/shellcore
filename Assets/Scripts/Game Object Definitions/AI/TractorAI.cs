using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorAI : AIModule
{
    // Follow mode:
    Transform followTarget;
    TractorBeam beam;
    float beamUpdateTimer = 0.5f;

    //Path mode:
    Vector2 targetPosition;

    bool followingPlayer = true;

    public void Follow(Transform target)
    {
        followTarget = target;
        followingPlayer = true;
    }

    public void GoTo(Vector2 pos)
    {
        ai.movement.SetMoveTarget(pos, 25f);
        followingPlayer = false;
    }

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
        if(followingPlayer)
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
                ai.movement.SetMoveTarget(target.position + new Vector3(Random.Range(-1F, 1F), Random.Range(-1F, 1F)));
            }
        }
        if (beamUpdateTimer > 0f)
        {
            beamUpdateTimer -= Time.deltaTime;
            if (beamUpdateTimer <= 0f)
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
            float dist = TractorBeam.maxRangeSquared; //Max distance of new tractor beam
            for (int i = 0; i < AIData.strayParts.Count; i++)
            {
                float d = (AIData.strayParts[i].transform.position - craft.transform.position).sqrMagnitude;
                Draggable target = AIData.strayParts[i].GetComponent<Draggable>();
                if(target == owner.GetTractorTarget()) continue;
                if (d < dist && target && !target.dragging)
                {
                    dist = d;
                    part = target;
                }
            }
            for (int i = 0; i < AIData.rockFragments.Count; i++)
            {
                float d = (AIData.rockFragments[i].transform.position - craft.transform.position).sqrMagnitude;
                Draggable target = AIData.rockFragments[i].GetComponent<Draggable>();
                if (target == owner.GetTractorTarget()) continue;
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
        } else if (beam.GetTractorTarget() == owner.GetTractorTarget()) beam.SetTractorTarget(null);
    }

    public override void StateTick()
    {

    }
}
