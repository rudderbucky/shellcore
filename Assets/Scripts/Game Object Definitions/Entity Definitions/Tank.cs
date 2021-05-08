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
    float pathfindTimer = 0f;

    WeaponAbility weapon;
    WeaponAbility Weapon
    {
        get
        {
            if (weapon == null)
                weapon = GetComponentInChildren<WeaponAbility>();
            return weapon;
        }
    }

    public override bool isImmobile
    {
        get
        {
            return pins > 0 || forceImmobile || !isOnGround;
        }
        set
        {
            forceImmobile = true;
        }
    }

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

        if (pathfindTimer >= 0f)
        {
            pathfindTimer -= Time.deltaTime;
            if (pathfindTimer <= 0f)
            {
                pathfindToTarget();
            }
        }

        if (isOnGround && !isDead)
        {
            TargetManager.Enqueue(targeter);
            if (!isDead && Weapon && !draggable.dragging)
            {
                Weapon.Tick();
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
        if (!isOnGround)
            return;

        // Find the closest ground target
        Entity[] entities = FindObjectsOfType<Entity>();
        Entity target = null;
        float minD = float.MaxValue;
        for(int i = 0; i < entities.Length; i++)
        {
            if (FactionManager.IsAllied(entities[i].faction, faction))
                continue;
            float d2 = (transform.position - entities[i].transform.position).sqrMagnitude;
            if(d2 < minD && Weapon.CheckCategoryCompatibility(entities[i]))
            {
                minD = d2;
                target = entities[i];
            }
        }

        // If a target is found, find a path to it
        if (target != null)
        {
            pathfindTarget = target.transform.position;

            path = null;
            if (target && (target.transform.position - transform.position).sqrMagnitude > 16)
                path = LandPlatformGenerator.pathfind(transform.position, target.transform.position, Weapon.GetRange());
            hasPath = (path != null && path.Length > 0);

            if (hasPath)
                index = path.Length - 1;
        }
    }

    protected virtual void drive()
    {
        if (!isOnGround)
            return;
            
        if (hasPath)
        {
            Vector2 direction = path[index] - (Vector2)transform.position;

            const float minDistance = 2.0f;

            // TODO: optimize?

            var normalized = direction.normalized;
            for (int i = 0; i < AIData.entities.Count; i++)
            {
                Entity e = AIData.entities[i];
                if (e is GroundCraft && e != this && e.GetInstanceID() > GetInstanceID())
                {
                    float d = (e.transform.position - (transform.position)).sqrMagnitude;
                    if (d < minDistance && pathfindTimer <= 0f)
                    {
                        hasPath = false;
                        pathfindTimer = 0.5f;
                        return;
                    }
                }
            }

            if (direction.magnitude < 0.5f)
            {
                index--;
                if (index < 0)
                {
                    hasPath = false;
                }
            }
            else if (index > 0 || direction.magnitude > 2F) 
            {
                MoveCraft(normalized);
            }
            else
                MoveCraft(normalized * 0.5F);
        }
        else if (pathfindTimer <= 0f)
        {
            pathfindTimer = 0.5f;
        }
    }

    public void SetOwner(IOwner owner)
    {
        this.owner = owner;
        owner.GetUnitsCommanding().Add(this);
    }
}
