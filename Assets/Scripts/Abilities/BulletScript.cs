using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for the bullet projectile of the Bullet and MainBullet ability
/// </summary>
public class BulletScript : MonoBehaviour {
    
    // TODO: Grab the shooter's alignment (once alignment is implemented) to prevent friendly fire

    private float damage; // damage of the spawned bullet
    
    /// <summary>
    /// Sets the damage value of the spawned buller
    /// </summary>
    /// <param name="damage">The damage the bullet does</param>
    public void SetDamage(float damage) {
        this.damage = damage; // set damage
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        var hit = collision.gameObject.transform.root; // grab collision, get the topmost GameObject of the hierarchy, which would have the craft component
        var craft = hit.GetComponent<Craft>(); // check if it has a craft component
        if (craft != null) // check if the component was obtained
        {
            craft.TakeDamage(damage, 0); // deal the damage to the target, no shell penetration
            damage = 0; // make sure, that other collision events with the same bullet don't do any more damage

            // if the shell is low, detach the part
            if(craft.GetHealth()[0] == 0)
            {
                ShellPart part = collision.gameObject.GetComponent<ShellPart>();
                if (part)
                {
                    craft.RemovePart(part);
                }
            }

            Destroy(gameObject); // bullet has collided with a target, delete immediately
        }
    }
}
