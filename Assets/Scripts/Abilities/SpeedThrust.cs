using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives a temporary increase to the core's engine power
/// </summary>
public class SpeedThrust : ActiveAbility
{
    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        ID = 1;
        cooldownDuration = 5;
        CDRemaining = cooldownDuration;
        activeDuration = 3;
        activeTimeRemaining = activeDuration;
        energyCost = 50;
    }

    /// <summary>
    /// Returns the engine power to the original value
    /// </summary>
    protected override void Deactivate()
    {
        core.enginePower -= 200; // bring the engine power back (will change to vary as Speed Thrust is tiered)
    }

    /// <summary>
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        // adjust fields
        core.enginePower += 200; // add 200 to engine power (will change to vary as Speed Thrust is tiered)
        isActive = true; // set to active
        isOnCD = true; // set to on cooldown
    }
}
