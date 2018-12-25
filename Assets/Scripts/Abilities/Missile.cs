using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : WeaponAbility {

    public GameObject missilePrefab;

    protected override void Awake()
    {
        base.Awake();
        ID = 7;
        cooldownDuration = 5F;
        CDRemaining = cooldownDuration;
        range = 20;
        energyCost = 25;
        category = Entity.EntityCategory.All;
    }

    protected override bool Execute(Vector3 victimPos)
    {
        if (targetingSystem.GetTarget() && targetingSystem.GetTarget().GetComponent<Entity>() != null) // check if there is actually a target, do not fire if there is not
        {
            var missile = Instantiate(missilePrefab, transform.position, Quaternion.identity);
            var script = missile.GetComponent<MissileScript>();
            script.SetTarget(targetingSystem.GetTarget());
            script.faction = Core.faction;
            script.SetDamage(1500);
            Destroy(missile, 7);
            isOnCD = true; // set on cooldown
            return true;
        }
        return false;
    }
}
