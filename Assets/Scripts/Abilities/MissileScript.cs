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
    private int damage; // damage missile projectile should deal
    private Entity.TerrainType terrain;
    private Entity.EntityCategory category;

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
    public void SetDamage(int damage)
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
            GetComponent<Rigidbody2D>().AddForce(50 * moveVector);
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

    public bool CheckCategoryCompatibility(Entity entity)
    {
        return (category == Entity.EntityCategory.All || category == entity.category) && (terrain == Entity.TerrainType.All || terrain == entity.Terrain);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        var hit = collision.transform.root; // grab collision, get the topmost GameObject of the hierarchy, which would have the craft component
        var craft = hit.GetComponent<Entity>(); // check if it has a craft component
        if (craft != null) // check if the component was obtained
        {
            if (craft.faction != faction && CheckCategoryCompatibility(craft))
            {
                craft.TakeDamage(damage, 0); // deal the damage to the target, no shell penetration
                                                        // if the shell is low, damage the part
                if (craft.GetHealth()[0] <= 0)
                {
                    ShellPart part = collision.transform.GetComponent<ShellPart>();
                    if (part)
                    {
                        part.TakeDamage(damage); // damage the part
                    }
                }
                damage = 0; // make sure, that other collision events with the same bullet don't do any more damage
                Destroy(gameObject); // bullet has collided with a target, delete immediately
            }
        }
    }
}
