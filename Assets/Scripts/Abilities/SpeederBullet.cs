using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeederBullet : Bullet {

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 50;
        survivalTime = 0.5F;
        range = bulletSpeed * survivalTime;
        ID = 5;
        cooldownDuration = 1.2F;
        CDRemaining = cooldownDuration;
        energyCost = 10;
        damage = 100;
        prefabScale = 1 * Vector3.one;
        category = Entity.EntityCategory.All;
    }
}
