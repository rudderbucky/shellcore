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

    /// <summary>
    /// Fires the cannon using the helper method
    /// </summary>
    /// <param name="victimPos">The position to fire the bullet to</param>
    protected override bool Execute(Vector3 victimPos)
    {
        if(!effectPrefab) effectPrefab = ResourceManager.GetAsset<GameObject>("cannonfire");
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
        if (effect && (!targetingSystem.GetTarget() || part.GetDetached()))
        {
            Destroy(effect);
        }
    }

    private void FixedUpdate() {
        if(effect) {
            var rate = 0.15F;
            if(effect.transform.localScale.x > 0F) {
                effect.transform.localScale = new Vector3(Mathf.Max(effect.transform.localScale.x - rate, 0),
                    Mathf.Min(effect.transform.localScale.y + 2 * rate, 2), 1);
            }
        }
    }

    private void FireCannon(Entity target)
    {
        this.target = target;
        var shooter = transform.Find("Shooter");
        if (effect) Destroy(effect);
        effect = Instantiate(effectPrefab, shooter, false);
        Destroy(effect, 0.2F);
        var residue = target.TakeShellDamage(damage, 0, GetComponentInParent<Entity>());
        target.TakeCoreDamage(residue);
    }
}
