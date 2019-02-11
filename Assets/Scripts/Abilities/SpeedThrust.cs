using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives a temporary increase to the core's engine power
/// </summary>
public class SpeedThrust : ActiveAbility
{
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
        if(craft) craft.enginePower /= Mathf.Pow(1.25F, abilityTier); // bring the engine power back (will change to vary as Speed Thrust is tiered)
        ToggleIndicator();
    }

    /// <summary>
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        // adjust fields
        if(craft) craft.enginePower *= Mathf.Pow(1.25F, abilityTier); // add 200 to engine power (will change to vary as Speed Thrust is tiered)
        ResourceManager.PlayClipByID("clip_activateability", transform.position);
        isActive = true; // set to active
        isOnCD = true; // set to on cooldown
        ToggleIndicator();
    }
}
