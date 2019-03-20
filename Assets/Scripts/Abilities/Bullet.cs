using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : WeaponAbility {

    // TODO: maybe "blueprint-ize" this ability, as well as every ability?

    public GameObject bulletPrefab; // the prefabbed sprite for a bullet with a BulletScript
    protected float bulletSpeed; // the speed of the bullet
    protected float survivalTime; // the time the bullet takes to delete itself
    protected float damage; // damage of the bullet
    protected Vector3 prefabScale; // the scale of the bullet prefab, used to enlarge the siege turret bullet
    protected float pierceFactor = 0; // pierce factor; increase this to pierce more of the shell


    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        description = "Projectile that deals " + damage + " damage.";
        abilityName = "Bullet";
        bulletSpeed = 15;
        survivalTime = 0.5F;
        range = bulletSpeed * survivalTime;
        ID = 5;
        cooldownDuration = 2F;
        CDRemaining = cooldownDuration;
        energyCost = 50;
        damage = 450;
        prefabScale = 1 * Vector3.one;
        category = Entity.EntityCategory.All;
    }

    protected virtual void Start() {
        bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
    }

    /// <summary>
    /// Fires the bullet using the helper method
    /// </summary>
    /// <param name="victimPos">The position to fire the bullet to</param>
    protected override bool Execute(Vector3 victimPos)
    {
        if (targetingSystem.GetTarget()) // check if there is actually a target, do not fire if there is not
        {
            FireBullet(victimPos); // fire if there is
            isOnCD = true; // set on cooldown
            return true;
        }
        return false;
    }

    /// <summary>
    /// Helper method for Execute() that creates a bullet and modifies it to be shot
    /// </summary>
    /// <param name="targetPos">The position to fire the bullet to</param>
    void FireBullet(Vector3 targetPos)
    {
        ResourceManager.PlayClipByID("clip_bullet", transform.position);
        Vector3 originPos = part ? part.transform.position : Core.transform.position;
        // Create the Bullet from the Bullet Prefab
        Vector3 diff = targetPos - originPos;
        if(bulletPrefab == null)
        {
            bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
        }
        var bullet = Instantiate(bulletPrefab, originPos, Quaternion.Euler(new Vector3(0, 0, Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg - 90)));
        bullet.transform.localScale = prefabScale;

        // Update its damage to match main bullet
        var script = bullet.GetComponent<BulletScript>();
        script.owner = GetComponentInParent<Entity>();
        script.SetDamage(damage * (this as MainBullet ? 1 : abilityTier));
        script.SetCategory(category);
        script.SetTerrain(terrain);
        script.SetShooterFaction(Core.faction);
        script.SetPierceFactor(pierceFactor);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody2D>().velocity = Vector3.Normalize(targetPos - originPos) * bulletSpeed;

        // Destroy the bullet after survival time
        Destroy(bullet, survivalTime);
    }
}
