using UnityEngine;

/// <summary>
/// Gives a temporary increase to the core's engine power
/// </summary>
public class Unload : ActiveAbility
{
    private float gcdTemp;
    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        abilityName = "Unload";
        description = "Temporarily reduces Global Cooldown.";
        ID = AbilityID.Unload;
        cooldownDuration = 10;
        activeDuration = 5;
        energyCost = 50;
    }

    /// <summary>
    /// Returns the engine power to the original value
    /// </summary>
    public override void Deactivate()
    {
        Core.WeaponGCD = Core.WeaponGCD + gcdTemp;
        base.Deactivate();
    }

    /// <summary>
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        gcdTemp = Core.WeaponGCD * ((float)abilityTier / ((float)abilityTier + 1));
        Core.WeaponGCD = Core.WeaponGCD - gcdTemp;
        AudioManager.PlayClipByID("clip_buff", transform.position);
        base.Execute();
    }
}
