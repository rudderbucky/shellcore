using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileScript : MonoBehaviour {

    public GameObject missileLinePrefab;
    private Transform target;
    public int faction;
    private int damage;

	// Use this for initialization
	void Start () {
		if (!missileLinePrefab)
        {
            missileLinePrefab = new GameObject("Missile Line");
            missileLinePrefab.transform.SetParent(transform, false);
            LineRenderer lineRenderer = missileLinePrefab.AddComponent<LineRenderer>();
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material");
            MissileAnimationScript comp = missileLinePrefab.AddComponent<MissileAnimationScript>();
            comp.Initialize();
            for (int i = 0; i < 2; i++)
            {
                var x = Instantiate(missileLinePrefab, transform);
                x.GetComponent<MissileAnimationScript>().Initialize();
            }
            GetComponent<Rigidbody2D>().AddTorque(50);
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                var x = Instantiate(missileLinePrefab, transform);
                x.GetComponent<MissileAnimationScript>().Initialize();
            }
        }

        if (!GetComponent<Collider2D>())
        {
            var collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
        }
    }
	
    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

	// Update is called once per frame
	void FixedUpdate () {
        var moveVector = (target.position - transform.position).normalized;
        GetComponent<Rigidbody2D>().AddForce(50 * moveVector);
	}

    void OnTriggerEnter2D(Collider2D collision)
    {
        var hit = collision.transform.root; // grab collision, get the topmost GameObject of the hierarchy, which would have the craft component
        var craft = hit.GetComponent<Entity>(); // check if it has a craft component
        if (craft != null) // check if the component was obtained
        {
            if (craft.faction != faction)
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
