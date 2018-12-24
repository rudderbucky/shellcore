using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiegeBullet : Bullet {

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 15;
        survivalTime = 3F;
        range = bulletSpeed * survivalTime;
        ID = 3;
        cooldownDuration = 5F;
        CDRemaining = cooldownDuration;
        energyCost = 10;
        damage = 500;
        prefabScale = 2 * Vector2.one;
    }
}
