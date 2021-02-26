using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes the craft temporarily invisible
/// </summary>
public class Stealth : ActiveAbility, IBlinkOnUse
{
    Craft craft;
    protected override void Awake()
    {
        base.Awake(); // base awake
        ID = AbilityID.Stealth;
        cooldownDuration = 10;
        CDRemaining = cooldownDuration;
        activeDuration = 4;
        activeTimeRemaining = activeDuration;
        energyCost = 100;
    }

    private void Start()
    {
        craft = Core as Craft;
    }

    /// <summary>
    /// Makes the craft visible again
    /// </summary>
    protected override void Deactivate()
    {
        base.Deactivate();

        craft.invisible = false;
        SpriteRenderer[] renderers = craft.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            var c = renderers[i].color;
            c.a = 1f;
            renderers[i].color = c;
        }
        Collider2D[] colliders = craft.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
        }
        Ability[] abilities = craft.GetAbilities();
        foreach (var ability in abilities)
        {
            ability.SetIsEnabled(false);
        }
    }

    /// <summary>
    /// Makes the craft invisible
    /// </summary>
    protected override void Execute()
    {
        if(craft) {
            // change visibility
            craft.invisible = true;
            SpriteRenderer[] renderers = craft.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var c = renderers[i].color;
                c.a = Core.faction == 0 ? 0.2f : 0f;
                renderers[i].color = c;
            }
            Collider2D[] colliders = craft.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }
        AudioManager.PlayClipByID("clip_activateability", transform.position);
        // adjust fields
        isActive = true; // set to active
        isOnCD = true; // set to on cooldown
        base.Execute();
    }
}
