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
        return FireBullet(victimPos); // fire if there is
    }

    /// <summary>
    /// Helper method for Execute() that creates a bullet and modifies it to be shot
    /// </summary>
    /// <param name="targetPos">The position to fire the bullet to</param>
    protected virtual bool FireBullet(Vector3 targetPos)
    {
        AudioManager.PlayClipByID(bulletSound, transform.position);
        Vector3 originPos = part ? part.transform.position : Core.transform.position;
    
        // Calculate future target position
        Vector2 targetVelocity = targetingSystem.GetTarget() ? targetingSystem.GetTarget().GetComponentInChildren<Rigidbody2D>().velocity : Vector2.zero;
        
        // Closed form solution to bullet lead problem involves finding t via a quadratic solved here.
        Vector2 relativeDistance = targetPos - originPos;
        var a = (bulletSpeed * bulletSpeed - Vector2.Dot(targetVelocity,targetVelocity));
        var b = -(2*targetVelocity.x*relativeDistance.x + 2*targetVelocity.y*relativeDistance.y) ;
        var c = -Vector2.Dot(relativeDistance,relativeDistance);

        if(a == 0 || b*b-4*a*c<0) return false;

        var t1 = (-b + Mathf.Sqrt(b*b - 4*a*c))/(2*a);
        var t2 = (-b - Mathf.Sqrt(b*b - 4*a*c))/(2*a);
        
        float t = t1 < 0 ? (t2 < 0 ? 0 : t2) : (t2 < 0 ? t1 : Mathf.Min(t1,t2));
        if(t <= 0) return false;

        // Create the Bullet from the Bullet Prefab
        if(bulletPrefab == null)
        {
            bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
        }
        var bullet = Instantiate(bulletPrefab, originPos, Quaternion.Euler(new Vector3(0, 0, Mathf.Atan2(relativeDistance.y, relativeDistance.x) * Mathf.Rad2Deg - 90)));
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
        bullet.GetComponent<Rigidbody2D>().velocity = Vector3.Normalize(relativeDistance + targetVelocity*t) * bulletSpeed;

        // Destroy the bullet after survival time
        script.StartSurvivalTimer(survivalTime);
        return true;
    }
}
