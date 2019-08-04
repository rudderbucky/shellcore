using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The main bullet ability that every shellcore has as a basic ability (this will inherit from the base bullet ability)
/// </summary>
public class MainBullet : Bullet {

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 50;
        survivalTime = 0.5F;
        range = bulletSpeed * survivalTime;
        ID = 3;
        cooldownDuration = 0.4F;
        CDRemaining = cooldownDuration;
        energyCost = 10;
        damage = 150;
        description = "Projectile that deals " + damage + " damage. \nStays with you no matter what.";
        abilityName = "Main Bullet";
        bulletSound = "clip_bullet";
    }

}
