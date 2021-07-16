using System.Collections.Generic;
using UnityEngine;

public class Tank : GroundCraft, IOwnable
{
    Vector2[] path; // positions for tank to move to
    int index = 0;
    bool hasPath = false;
    IOwner owner;
    float pathfindTimer = 0f;

    WeaponAbility weapon;

    WeaponAbility Weapon
    {
        get
        {
            if (weapon == null)
            {
                weapon = GetComponentInChildren<WeaponAbility>();
            }

            return weapon;
        }
    }

    public override bool isImmobile
    {
        get { return pins > 0 || forceImmobile || !isOnGround; }
        set { forceImmobile = true; }
    }

    protected override void OnDeath()
    {
        if (owner != null && !(owner.Equals(null)) && owner.GetUnitsCommanding().Contains(this))
        {
            owner.GetUnitsCommanding().Remove(this);
        }

        base.OnDeath();
    }

    protected override void OnDestroy()
    {
        if (owner != null && !(owner.Equals(null)) && owner.GetUnitsCommanding().Contains(this))
        {
            owner.GetUnitsCommanding().Remove(this);
        }

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
        {
            return;
        }

        if (!Weapon)
        {
            return;
        }

        // Find valid ground targets
        List<Entity> targets = new List<Entity>(AIData.entities);

        for (int i = 0; i < targets.Count; i++)
        {
            if (!targets[i] || 
                targets[i].IsInvisible || 
                targets[i] == this || 
                FactionManager.IsAllied(faction, targets[i].faction) || 
                !Weapon.CheckCategoryCompatibility(targets[i]))
            {
                targets.RemoveAt(i);
                i--;
            }
        }

        // Find a path to the closest one
        if (targets.Count > 0)
        {
            path = LandPlatformGenerator.pathfind(transform.position, targets.ToArray(), weapon.GetRange());

            hasPath = (path != null && path.Length > 0);

            if (hasPath)
            {
                index = path.Length - 1;
            }
        }
        else
        {
            Debug.Log("No valid targets.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (hasPath && path.Length > 1)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < path.Length - 1; i++)
            {
                if (i == index)
                {
                    Gizmos.color = Color.green;
                }

                Gizmos.DrawLine(path[i], path[i + 1]);
            }
        }
    }

    protected virtual void drive()
    {
        if (!isOnGround)
        {
            return;
        }

        if (hasPath)
        {
            Vector2 direction = path[index] - (Vector2)transform.position;

            float dx = Mathf.Abs(direction.x);
            float dy = Mathf.Abs(direction.y);

            if (dx < dy && dx > 2.5f)
            {
                direction.y = 0;
            }

            if (dy < dx && dy > 2.5f)
            {
                direction.x = 0;
            }

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
                        //hasPath = false;
                        //pathfindTimer = 0.5f;
                        return;
                    }
                }
            }

            if (direction.magnitude < 0.4f)
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
            {
                MoveCraft(normalized * 0.5F);
            }

            if (pathfindTimer <= 0f)
            {
                pathfindTimer = 5f;
            }
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
