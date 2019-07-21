using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : WeaponAbility {

    public GameObject effectPrefab;
    public GameObject effect;
    public Entity target;

    protected override void Awake()
    {
        base.Awake();
        abilityName = "Cannon";
        description = "Instant attack that deals " + damage + " damage.";
        ID = 6;
        damage = 100;
        cooldownDuration = 1F;
        CDRemaining = cooldownDuration;
        range = 10;
        energyCost = 10;
        category = Entity.EntityCategory.All;
    }

    protected override void Start() {
        effectPrefab = ResourceManager.GetAsset<GameObject>("cannonfire");
        base.Start();
    }
    /// <summary>
    /// Fires the cannon using the helper method
    /// </summary>
    /// <param name="victimPos">The position to fire the bullet to</param>
    protected override bool Execute(Vector3 victimPos)
    {
        if (targetingSystem.GetTarget().GetComponent<Entity>() != null) // check if there is actually a target, do not fire if there is not
        {
            ResourceManager.PlayClipByID("clip_cannon", transform.position);
            FireCannon(targetingSystem.GetTarget().GetComponent<Entity>()); // fire if there is
            isOnCD = true; // set on cooldown
            return true;
        }
        return false;
    }

    protected void Update()
    {
        if (effect && !targetingSystem.GetTarget())
        {
            Destroy(effect);
        }
    }
    private void FireCannon(Entity target)
    {
        this.target = target;
        var shooter = transform.Find("Shooter");
        if (effect) Destroy(effect);
        effect = Instantiate(effectPrefab, shooter, false);
        Destroy(effect, 0.1F);
        var residue = target.TakeShellDamage(damage, 0, GetComponentInParent<Entity>());
        target.TakeCoreDamage(residue);
    }
}
