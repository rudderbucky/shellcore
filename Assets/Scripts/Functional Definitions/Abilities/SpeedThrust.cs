using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives a temporary increase to the core's engine power
/// </summary>
public class SpeedThrust : ActiveAbility, IBlinkOnUse
{
    bool activated = false;
    Craft craft;
    public static readonly float boost = 20;
    protected override void Awake()
    {
        base.Awake(); // base awake
                      // hardcoded values here
        abilityName = "Speed Thrust";
        description = "Temporarily increases speed.";
        ID = AbilityID.SpeedThrust;
        cooldownDuration = 10;
        CDRemaining = cooldownDuration;
        activeDuration = 5;
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
            (Core as Craft).enginePower -= 100F * Mathf.Pow(abilityTier, 1.5F);
            (Core as Craft).speed -= boost * abilityTier;
            (Core as Craft).CalculatePhysicsConstants();
        } // bring the engine power back
        base.Deactivate();
    }

    /// <summary>
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        // adjust fields
        if(craft) {
            var enginePower = (Core as Craft).enginePower;
            activated = true;
            (Core as Craft).enginePower += 100F * Mathf.Pow(abilityTier, 1.5F);
            (Core as Craft).speed += boost * abilityTier;
            (Core as Craft).CalculatePhysicsConstants();
        } // change engine power
        AudioManager.PlayClipByID("clip_activateability", transform.position);
        isActive = true; // set to active
        isOnCD = true; // set to on cooldown
        base.Execute();
    }
}
