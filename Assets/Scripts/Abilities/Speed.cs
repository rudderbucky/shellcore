using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speed : PassiveAbility {

    private bool activated;
    protected override void Awake()
    {
        ID = 13;
        base.Awake();
        abilityName = "Speed";
        description = "Passively increases speed.";
    }

    public override void SetDestroyed(bool input)
    {
        var enginePower = (Core as Craft).enginePower;
        if (input && activated) 
        {
            (Core as Craft).enginePower += 75 * abilityTier; //Mathf.Pow(enginePower, 1/(abilityTier/6 + 1.1F));
        }
        base.SetDestroyed(input);
    }

    protected override void Execute()
    {
        var enginePower = (Core as Craft).enginePower;
        if(enginePower <= 1000) {
            activated = true;
            (Core as Craft).enginePower = 75 * abilityTier;
        } 
        else activated = false;
    }
}
