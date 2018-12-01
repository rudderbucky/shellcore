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
        var hit = collision.gameObject; // grab collision 
        var craft = hit.GetComponent<Craft>(); // check if it has a craft component
        if (craft != null) // check if the component was obtained
        {
            craft.TakeDamage(damage, 0); // deal the damage to the target, no shell penetration
        }

        Destroy(gameObject); // bullet has collided, delete immediately
    }
}
