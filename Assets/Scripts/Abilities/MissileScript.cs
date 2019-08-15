using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script used for the missile projectile
/// </summary>
public class MissileScript : MonoBehaviour {

    // TODO: Maybe merge all projectile scripts and make it modular?

    public GameObject missileLinePrefab; // line rendering prefab for missiles
    private Transform target; // target of missile
    public int faction; // faction of projectile
    public Entity owner; // owner of projectile
    private float damage; // damage missile projectile should deal
    private Entity.TerrainType terrain;
    private Entity.EntityCategory category;
    private bool worked = false;
    private Vector2 vector;
    public GameObject hitPrefab;
    public GameObject missPrefab;

    // Use this for initialization
    void Start () {
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
            }
            GetComponent<Rigidbody2D>().AddTorque(50); // add angular velocity
        }
        else
        {
            for (int i = 0; i < 3; i++) // instantiate the prefabs
            {
                var x = Instantiate(missileLinePrefab, transform); // instantiate 
                x.GetComponent<MissileAnimationScript>().Initialize(); // initialize
            }
        }

        if (!GetComponent<Collider2D>()) // no collider? no problem
        {
            var collider = gameObject.AddComponent<CircleCollider2D>(); // add collider component
            collider.isTrigger = true; // set trigger
        }
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

	// Update is called once per frame
	void FixedUpdate () {
        if(target)
        {
            var moveVector = (target.position - transform.position).normalized;
            GetComponent<Rigidbody2D>().AddForce(80 * moveVector);
        }
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

    void OnTriggerEnter2D(Collider2D collision)
    {
        var hit = collision.transform.root; // grab collision, get the topmost GameObject of the hierarchy, which would have the craft component
        var craft = hit.GetComponent<IDamageable>(); // check if it has a craft component
        if (craft != null && !craft.GetIsDead()) // check if the component was obtained
        {
            if (craft.GetFaction() != faction && CheckCategoryCompatibility(craft))
            {
                var residue = craft.TakeShellDamage(damage, 0, owner); // deal the damage to the target, no shell penetration
                                                        // if the shell is low, damage the part

                ShellPart part = collision.transform.GetComponent<ShellPart>();
                if (part)
                {
                    part.TakeDamage(residue); // damage the part
                }
                damage = 0; // make sure, that other collision events with the same bullet don't do any more damage
                worked = true;
                Destroy(gameObject); // bullet has collided with a target, delete immediately
            }
        }
    }
    public void OnDestroy() {
        if(!worked) {
            vector = target && transform ? (target.position - transform.position) : Vector3.zero;
            var miss = Instantiate(missPrefab, transform.position, Quaternion.Euler(0, 0, 
                Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg)).GetComponent<ParticleSystem>();
        }
        else Instantiate(hitPrefab, transform.position, Quaternion.identity);

        if(transform.GetComponentInChildren<TrailRenderer>()) {
            transform.GetComponentInChildren<TrailRenderer>().autodestruct = true;
            transform.DetachChildren();
        }
    }
}
