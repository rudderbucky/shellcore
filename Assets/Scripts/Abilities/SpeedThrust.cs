using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives a temporary increase to the core's engine power
/// </summary>
public class SpeedThrust : ActiveAbility
{
    bool activated = false;
    Craft craft;
    protected override void Awake()
    {
        base.Awake(); // base awake
                      // hardcoded values here
        abilityName = "Speed Thrust";
        description = "Temporarily increases speed.";
        ID = 1;
        cooldownDuration = 5;
        CDRemaining = cooldownDuration;
        activeDuration = 3;
        activeTimeRemaining = activeDuration;
        energyCost = 50;
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
        var enginePower = (Core as Craft).enginePower;
        if(craft && activated) {
            (Core as Craft).enginePower -= 100F * abilityTier;
        } // bring the engine power back (will change to vary as Speed Thrust is tiered)
        ToggleIndicator(true);
    }

    /// <summary>
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        // adjust fields
        if(craft) {
            var enginePower = (Core as Craft).enginePower;
            if(enginePower <= 1000) {
                activated = true;
                (Core as Craft).enginePower += 100F * abilityTier;
            }
            else activated = false;
        } // change engine power
        AudioManager.PlayClipByID("clip_activateability", transform.position);
        isActive = true; // set to active
        isOnCD = true; // set to on cooldown
        ToggleIndicator(true);
    }
}
