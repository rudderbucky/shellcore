using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowAI : AIModule
{
    // Follow mode:
    public Transform followTarget;

    public override void Init()
    {
        initialized = true;
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
    }

    public override void StateTick()
    {
    }
}
