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

    public override void SetDestroyed(bool input)
    {
        if (input && activated) 
        {
            (Core as Craft).speed -= boost * abilityTier;
            (Core as Craft).CalculatePhysicsConstants();
        }
        base.SetDestroyed(input);
    }

    protected override void Execute()
    {
        activated = true;
        (Core as Craft).speed += boost * abilityTier;
        (Core as Craft).CalculatePhysicsConstants();
    }
}
