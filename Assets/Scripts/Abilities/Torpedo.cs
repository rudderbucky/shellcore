using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torpedo : Bullet {

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 10;
        survivalTime = 2.5F;
        range = bulletSpeed * survivalTime;
        ID = 8;
        cooldownDuration = 3F;
        CDRemaining = cooldownDuration;
        energyCost = 100;
        damage = 600;
        terrain = Entity.TerrainType.Ground;
        category = Entity.EntityCategory.All;
    }

    protected override void Start() {
        bulletPrefab = ResourceManager.GetAsset<GameObject>("torpedo_prefab");
    }
}
