using UnityEngine;

public class Torpedo : Bullet
{
    public static readonly int torpedoDamage = 600;

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 10;
        survivalTime = 1.8F;
        range = bulletSpeed * survivalTime;
        ID = AbilityID.Torpedo;
        cooldownDuration = 3F;
        energyCost = 100;
        damage = torpedoDamage;
        terrain = Entity.TerrainType.Ground;
        category = Entity.EntityCategory.All;
        bonusDamageType = typeof(GroundConstruct);
    }

    protected override void Start()
    {
        bulletPrefab = ResourceManager.GetAsset<GameObject>("torpedo_prefab");
    }
}
