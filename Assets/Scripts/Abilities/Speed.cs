using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speed : PassiveAbility {

    protected override void Awake()
    {
        ID = 13;
        base.Awake();
        abilityName = "Speed";
        description = "Passively increases speed.";
    }

    public override void SetDestroyed(bool input)
    {
        if (input) (Core as Craft).enginePower /= Mathf.Pow(1.15F, abilityTier);
        base.SetDestroyed(input);
    }

    protected override void Execute()
    {
        (Core as Craft).enginePower *= Mathf.Pow(1.15F, abilityTier);
    }
}
