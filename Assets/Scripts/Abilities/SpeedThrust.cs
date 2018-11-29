using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives a temporary increase to the core's engine power
/// </summary>
public class SpeedThrust : ActiveAbility
{
    /*protected override void Start()
    {
        ID = 1;
        cooldownDuration = 5;
        CDRemaining = cooldownDuration;
        activeDuration = 3;
        activeTimeRemaining = activeDuration;
        energyCost = 50;
    }*/

    protected override void Awake()
    {
        base.Awake();
        ID = 1;
        cooldownDuration = 5;
        CDRemaining = cooldownDuration;
        activeDuration = 3;
        activeTimeRemaining = activeDuration;
        energyCost = 50;
    }

    //public override int GetID() {
    //    return 1;
    //}
    /// <summary>
    /// Returns the engine power to the original value
    /// </summary>
    protected override void Deactivate()
    {
        core.enginePower /= 2;
    }

    /// <summary>
    /// Increases core engine power
    /// </summary>
    protected override void Execute()
    {
        // adjust fields
        core.enginePower *= 2;
        isActive = true;
        isOnCD = true;
    }
}
