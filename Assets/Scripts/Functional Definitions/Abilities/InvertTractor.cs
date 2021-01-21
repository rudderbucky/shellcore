using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertTractor : ActiveAbility
{
    float activationDelay = 0f; // the delay between clicking the ability and its activation

    public override void SetTier(int abilityTier)
    {
        base.SetTier(abilityTier);
        activeDuration = 5 * abilityTier;
        activeTimeRemaining = activeDuration;
        print(activeDuration);
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
                      // hardcoded values here
        ID = AbilityID.InvertTractor;
        cooldownDuration = 20;
        CDRemaining = cooldownDuration;
        energyCost = 250;
    }

    /// <summary>
    /// Returns the engine power to the original value
    /// </summary>
    protected override void Deactivate()
    {
        Core.tractorSwitched = false;
        ToggleIndicator(true);
        Debug.Log("tes");
    }

    /// <summary>
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        Core.tractorSwitched = true;
        isOnCD = true; // set to on cooldown
        isActive = true; // set to "active"
        AudioManager.PlayClipByID("clip_buff", transform.position);
        ToggleIndicator(false);
    }
}
