using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives a temporary increase to the core's engine power
/// </summary>
public class Stealth : ActiveAbility
{
    bool activated = false;
    Craft craft;
    protected override void Awake()
    {
        base.Awake(); // base awake
        ID = 24;
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
    /// Returns the engine power to the original value
    /// </summary>
    protected override void Deactivate()
    {
        ToggleIndicator(true);

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
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        // adjust fields
        if(craft) {
            craft.invisible = true;
        } // change engine power
        AudioManager.PlayClipByID("clip_activateability", transform.position);
        isActive = true; // set to active
        isOnCD = true; // set to on cooldown
        ToggleIndicator(true);

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
}
