using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for the bullet projectile of the Bullet and MainBullet ability
/// </summary>
public class BulletScript : MonoBehaviour {

    private float damage; // damage of the spawned bullet
    private int faction;
    private Entity.TerrainType terrain;
    private Entity.EntityCategory category;
    private float pierceFactor = 0;

    /// <summary>
    /// Sets the damage value of the spawned buller
    /// </summary>
    /// <param name="damage">The damage the bullet does</param>
    public void SetDamage(float damage) {
        this.damage = damage; // set damage
    }

    public void SetPierceFactor(float pierce)
    {
        pierceFactor = pierce;
    }

    public void SetShooterFaction(int faction)
    {
        this.faction = faction;
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
        return (category == Entity.EntityCategory.All || category == entity.category) && (terrain == Entity.TerrainType.All || terrain == entity.terrain);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        var hit = collision.transform.root; // grab collision, get the topmost GameObject of the hierarchy, which would have the craft component
        var craft = hit.GetComponent<Entity>(); // check if it has a craft component
        if (craft != null) // check if the component was obtained
        {
            if (craft.faction != faction && CheckCategoryCompatibility(craft))
            {
                craft.TakeDamage(damage, pierceFactor); // deal the damage to the target, no shell penetration
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
