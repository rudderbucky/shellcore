using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : GroundCraft
{
    Vector2[] path;
    int index = 0;
    bool hasPath = false;

    protected override void Start()
    {
        isDraggable = true;
        base.Start();
        if (entityBody)
            entityBody.drag = 25f;
    }

    protected override void Update()
    {
        base.Update();

        if (isOnGround && !isDead)
        {
            targeter.GetTarget(true);
            if (!isDead && GetComponentInChildren<WeaponAbility>() && !draggable.dragging)
            {
                GetComponentInChildren<WeaponAbility>().Tick(null);
            }
            drive();
        }
        else
        {
            hasPath = false;
        }
    }

    private void pathfindToTarget()
    {
        Entity[] entities = FindObjectsOfType<Entity>();
        Entity target = null;
        float minD = float.MaxValue;
        for(int i = 0; i < entities.Length; i++)
        {
            if (entities[i].faction == faction)
                continue;
            float d2 = (transform.position - entities[i].transform.position).sqrMagnitude;
            if(d2 < minD)
            {
                minD = d2;
                target = entities[i];
            }
        }

        path = LandPlatformGenerator.pathfind(transform.position, target.transform.position);
        hasPath = (path != null);

        if (path != null)
            index = path.Length - 1;
    }

    protected virtual void drive()
    {
        isImmobile = !isOnGround;

        if (hasPath)
        {
            Vector2 direction = path[index] - (Vector2)transform.position;
            MoveCraft(direction.normalized);

            if (direction.magnitude < 0.1f)
            {
                index--;
                if (index < 0)
                {
                    hasPath = false;
                }
            }
        }
        else
        {
            pathfindToTarget();
        }
    }
}
