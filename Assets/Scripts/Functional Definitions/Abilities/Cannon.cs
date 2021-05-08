using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : WeaponAbility {

    public GameObject effectPrefab;
    public GameObject effect;
    public Entity target;
    public static readonly int cannonDamage = 200;

    protected override void Awake()
    {
        base.Awake();
        abilityName = "Cannon";
        description = "Instant attack that deals " + damage + " damage.";
        ID = AbilityID.Cannon;
        damage = cannonDamage;
        cooldownDuration = 2F;
        range = 10;
        energyCost = 25;
        category = Entity.EntityCategory.All;
        bonusDamageType = typeof(Drone);
    }

    /// <summary>
    /// Fires the cannon using the helper method
    /// </summary>
    /// <param name="victimPos">The position to fire the bullet to</param>
    protected override bool Execute(Vector3 victimPos)
    {
        // TODO: There was a check if the target was an entity. Is that necessary?
        if(!effectPrefab) effectPrefab = ResourceManager.GetAsset<GameObject>("cannonfire");
        AudioManager.PlayClipByID("clip_cannon", transform.position);
        FireCannon(targetingSystem.GetTarget().GetComponent<Entity>()); // fire if there is
        return true;
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
        GetDamage();
        var residue = target.TakeShellDamage(GetDamage(), 0, GetComponentInParent<Entity>());
        target.TakeCoreDamage(residue);
    }
}
