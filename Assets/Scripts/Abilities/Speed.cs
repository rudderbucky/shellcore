using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speed : PassiveAbility {

    private bool activated;
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
            (Core as Craft).enginePower -= 50F * Mathf.Pow(abilityTier, 1.5F); //Mathf.Pow(enginePower, 1/(abilityTier/6 + 1.1F));
        }
        base.SetDestroyed(input);
    }

    protected override void Execute()
    {
        var enginePower = (Core as Craft).enginePower;
        activated = true;
        (Core as Craft).enginePower += 50F * Mathf.Pow(abilityTier, 1.5F);
    }
}
