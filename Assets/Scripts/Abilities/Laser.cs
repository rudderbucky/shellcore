using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Bullet {

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 100;
        survivalTime = 0.1F;
        range = bulletSpeed * survivalTime;
        ID = 9;
        cooldownDuration = 0.2F;
        CDRemaining = cooldownDuration;
        energyCost = 15;
        damage = 50;
        prefabScale = Vector2.one;
        terrain = Entity.TerrainType.All;
        category = Entity.EntityCategory.Unit;
        pierceFactor = 0.18F;
    }

    protected override void Start() {
        bulletPrefab = ResourceManager.GetAsset<GameObject>("laser_prefab");
    }
}
