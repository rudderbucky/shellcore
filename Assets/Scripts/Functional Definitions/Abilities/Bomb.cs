using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : WeaponAbility
{
    public GameObject bombPrefab; // the prefabbed sprite for a bullet with a BulletScript
    protected float bombSpeed; // the speed of the bullet
    protected float survivalTime; // the time the bullet takes to delete itself
    protected Vector3 prefabScale; // the scale of the bullet prefab, used to enlarge the siege turret bullet
    protected float pierceFactor = 0; // pierce factor; increase this to pierce more of the shell
    protected string bulletSound = "clip_bullet2";
    public static readonly int bombDamage = 5000;

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        description = "Projectile that deals " + damage + " damage.";
        abilityName = "Bomb";
        bombSpeed = 2;
        survivalTime = 45F;
        range = 30F;
        ID = AbilityID.Bomb;
        cooldownDuration = 30F;
        energyCost = 500;
        damage = bombDamage;
        prefabScale = 1 * Vector3.one;
        category = Entity.EntityCategory.All;
        bonusDamageType = typeof(AirConstruct);
    }

    protected override void Start() {
        bombPrefab = ResourceManager.GetAsset<GameObject>("bomb_prefab");
        survivalTime = 45F * abilityTier;
        base.Start();
    }

    /// <summary>
    /// Fires the bullet using the helper method
    /// </summary>
    /// <param name="victimPos">The position to fire the bullet to</param>
    protected override bool Execute(Vector3 victimPos)
    {
        FireBullet(victimPos); // fire if there is
        return true;
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
        if(bombPrefab == null)
        {
            bombPrefab = ResourceManager.GetAsset<GameObject>("bomb_prefab");
        }
        var bullet = Instantiate(bombPrefab, originPos, Quaternion.identity);
        bullet.transform.localScale = prefabScale;

        // Update its damage to match main bullet
        var script = bullet.GetComponent<BombScript>();
        script.owner = GetComponentInParent<Entity>();
        script.SetDamage(GetDamage());
        script.SetCategory(category);
        script.SetTerrain(terrain);
        script.bombColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction) : new Color(0.8F,1F,1F,0.9F);
        script.faction = Core.faction;

        // Destroy the bullet after survival time
        script.StartSurvivalTimer(survivalTime);
    }
}
