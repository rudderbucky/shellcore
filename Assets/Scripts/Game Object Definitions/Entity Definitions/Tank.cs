using System.Collections.Generic;
using UnityEngine;

public class Tank : GroundCraft, IOwnable
{
    Vector2[] path; // positions for tank to move to
    int index = 0;
    public bool HasPath = false;
    IOwner owner;
    float pathfindTimer = 0f;

    public bool IsInRange = false;

    WeaponAbility[] weapons;

    WeaponAbility[] Weapons
    {
        get
        {
            if (weapons == null)
            {
                weapons = GetComponentsInChildren<WeaponAbility>();
            }

            return weapons;
        }
    }

    public IOwner GetOwner()
    {
        return owner;
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
        if (AIData.tanks.Contains(this))
        {
            AIData.tanks.Remove(this);
        }

        base.OnDestroy();
    }

    protected override void Awake()
    {
        base.Awake();
        isStandardTractorTarget = true;
        if (!AIData.tanks.Contains(this))
        {
            AIData.tanks.Add(this);
        }
    }

    protected override void Start()
    {
        if (!GetComponent<Draggable>())
        {
            draggable = gameObject.AddComponent<Draggable>();
        }

        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (isOnGround && pathfindTimer >= 0f)
        {
            pathfindTimer -= Time.deltaTime;
            if (pathfindTimer <= 0f)
            {
                pathfindToTarget();
                if (HasPath)
                {
                    pathfindTimer = 3.0f;
                }
                else
                {
                    pathfindTimer = 1f;
                }
            }
        }

        if (isOnGround && !isDead)
        {
            TargetManager.Enqueue(targeter);
            if (!isDead && !draggable.Dragging)
            {
                foreach (var weapon in Weapons)
                {
                    if (weapon)
                        weapon.Tick();
                }
            }

            drive();
        }
        else
        {
            pathfindTimer = 1f;
            HasPath = false;
        }
    }

    private void pathfindToTarget()
    {
        if (!isOnGround)
        {
            return;
        }

        if (Weapons.Length == 0)
        {
            return;
        }

        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client) return;
        // Find valid ground targets
        List<Entity> targets = new List<Entity>(AIData.entities);

        for (int i = 0; i < targets.Count; i++)
        {
            if (!targets[i] ||
                targets[i].IsInvisible ||
                targets[i] == this ||
                FactionManager.IsAllied(faction, targets[i].faction) ||
                !Weapons[0].CheckCategoryCompatibility(targets[i]))
            {
                targets.RemoveAt(i);
                i--;
            }
        }
        
        List<Vector2> flags = new List<Vector2>();
        foreach(Flag flag in AIData.flags){
            if(flag.name == $"tankpickup{faction.factionID}"){
                flags.Add(flag.transform.position);
            }
        }

        // Find a path to the closest one
        if (targets.Count == 0) return;
        Vector2[] newPath = LandPlatformGenerator.pathfind(transform.position, targets.ToArray(), null, Weapons[0].GetRange());
        if(newPath == null){newPath = LandPlatformGenerator.pathfind(transform.position, null, flags.ToArray(), Weapons[0].GetRange());}
        if (!HasPath)
        {
            path = newPath;
            HasPath = (path != null && path.Length > 0);
            if (HasPath)
            {
                index = path.Length - 1;
            }
        }
        else if (newPath != null && path != null && newPath.Length > 0 && path.Length > 0)
        {
            if (newPath[0] != path[0])
            {
                path = newPath;
                HasPath = (path != null && path.Length > 0);
                if (HasPath)
                {
                    index = path.Length - 1;
                }
            }
        }
        else
        {
            path = newPath;
            HasPath = (path != null && path.Length > 0);
            if (HasPath)
            {
                index = path.Length - 1;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (HasPath && path.Length > 1)
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

        if (HasPath)
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

            IsInRange = false;
            var target = Weapons[0].GetTarget();
            if (target != null)
            {
                float r2 = Weapons[0].GetRange();
                r2 = r2 * r2;
                IsInRange = (target.transform.position - transform.position).sqrMagnitude < r2;
            }

            if (IsInRange)
            {
                return;
            }

            // Don't move if there's another tank with higher instance ID in range (give way and form a line)
            const float minDistance = 2.0f;
            var normalized = direction.normalized;
            for (int i = 0; i < AIData.entities.Count; i++)
            {
                Entity e = AIData.entities[i];
                if (e is Tank t && e != this && e.GetInstanceID() > GetInstanceID() && !t.IsInRange && t.HasPath)
                {
                    float d = (e.transform.position - (transform.position)).sqrMagnitude;
                    if (d < minDistance)
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
                    HasPath = false;
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
        }
    }

    public void SetOwner(IOwner owner)
    {
        this.owner = owner;
        owner.GetUnitsCommanding().Add(this);
    }
}
