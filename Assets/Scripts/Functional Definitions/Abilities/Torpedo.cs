using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torpedo : Bullet {

    public static readonly int torpedoDamage = 600;
    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 10;
        survivalTime = 2.5F;
        range = bulletSpeed * survivalTime;
        ID = AbilityID.Torpedo;
        cooldownDuration = 3F;
        energyCost = 100;
        damage = torpedoDamage;
        terrain = Entity.TerrainType.Ground;
        category = Entity.EntityCategory.All;
    }

    protected override void Start()
    {
        bulletPrefab = ResourceManager.GetAsset<GameObject>("torpedo_prefab");
    }
}
