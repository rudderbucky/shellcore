using UnityEngine;
using System.Collections;

public class AIMovement
{
    AirCraftAI ai;
    Craft craft;

    public AIMovement(AirCraftAI ai)
    {
        this.ai = ai;
        craft = ai.craft;
    }

    bool requireRangeUpdate = false;
    Vector2 moveTarget;
    float minDist = 10000f;
    public void SetMoveTarget(Vector2 target, float minDistance = 64f)
    {

        if (moveTarget != target || minDistance != minDist)
        {
            requireRangeUpdate = true;
            moveTarget = target;
            minDist = minDistance;
        }
    }
    bool inRange = false;
    public float DistanceToTarget
    {
        get; private set;
    }
    public bool targetIsInRange()
    {
        if (requireRangeUpdate)
        {
            DistanceToTarget = (moveTarget - (Vector2)craft.transform.position).sqrMagnitude;
            inRange = DistanceToTarget < minDist;
            requireRangeUpdate = false;
        }
        return inRange;
    }

    public void Update()
    {
        requireRangeUpdate = true;
        if (!targetIsInRange())
        {
            craft.MoveCraft((moveTarget - (Vector2)craft.transform.position).normalized);
        }
    }

    public Vector2 GetTarget()
    {
        return moveTarget;
    }
}
