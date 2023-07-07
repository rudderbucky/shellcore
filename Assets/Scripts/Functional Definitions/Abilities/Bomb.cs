using Unity.Netcode;
using UnityEngine;

public class Bomb : WeaponAbility
{
    public GameObject bombPrefab; // the prefabbed sprite for a bullet with a BulletScript
    protected float bombSpeed; // the speed of the bullet
    protected float survivalTime; // the time the bullet takes to delete itself
    protected Vector3 prefabScale; // the scale of the bullet prefab, used to enlarge the siege turret bullet
    protected float pierceFactor = 0; // pierce factor; increase this to pierce more of the shell
    protected string bombSound = "clip_bomb";
    public static readonly int bombDamage = 800;

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        description = $"Projectile that deals {damage} damage.";
        abilityName = "Bomb";
        bombSpeed = 12.5F;
        survivalTime = 5F;
        range = 10F;
        ID = AbilityID.Bomb;
        cooldownDuration = 8F;
        energyCost = 200;
        damage = bombDamage;
        prefabScale = 0.5F * Vector3.one;
        category = Entity.EntityCategory.All;
        bonusDamageType = typeof(AirConstruct);
    }

    protected override void Start()
    {
        bombPrefab = ResourceManager.GetAsset<GameObject>("bomb_prefab");
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
        AudioManager.PlayClipByID(bombSound, transform.position);
        Vector3 originPos = part ? part.transform.position : Core.transform.position;
        // Create the Bullet from the Bullet Prefab
        Vector3 diff = targetPos - originPos;
        if (bombPrefab == null)
        {
            bombPrefab = ResourceManager.GetAsset<GameObject>("bomb_prefab");
        }

        var bullet = Instantiate(bombPrefab, originPos, Quaternion.identity);
        bullet.transform.localScale = prefabScale;

        // Update its damage to match main bullet
        var script = bullet.GetComponent<BombScript>();
        bullet.GetComponent<Rigidbody2D>().velocity = (targetPos - originPos).normalized * bombSpeed * Mathf.Sqrt(Mathf.Min(1, (targetPos - transform.position).sqrMagnitude / (range * range)));
        script.owner = GetComponentInParent<Entity>();
        script.SetDamage(GetDamage());
        script.SetCategory(type == WeaponDiversityType.Torpedo ? Entity.EntityCategory.All : category);
        script.SetTerrain(type == WeaponDiversityType.Torpedo ? Entity.TerrainType.Ground : script.owner.Terrain);
        script.bombColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction) : new Color(0.8F, 1F, 1F, 0.9F);
        script.faction = Core.faction;

        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && (!NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost))
        {
            script.GetComponent<NetworkObject>().Spawn();
        }

        // Destroy the bullet after survival time
        script.StartSurvivalTimer(survivalTime);
    }
}
