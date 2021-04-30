using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertTractor : ActiveAbility
{
    public override void SetTier(int abilityTier)
    {
        base.SetTier(abilityTier);
        activeDuration = 5 * abilityTier;
        print(activeDuration);
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
                      // hardcoded values here
        ID = AbilityID.InvertTractor;
        cooldownDuration = 20;
        energyCost = 250;
    }

    /// <summary>
    /// Returns the engine power to the original value
    /// </summary>
    public override void Deactivate()
    {
        Core.tractorSwitched = false;
        base.Deactivate();
    }

    /// <summary>
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        Core.tractorSwitched = true;
        AudioManager.PlayClipByID("clip_buff", transform.position);
        base.Execute();
    }
}
