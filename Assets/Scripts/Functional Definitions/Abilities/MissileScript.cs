using System.Collections;
using UnityEngine;

/// <summary>
/// Script used for the missile projectile
/// </summary>
public class MissileScript : MonoBehaviour, IProjectile
{
    // TODO: Maybe merge all projectile scripts and make it modular?

    public GameObject missileLinePrefab; // line rendering prefab for missiles
    private Transform target; // target of missile
    public int faction; // faction of projectile
    public Entity owner; // owner of projectile
    private float damage; // damage missile projectile should deal
    private Entity.TerrainType terrain;
    private Entity.EntityCategory category;
    private Vector2 vector;
    public GameObject hitPrefab;
    public GameObject missPrefab;
    public Color missileColor;
    bool fired = false;
    Vector2 prevPos;

    // Use this for initialization
    void Start()
    {
        if (!missileLinePrefab) // no missile line prefab? no problem
        {
            missileLinePrefab = new GameObject("Missile Line"); // create prefab and set to parent
            missileLinePrefab.transform.SetParent(transform, false);

            // I use this prefab as one of the active lines on the missile 
            // because what's the point in not doing it this way

            LineRenderer lineRenderer = missileLinePrefab.AddComponent<LineRenderer>(); // add line renderer
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material"); // get material
            MissileAnimationScript comp = missileLinePrefab.AddComponent<MissileAnimationScript>(); // add the animation script
            comp.Initialize(); // initialize the script to make it update-safe
            for (int i = 0; i < 2; i++) // instantiate thrice for a total of four prefabs
            {
                var x = Instantiate(missileLinePrefab, transform); // instantiate
                x.GetComponent<MissileAnimationScript>().Initialize(); // initialize
                x.GetComponent<MissileAnimationScript>().lineColor = missileColor;
            }

            GetComponent<Rigidbody2D>().AddTorque(50); // add angular velocity
        }
        else
        {
            for (int i = 0; i < 3; i++) // instantiate the prefabs
            {
                var x = Instantiate(missileLinePrefab, transform); // instantiate 
                x.GetComponent<MissileAnimationScript>().lineColor = missileColor;
                x.GetComponent<MissileAnimationScript>().Initialize(); // initialize
            }
        }

        //if (!GetComponent<Collider2D>()) // no collider? no problem
        //{
        //    var collider = gameObject.AddComponent<CircleCollider2D>(); // add collider component
        //    collider.isTrigger = true; // set trigger
        //}

        GetComponent<SpriteRenderer>().color = missileColor;
        prevPos = transform.position;
        AIData.collidingProjectiles.Add(this);
    }

    /// <summary>
    /// Set the damage of the missile projectile
    /// </summary>
    /// <param name="damage">damage</param>
    public void SetDamage(float damage)
    {
        this.damage = damage; // set damage
    }

    /// <summary>
    /// Set the target of the missile projectile
    /// </summary>
    /// <param name="target">The transform of the to-be-set target</param>
    public void SetTarget(Transform target)
    {
        this.target = target; // set target
    }

    float forceConst = 160;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target && (!target.GetComponent<Entity>() || !target.GetComponent<Entity>().IsInvisible))
        {
            var moveVector = (target.position - transform.position).normalized;
            GetComponent<Rigidbody2D>().AddForce(forceConst * moveVector);
        }

        if (target && target.GetComponent<ITargetable>().GetIsDead() && Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            Destroy(gameObject);
        }
        prevPos = transform.position;
    }

    public void SetTerrain(Entity.TerrainType terrain)
    {
        this.terrain = terrain;
    }

    public void SetCategory(Entity.EntityCategory category)
    {
        this.category = category;
    }

    public bool CheckCategoryCompatibility(IDamageable entity)
    {
        return (category == Entity.EntityCategory.All || category == entity.GetCategory()) && (terrain == Entity.TerrainType.All || terrain == entity.GetTerrain());
    }

    public void OnDestroy()
    {
        if (transform.GetComponentInChildren<TrailRenderer>())
        {
            transform.GetComponentInChildren<TrailRenderer>().autodestruct = true;
            transform.DetachChildren();
        }
        AIData.collidingProjectiles.Remove(this);
    }

    public void StartSurvivalTimer(float time)
    {
        StartCoroutine(DestroyTimer(time));
    }

    IEnumerator DestroyTimer(float time)
    {
        yield return new WaitForSeconds(time);
        vector = target && transform ? (target.position - transform.position) : Vector3.zero;
        InstantiateMissPrefab();
        Destroy(gameObject);
    }


    public void InstantiateHitPrefab()
    {
        Instantiate(hitPrefab, transform.position, Quaternion.identity);
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && !MasterNetworkAdapter.lettingServerDecide)
        {
            MasterNetworkAdapter.instance.BulletEffectClientRpc("strong_bullet_hit", transform.position, Vector2.zero);
        }
    }

    public void InstantiateMissPrefab()
    {
        Instantiate(missPrefab, transform.position, Quaternion.Euler(0, 0, Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg));
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && !MasterNetworkAdapter.lettingServerDecide)
        {
            MasterNetworkAdapter.instance.BulletEffectClientRpc("bullet_miss_prefab",transform.position, vector);
        }
    }

    public int GetFaction()
    {
        return faction;
    }

    public Entity GetOwner()
    {
        return owner;
    }

    public Vector4 GetPositions()
    {
        var pos = transform.position;
        return new Vector4(pos.x, pos.y, prevPos.x, prevPos.y);
    }

    public float GetDamage()
    {
        return damage;
    }

    public void HitPart(ShellPart part)
    {
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client) return;
        if (fired) return;

        var residue = part.craft.TakeShellDamage(damage, 0, owner); // deal the damage to the target, no shell penetration
        part.TakeDamage(residue);// if the shell is low, damage the part

        damage = 0; // make sure, that other collision events with the same bullet don't do any more damage
        InstantiateHitPrefab();
        fired = true;
        Destroy(gameObject); // bullet has collided with a target, delete immediately
    }

    public void HitDamageable(IDamageable damageable)
    {
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client) return;
        if (fired) return;

        float residue = damageable.TakeShellDamage(damage, 0, owner);
        if (damageable is Entity)
        {
            (damageable as Entity).TakeCoreDamage(residue);
        }
        damage = 0;
        InstantiateHitPrefab();
        fired = true;
        Destroy(gameObject);
    }
}
