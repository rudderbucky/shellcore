using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Temporarily increases the craft's regen
/// </summary>
public class ActiveRegen : ActiveAbility, IChargeOnUseThenBlink
{
    const float healAmount = 100f;
    public int index;

    public void Initialize()
    {
        cooldownDuration = 20;
        CDRemaining = cooldownDuration;
        activeDuration = 10;
        activeTimeRemaining = activeDuration;
        energyCost = 150;
        activationDelay = 3f;

        switch (index)
        {
            case 0:
                ID = AbilityID.ActiveShellRegen;
                break;
            case 1:
                ID = AbilityID.ActiveCoreRegen;
                break;
            case 2:
                ID = AbilityID.ActiveEnergyRegen;
                energyCost = 0;
                break;
        }
    }

    /// <summary>
    /// Returns the regen back to previous
    /// </summary>
    public override void Deactivate()
    {
        base.Deactivate();
        if (Core && TrueActive)
        {
            float[] regens = Core.GetRegens();
            regens[index] -= healAmount * abilityTier;
            Core.SetRegens(regens);
        }
    }

    public override void Tick(int key)
    {
        base.Tick(key);
        if (isOnCD && Time.time > activationTime && !TrueActive && GetActiveTimeRemaining() > 0)
        {
            if (Core)
            {
                float[] regens = Core.GetRegens();
                regens[index] += healAmount * abilityTier;
                Core.SetRegens(regens);
            }
            AudioManager.PlayClipByID("clip_activateability", transform.position);
        }
    }

    /// <summary>
    /// Increases a regen
    /// </summary>
    protected override void Execute()
    {
        activationTime = Time.time + activationDelay;
        isOnCD = true; // set to on cooldown
        isActive = true; // set to "active"
        base.Execute();
    }
}
