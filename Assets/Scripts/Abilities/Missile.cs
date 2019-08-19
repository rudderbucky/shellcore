using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : WeaponAbility {

    public GameObject missilePrefab;
    protected override void Awake()
    {
        base.Awake();
        damage = 1000;
        description = "Homing projectile that deals " + damage + " damage.";
        abilityName = "Missile";
        ID = 7;
        cooldownDuration = 5F;
        CDRemaining = cooldownDuration;
        range = 20;
        energyCost = 125;
        category = Entity.EntityCategory.All;
    }

    protected override void Start() {
        missilePrefab = ResourceManager.GetAsset<GameObject>("missile_prefab");
        base.Start();
    }
    protected override bool Execute(Vector3 victimPos)
    {
        if(Core.RequestGCD()) {
            if (targetingSystem.GetTarget() && targetingSystem.GetTarget().GetComponent<IDamageable>() != null) // check if there is actually a target, do not fire if there is not
            {
                ResourceManager.PlayClipByID("clip_bullet2", transform.position);
                if(missilePrefab == null)
                    missilePrefab = ResourceManager.GetAsset<GameObject>("missile_prefab");
                var missile = Instantiate(missilePrefab, transform.position, Quaternion.identity);
                var script = missile.GetComponent<MissileScript>();
                script.owner = GetComponentInParent<Entity>();
                script.SetTarget(targetingSystem.GetTarget());
                script.SetCategory(category);
                script.SetTerrain(terrain);
                script.faction = Core.faction;
                script.SetDamage(damage * abilityTier);
                script.StartSurvivalTimer(7);
                isOnCD = true; // set on cooldown
                return true;
            }
            return false;
        } return false;
    }
}
