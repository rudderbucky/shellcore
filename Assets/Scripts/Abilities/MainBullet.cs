using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The main bullet ability that every shellcore has as a basic ability (this will inherit from the base bullet ability)
/// </summary>
public class MainBullet : Bullet {

    public GameObject muzzleFlash;
    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        bulletSpeed = 50;
        survivalTime = 0.3F;
        range = bulletSpeed * survivalTime;
        ID = 3;
        cooldownDuration = 0.4F;
        CDRemaining = cooldownDuration;
        energyCost = 10;
        damage = 150;
        description = "Projectile that deals " + damage + " damage. \nStays with you no matter what.";
        abilityName = "Main Bullet";
        bulletSound = "clip_bullet";
        muzzleFlash = ResourceManager.GetAsset<GameObject>("main_bullet_muzzle_flash");
    }

    private GameObject muzzle;
    protected override void FireBullet(Vector3 targetPos)
    {
        muzzle = Instantiate(muzzleFlash, transform.position, Quaternion.identity);

        var deltaVector = targetPos - transform.position;
        float targetAngle = Mathf.Atan2(deltaVector.y, deltaVector.x) * Mathf.Rad2Deg;
        // float craftAngle = Mathf.Atan2(transform.up.y, transform.up.x) * Mathf.Rad2Deg;

        // float delta = Mathf.Abs(Mathf.DeltaAngle(targetAngle - craftAngle, 90));

        muzzle.transform.eulerAngles = new Vector3(0, 0, targetAngle);
        base.FireBullet(targetPos);
    }

    void Update()
    {
        if(muzzle) muzzle.transform.position = transform.position;
    }

}
