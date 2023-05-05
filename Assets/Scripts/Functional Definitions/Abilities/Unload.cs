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
        gcdTemp = Core.WeaponGCD * ((float)Mathf.Min(abilityTier, 1) / ((float)Mathf.Max(abilityTier, 1) + 1));
        Core.WeaponGCD = Core.WeaponGCD - gcdTemp;
        AudioManager.PlayClipByID("clip_unload", transform.position);
        base.Execute();
    }
}
