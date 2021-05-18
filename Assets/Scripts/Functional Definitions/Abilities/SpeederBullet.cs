using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeederBullet : Bullet {

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 50;
        survivalTime = 0.2F;
        range = bulletSpeed * survivalTime;
        ID = AbilityID.SpeederBullet;
        cooldownDuration = 1.2F;
        energyCost = 10;
        damage = 100;
        prefabScale = 1 * Vector3.one;
        category = Entity.EntityCategory.All;
    }
}
