﻿using System.Collections;
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
    public bool detachible = true;
    private bool collectible;
    private int faction;

    public int GetFaction()
    {
        return faction;
    }

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
        rigid.angularDrag = 0;
        float[] directions = new float[] { -1, 1 };
        rigid.AddForce(new Vector2(200 * directions[Random.Range(0,2)], 200 * directions[Random.Range(0, 2)]));
        rigid.AddTorque(150 * directions[Random.Range(0, 1)]);
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
        if(transform.Find("Shooter"))
        {
            transform.Find("Shooter").GetComponent<SpriteRenderer>().color = FactionColors.colors[craft.faction];
        }
        if (GetComponent<Ability>())
        {
            GetComponent<Ability>().part = this;
        }
    }

    private void AimShooter()
    {
        if (craft.GetTargetingSystem().GetTarget() != null && transform.Find("Shooter"))
        {
            GameObject shooter = transform.Find("Shooter").gameObject;
            Vector3 targeterPos = craft.GetTargetingSystem().GetTarget().position;
            Vector3 diff = targeterPos - shooter.transform.position;
            shooter.transform.eulerAngles = new Vector3(0, 0, (Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg) + 90);
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
        AimShooter();
        if (hasDetached && Time.time - detachedTime < 1) // checks if the part has been detached for more than a second (hardcoded)
        {
            Blink(); // blink
        }
        else if (hasDetached) { // if it has actually detached
            if (collectible && detachible)
            {
                rigid.drag = 25;
                // add "Draggable" component so that shellcores can grab the part
                if (!gameObject.GetComponent<Draggable>()) gameObject.AddComponent<Draggable>();
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
        if (currentHealth <= 0 && detachible) {
            craft.RemovePart(this);
        }
    }
}
