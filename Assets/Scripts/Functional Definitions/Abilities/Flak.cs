using System.Collections.Generic;
using UnityEngine;

public class Flak : WeaponAbility
{
    public GameObject bulletPrefab; // the prefabbed sprite for a bullet with a BulletScript
    protected float bulletSpeed; // the speed of the bullet
    protected float survivalTime; // the time the bullet takes to delete itself
    protected Vector3 prefabScale; // the scale of the bullet prefab, used to enlarge the siege turret bullet
    protected float pierceFactor = 0; // pierce factor; increase this to pierce more of the shell
    protected string bulletSound = "clip_flak";
    public static readonly int bulletDamage = 100;

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        description = "Projectile that deals " + damage + " damage.";
        abilityName = "Bullet";
        bulletSpeed = 40;
        survivalTime = 0.25F;
        range = bulletSpeed * survivalTime;
        ID = AbilityID.Flak;
        cooldownDuration = 1F;
        energyCost = 30;
        damage = bulletDamage;
        prefabScale = 0.5F * Vector3.one;
        category = Entity.EntityCategory.All;
        bonusDamageType = typeof(Drone);
    }

    protected override void Start()
    {
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

        // Create the Bullet from the Bullet Prefab
        if (bulletPrefab == null)
        {
            bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
        }

        var bullets = 5;
        // TODO: Highly suboptimal, this essentially makes the target check go twice
        List<Entity> potentialTargets = TargetManager.GetTargetList(targetingSystem, category);
        List<Transform> targets = new List<Transform>();
        // Just get the 5 closest entities, the complexity is just O(N) instead of sorting which would be O(NlogN)
        for (int i = 0; i < bullets; i++)
        {
            var target = TargetManager.GetClosestFromList(potentialTargets, targetingSystem, category);
            if (target != null)
            {
                potentialTargets.Remove(target.GetComponentInChildren<Entity>());
                targets.Add(target);
            }
            else
            {
                break;
            }
        }


        Vector3 originPos = part ? part.transform.position : Core.transform.position;
        for (int i = 0; i < Mathf.Min(bullets, targets.Count); i++)
        {
            // Calculate future target position
            Vector2 targetVelocity = targets[i] ? targets[i].GetComponentInChildren<Rigidbody2D>().velocity : Vector2.zero;
            targetPos = targets[i].transform.position;
            // Closed form solution to bullet lead problem involves finding t via a quadratic solved here.
            Vector2 relativeDistance = targetPos - originPos;
            var a = (bulletSpeed * bulletSpeed - Vector2.Dot(targetVelocity, targetVelocity));
            var b = -(2 * targetVelocity.x * relativeDistance.x + 2 * targetVelocity.y * relativeDistance.y);
            var c = -Vector2.Dot(relativeDistance, relativeDistance);

            var delta = b * b - 4 * a * c;
            if (a == 0 || delta < 0)
            {
                continue;
            }

            var deltaSqrt = Mathf.Sqrt(delta);
            var t1 = (-b + deltaSqrt) / (2 * a);
            var t2 = (-b - deltaSqrt) / (2 * a);

            float t = t1 < 0 ? (t2 < 0 ? 0 : t2) : (t2 < 0 ? t1 : Mathf.Min(t1, t2));
            if (t <= 0)
            {
                continue;
            }


            var totalSpreadInDegrees = bullets * 20;
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
            script.particleColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction) : new Color(0.8F, 1F, 1F, 0.9F);
            script.missParticles = true;

            var normalizedVec = Vector3.Normalize(relativeDistance + targetVelocity * t);
            //var angle = (-(bullets / 2) + i) * 20;
            //var finalVec = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.Atan2(normalizedVec.y, normalizedVec.x)),
            //    Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.Atan2(normalizedVec.y, normalizedVec.x))).normalized;
            // Add velocity to the bullet
            bullet.GetComponent<Rigidbody2D>().velocity = normalizedVec * bulletSpeed;

            // Destroy the bullet after survival time
            script.StartSurvivalTimer(survivalTime);
        }

        return true;
    }
}
