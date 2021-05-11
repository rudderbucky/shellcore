using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives a temporary increase to the core's engine power
/// </summary>
public class SpeedThrust : ActiveAbility
{
    Craft craft;
    public static readonly float boost = 20;
    protected override void Awake()
    {
        base.Awake(); // base awake
                      // hardcoded values here
        abilityName = "Speed Thrust";
        description = "Temporarily increases speed.";
        ID = AbilityID.SpeedThrust;
        cooldownDuration = 10;
        activeDuration = 5;
        energyCost = 50;
    }

    private void Start()
    {
        if(!(Core as Craft)) Debug.LogError("Why did you add Speed Thrust to a non-moving entity? Weirdo!");
        craft = Core as Craft;
    }
    /// <summary>
    /// Returns the engine power to the original value
    /// </summary>
    public override void Deactivate()
    {
        craft.speed -= boost * abilityTier;
        craft.CalculatePhysicsConstants();
        base.Deactivate();
    }

    /// <summary>
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        craft.speed += boost * abilityTier;
        craft.CalculatePhysicsConstants();
        AudioManager.PlayClipByID("clip_activateability", transform.position);
        base.Execute();
    }
}
