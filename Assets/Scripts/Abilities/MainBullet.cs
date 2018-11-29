using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainBullet : WeaponAbility {

    public GameObject bulletPrefab;
    private float bulletSpeed;
    private float survivalTime;

    protected override void Awake()
    {
        base.Awake();
        bulletSpeed = 50;
        survivalTime = 0.5F;
        range = bulletSpeed * survivalTime;
        ID = 3;
        cooldownDuration = 0.4F;
        CDRemaining = cooldownDuration;
        energyCost = 10;
    }

    protected override void Execute(Vector3 victimPos)
    {
        if (core.GetTargetingSystem().GetTarget() != null)
        {
            FireBullet(victimPos);
            isOnCD = true;
        }
    }

    void FireBullet(Vector3 targetPos)
    {
        // Create the Bullet from the Bullet Prefab
        var bullet = Instantiate(bulletPrefab, core.transform.position + Vector3.Normalize(targetPos - core.transform.position) * 1.5F, Quaternion.identity);

        //bullet.GetComponent<>
        // Update its damage to match main bullet
        bullet.GetComponent<BulletScript>().SetDamage(10);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody2D>().velocity = Vector3.Normalize(targetPos - core.transform.position) * 50;

        // Destroy the bullet after 0.5 seconds
        Destroy(bullet, survivalTime);
    }
}
