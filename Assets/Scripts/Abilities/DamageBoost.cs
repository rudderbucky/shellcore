using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Temporarily increases the craft's damage multiplier
/// </summary>
public class DamageBoost: ActiveAbility
{
    Craft craft;
    float activationDelay = 2f; // the delay between clicking the ability and its activation
    float activationTime = 0f;

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

    private void Start()
    {
        craft = Core as Craft;
    }
    /// <summary>
    /// Returns the engine power to the original value
    /// </summary>
    protected override void Deactivate()
    {
        craft.damageAddition -= 150;
        ToggleIndicator(true);
    }

    public override void Tick(string key)
    {
        base.Tick(key);
        if (isOnCD && !isActive && activationTime > Time.time)
        {
            isActive = true; // set to active
            if (craft)
                craft.damageAddition += 150;
            AudioManager.PlayClipByID("clip_activateability", transform.position);
        }
    }

    /// <summary>
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        activationTime = Time.time + activationDelay;
        isOnCD = true; // set to on cooldown
        ToggleIndicator(true);
    }
}
