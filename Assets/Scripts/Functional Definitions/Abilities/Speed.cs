using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speed : PassiveAbility {

    private bool activated;
    public static readonly float boost = 10;
    protected override void Awake()
    {
        ID = AbilityID.Speed;
        base.Awake();
        abilityName = "Speed";
        description = "Passively increases speed.";
    }

    public override void Deactivate()
    {
        if(Core as Craft)
        {
            (Core as Craft).speed -= boost * abilityTier;
            (Core as Craft).CalculatePhysicsConstants();
        }
        base.Deactivate();
    }

    protected override void Execute()
    {
        if(Core as Craft)
        {
            activated = true;
            (Core as Craft).speed += boost * abilityTier;
            (Core as Craft).CalculatePhysicsConstants();
        }
        else Debug.LogError("Why did you add a Speed part to a non-moving entity? Weirdo!");
    }
}
