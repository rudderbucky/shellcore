using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiegeBullet : Bullet {

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 15;
        survivalTime = 2F;
        range = bulletSpeed * survivalTime;
        ID = 14;
        cooldownDuration = 5F;
        CDRemaining = cooldownDuration;
        energyCost = 10;
        damage = 1000;
        prefabScale = 2 * Vector2.one;
        category = Entity.EntityCategory.Station;
    }
}
