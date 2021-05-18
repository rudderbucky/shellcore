using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Bullet {

    public static readonly int laserDamage = 75;
    public static readonly float laserPierceFactor = 0.25F;
    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 100;
        survivalTime = 0.1F;
        range = bulletSpeed * survivalTime;
        ID = AbilityID.Laser;
        cooldownDuration = 0.2F;
        energyCost = 5;
        damage = 75;
        prefabScale = Vector2.one;
        terrain = Entity.TerrainType.All;
        category = Entity.EntityCategory.Unit;
        pierceFactor = laserPierceFactor;
    }

    protected override void Start() {
        bulletPrefab = ResourceManager.GetAsset<GameObject>("laser_prefab");
    }
}
