using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The part script for the shell around a core of a Shellcore (will be salvaged to make a more general part class)
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ShellPart : MonoBehaviour {

    float detachedTime; // time since detachment
    private bool hasDetached; // is the part detached
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigid;
    private Entity craft;
    public float partHealth; // health of the part (half added to shell, quarter to core)
    public float partMass; // mass of the part
    private float currentHealth; // current health of part
    public bool constructPart;
    private bool collectible;
    public int faction;

    public void SetCollectible(bool collectible) {
        this.collectible = collectible;
    }

    public float GetPartMass()
    {
        return partMass;
    }
    public float GetPartHealth()
    {
        return partHealth; // part health
    }

    /// <summary>
    /// Detach the part from the Shellcore
    /// </summary>
    public void Detach() {
        if (name != "Shell Sprite")
            transform.SetParent(null, true);
        detachedTime = Time.time; // update detached time
        hasDetached = true; // has detached now
        gameObject.AddComponent<Rigidbody2D>(); // add a rigidbody (this might become permanent)
        rigid = GetComponent<Rigidbody2D>();
        rigid.gravityScale = 0; // adjust the rigid body
        rigid.drag = 25;
        rigid.angularDrag = 0;
        //rigid.interpolation = RigidbodyInterpolation2D.Extrapolate;
        // add force and torque
        rigid.AddForce(new Vector2(250 * Random.Range(-1F,2), 250 * Random.Range(-1F, 2)));
        rigid.AddTorque(100 * Random.Range(-20, 21));
    }

    public void Awake()
    {
        //Find sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Start () {
        // initialize instance fields
        hasDetached = false;
        spriteRenderer.enabled = true;
        Destroy(GetComponent<Rigidbody2D>()); // remove rigidbody
        currentHealth = partHealth / 4;
        craft = transform.root.GetComponent<Entity>();
        faction = craft.faction;
        spriteRenderer.color = FactionColors.colors[craft.faction];
        if (GetComponent<Ability>())
        {
            GetComponent<Ability>().part = this;
        }
    }

    /// <summary>
    /// Makes the part blink like in the original game
    /// </summary>
    void Blink() {
        spriteRenderer.enabled = Time.time % 0.25F > 0.125F; // math stuff that blinks the part
    }

	// Update is called once per frame
	void Update () {
        if (hasDetached && Time.time - detachedTime < 1) // checks if the part has been detached for more than a second (hardcoded)
        {
            Blink(); // blink
        }
        else if (hasDetached) { // if it has actually detached
            if (collectible)
            {
                // add "Draggable" component so that shellcores can grab the part
                if(!gameObject.GetComponent<Draggable>()) gameObject.AddComponent<Draggable>();
                spriteRenderer.enabled = true;
                spriteRenderer.sortingOrder = 0;
                rigid.angularVelocity = rigid.angularVelocity > 0 ? 200 : -200;
            }
            else
            {
                if (name != "Shell Sprite")
                {
                    Destroy(gameObject);
                } else spriteRenderer.enabled = false; // disable sprite renderer
            }
        }
	}

    /// <summary>
    /// Take part damage, if it is damaged too much remove the part
    /// </summary>
    /// <param name="damage">damage to deal</param>
    public void TakeDamage(float damage) {
        currentHealth -= damage;
        if (currentHealth <= 0 && !constructPart) {
            craft.RemovePart(this);
        }
    }
}
