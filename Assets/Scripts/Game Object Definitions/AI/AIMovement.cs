using UnityEngine;

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
    Vector2? moveTarget;
    float minDist = 10000f;

    public void StopMoving()
    {
        moveTarget = null;
    }

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
    public float DistanceToTarget { get; private set; }

    public bool targetIsInRange()
    {
        if (requireRangeUpdate && moveTarget != null)
        {
            DistanceToTarget = ((Vector2)moveTarget - (Vector2)craft.transform.position).sqrMagnitude;
            inRange = DistanceToTarget < minDist;
            requireRangeUpdate = false;
        }

        return inRange;
    }

    public void Update()
    {
        requireRangeUpdate = true;
        if (!targetIsInRange() && moveTarget != null)
        {
            craft.MoveCraft(((Vector2)moveTarget - (Vector2)craft.transform.position).normalized);
        }
    }

    public Vector2? GetTarget()
    {
        return moveTarget;
    }
}
