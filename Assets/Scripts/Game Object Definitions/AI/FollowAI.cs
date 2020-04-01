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
            if(timer >= 0.3F)
            {
                timer = 0;
                direction = (target.position - craft.transform.position).magnitude > 3 ? target.position - craft.transform.position
                    + new Vector3(Random.Range(-1F, 1F), Random.Range(-1F, 1F)) : Vector3.zero;
            }
            else timer += Time.deltaTime;

            craft.MoveCraft(direction.normalized);
        }
    }

    public override void StateTick()
    {
    }
}
