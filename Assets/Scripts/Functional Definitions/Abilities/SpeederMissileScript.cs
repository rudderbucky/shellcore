using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Script used for the missile projectile
/// </summary>
public class SpeederMissileScript : MissileScript
{
    // TODO: Maybe merge all projectile scripts and make it modular?

    private float thresholdDamage;
    private float speedIncreaseThreshold = 10;
    private float magnification = 3.5f;
    private float damageIncreaseThreshold = 5;
    private float forceConst = 160;

    // Update is called once per frame
    protected virtual void FixedUpdate()
    {
        float magnitude = 0;
        if (target != null && target.GetComponent<Rigidbody2D>())
        {
            var moveVector = (target.position - transform.position).normalized;
            forceConst = 160;
            if (target.GetComponent<Rigidbody2D>().velocity.magnitude > speedIncreaseThreshold)
            {
                forceConst += target.GetComponent<Rigidbody2D>().velocity.magnitude * magnification
                    GetComponent<Rigidbody2D>().AddForce(moveVector * forceConst);
            }
        }
        if (target && target.GetComponent<ITargetable>().GetIsDead() && Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        var hit = collision.transform.root; // grab collision, get the topmost GameObject of the hierarchy, which would have the craft component
        var craft = hit.GetComponent<IDamageable>(); // check if it has a craft component
        if (craft != null && !craft.GetIsDead()) // check if the component was obtained
        {
            float residue;
            if (!FactionManager.IsAllied(faction, craft.GetFaction()) && CheckCategoryCompatibility(craft) && (!owner || (craft.GetTransform() != owner.transform)))
            {
                if (target.GetComponent<Rigidbody2D>().velocity.magnitude < damageIncreaseThreshold)
                {
                    residue = craft.TakeShellDamage(damage, 0, owner); // deal the damage to the target, no shell penetration
                                                                       // if the shell is low, damage the part
                }
                else
                {
                    residue = craft.TakeShellDamage(damage * 1.5f, 0, owner); //Use new damage if target is moving past threshold speed
                }


                ShellPart part = collision.transform.GetComponent<ShellPart>();
                if (part)
                {
                    part.TakeDamage(residue); // damage the part
                }

                damage = 0; // make sure, that other collision events with the same bullet don't do any more damage
                Instantiate(hitPrefab, transform.position, Quaternion.identity);
                Destroy(gameObject); // bullet has collided with a target, delete immediately
            }
        }
    }
}