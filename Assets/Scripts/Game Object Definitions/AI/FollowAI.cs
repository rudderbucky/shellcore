using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowAI : AIModule
{
    // Follow mode:
    public Transform followTarget;
    float timer = 0.3F;
    Vector2 direction = Vector2.zero;
    public override void Init()
    {
        initialized = true;
    }

    public override void ActionTick()
    {
        Transform target;
        if(owner != null && !owner.Equals(null))
        {
            target = owner.GetTransform();
        }
        else
        {
            target = followTarget;
        }

        if (target != null)
        {
            if(timer >= 0.3F)
            {
                timer = 0;
                ai.movement.SetMoveTarget(target.position + new Vector3(Random.Range(-1F, 1F), Random.Range(-1F, 1F)));
            }
            else timer += Time.deltaTime;
        }
    }

    public override void StateTick()
    {
    }
}
