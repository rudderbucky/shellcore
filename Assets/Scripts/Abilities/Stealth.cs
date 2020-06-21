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
                      // hardcoded values here
        abilityName = "Stealth";
        description = "Become invisible to enemies";
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
        craft.invisible = false;

        ToggleIndicator(true);
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
    }
}
