using UnityEngine;

/// <summary>
/// Gives a temporary increase to the core's engine power
/// </summary>
public class SpeedThrust : ActiveAbility
{
    Craft craft;
    public static readonly float boost = 20;
    public static readonly float duration = 5;

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        abilityName = "Speed Thrust";
        description = "Temporarily increases speed.";
        ID = AbilityID.SpeedThrust;
        cooldownDuration = 10;
        activeDuration = duration;
        energyCost = 50;
        if (Core && !(Core as Craft))
        {
            Debug.LogError("Why did you add Speed Thrust to a non-moving entity? Weirdo!");
        }

        craft = Core as Craft;
    }

    /// <summary>
    /// Returns the engine power to the original value
    /// </summary>
    public override void Deactivate()
    {
        if (craft)
        {
            craft.speed -= boost * abilityTier;
            craft.CalculatePhysicsConstants();
            base.Deactivate();
        }
    }

    public override void ActivationCosmetic(Vector3 targetPos)
    {
        base.ActivationCosmetic(targetPos);
        Execute();
    }

    /// <summary>
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        if (craft)
        {
            craft.speed += boost * abilityTier;
            craft.CalculatePhysicsConstants();
            AudioManager.PlayClipByID("clip_activateability", transform.position);
            base.Execute();
        }
    }
}
