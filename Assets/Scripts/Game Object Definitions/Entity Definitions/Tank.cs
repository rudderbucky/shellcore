using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : GroundCraft, IOwnable
{
    Vector2[] path; // positions for tank to move to
    int index = 0; 
    bool hasPath = false;
    IOwner owner;
    Vector2? pathfindTarget = null;

    protected override void OnDeath()
    {
        if(owner != null && !(owner.Equals(null)) && owner.GetUnitsCommanding().Contains(this))
            owner.GetUnitsCommanding().Remove(this);
        base.OnDeath();
    }

    protected override void OnDestroy() {
        if(owner != null && !(owner.Equals(null)) && owner.GetUnitsCommanding().Contains(this))
            owner.GetUnitsCommanding().Remove(this);
        base.OnDestroy();
    }
    protected override void Start()
    {
        isDraggable = true;
        if (!GetComponent<Draggable>())
        {
            draggable = gameObject.AddComponent<Draggable>();
        }
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (isOnGround && !isDead)
        {
            targeter.GetTarget(true);
            if (!isDead && GetComponentInChildren<WeaponAbility>() && !draggable.dragging)
            {
                GetComponentInChildren<WeaponAbility>().Tick(0);
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
            if (FactionManager.IsAllied(entities[i].faction, faction))
                continue;
            float d2 = (transform.position - entities[i].transform.position).sqrMagnitude;
            if(d2 < minD && GetComponentInChildren<WeaponAbility>().CheckCategoryCompatibility(entities[i]))
            {
                minD = d2;
                target = entities[i];
            }
        }
        if (target != null && pathfindTarget != target.transform.position)
        {
            pathfindTarget = target.transform.position;

            path = null;
            if (target && (target.transform.position - transform.position).sqrMagnitude > 16)
                path = LandPlatformGenerator.pathfind(transform.position, target.transform.position);
            hasPath = (path != null);

            if (path != null)
                index = path.Length - 1;
        }
    }

    protected virtual void drive()
    {
        isImmobile = !isOnGround;
        if (hasPath)
        {
            Vector2 direction = path[index] - (Vector2)transform.position;

            if (direction.magnitude < 0.5f)
            {
                //if(index > 0)
                    index--;
                if (index < 0)
                {
                    hasPath = false;
                }
            } else if(index > 0 || direction.magnitude > 2F) 
            {
                MoveCraft(direction.normalized);
            } else MoveCraft(direction.normalized * 0.5F);
        }
        else
        {
            pathfindToTarget();
        }
    }

    public void SetOwner(IOwner owner)
    {
        this.owner = owner;
        owner.GetUnitsCommanding().Add(this);
    }
}
