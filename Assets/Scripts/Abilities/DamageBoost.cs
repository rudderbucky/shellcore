using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Temporarily increases the craft's damage multiplier
/// </summary>
public class DamageBoost: ActiveAbility
{
    float activationDelay = 2f; // the delay between clicking the ability and its activation
    float activationTime = 0f;
    bool trueActive = false;

    protected override void Awake()
    {
        base.Awake(); // base awake
                      // hardcoded values here
        ID = 25;
        cooldownDuration = 20;
        CDRemaining = cooldownDuration;
        activeDuration = 5;
        activeTimeRemaining = activeDuration;
        energyCost = 200;
    }

    /// <summary>
    /// Returns the engine power to the original value
    /// </summary>
    protected override void Deactivate()
    {
        Core.damageAddition -= 150;
        ToggleIndicator(true);
        trueActive = false;
    }

    public override void Tick(string key)
    {
        base.Tick(key);
        if (isOnCD && Time.time > activationTime && !trueActive && GetActiveTimeRemaining() > 0)
        {
            if (Core)
                Core.damageAddition += 150;
            AudioManager.PlayClipByID("clip_activateability", transform.position);
            trueActive = true;
            ToggleIndicator(true);
        }
    }

    /// <summary>
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        activationTime = Time.time + activationDelay;
        isOnCD = true; // set to on cooldown
        isActive = true; // set to "active"
        ToggleIndicator(false);
    }
}
