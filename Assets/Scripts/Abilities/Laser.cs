using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Bullet {

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 100;
        survivalTime = 2F;
        range = bulletSpeed * survivalTime;
        ID = 9;
        cooldownDuration = 0.2F;
        CDRemaining = cooldownDuration;
        energyCost = 5;
        damage = 50;
        prefabScale = Vector2.one;
        category = Entity.EntityCategory.Unit;
        pierceFactor = 0.5F;
    }
}
