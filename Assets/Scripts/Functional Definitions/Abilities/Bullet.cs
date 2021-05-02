using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : WeaponAbility
{
    public GameObject bulletPrefab; // the prefabbed sprite for a bullet with a BulletScript
    protected float bulletSpeed; // the speed of the bullet
    protected float survivalTime; // the time the bullet takes to delete itself
    protected Vector3 prefabScale; // the scale of the bullet prefab, used to enlarge the siege turret bullet
    protected float pierceFactor = 0; // pierce factor; increase this to pierce more of the shell
    protected string bulletSound = "clip_bullet2";
    public static readonly int bulletDamage = 400;

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        description = "Projectile that deals " + damage + " damage.";
        abilityName = "Bullet";
        bulletSpeed = 20;
        survivalTime = 0.5F;
        range = bulletSpeed * survivalTime;
        ID = AbilityID.Bullet;
        cooldownDuration = 2F;
        CDRemaining = cooldownDuration;
        energyCost = 25;
        damage = bulletDamage;
        prefabScale = 1 * Vector3.one;
        category = Entity.EntityCategory.All;
        bonusDamageType = typeof(AirConstruct);
    }

    protected override void Start() {
        bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
        base.Start();
    }

    /// <summary>
    /// Fires the bullet using the helper method
    /// </summary>
    /// <param name="victimPos">The position to fire the bullet to</param>
    protected override bool Execute(Vector3 victimPos)
    {
        if(Core.RequestGCD()) {
            if (targetingSystem.GetTarget()) // check if there is actually a target, do not fire if there is not
            {
                FireBullet(victimPos); // fire if there is
                isOnCD = true; // set on cooldown
                return true;
            }
            return false;
        }
        return false;
    }

    /// <summary>
    /// Helper method for Execute() that creates a bullet and modifies it to be shot
    /// </summary>
    /// <param name="targetPos">The position to fire the bullet to</param>
    protected virtual void FireBullet(Vector3 targetPos)
    {
        AudioManager.PlayClipByID(bulletSound, transform.position);
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
        script.SetDamage(GetDamage());
        script.SetCategory(category);
        script.SetTerrain(terrain);
        script.SetShooterFaction(Core.faction);
        script.SetPierceFactor(pierceFactor);
        script.particleColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction) : new Color(0.8F,1F,1F,0.9F);
        script.missParticles = true;

        // Add velocity to the bullet
        Vector3 predictionAdjuster = Vector2.zero;
        if(targetingSystem.GetTarget()) predictionAdjuster = (targetingSystem.GetTarget().GetComponentInChildren<Rigidbody2D>().velocity) * survivalTime / 2;
        bullet.GetComponent<Rigidbody2D>().velocity = Vector3.Normalize(targetPos - originPos + predictionAdjuster) * bulletSpeed;

        // Destroy the bullet after survival time
        script.StartSurvivalTimer(survivalTime);
    }
}
