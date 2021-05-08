using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveAbility : Ability
{
    protected override void Awake()
    {
        base.Awake();
        abilityName = "Passive Ability";
    }

    protected override void Execute()
    {
        
    }

    public override void Tick()
    {
        if (State != AbilityState.Active)
        {
            State = AbilityState.Active;
            Execute();
        }
    }
}
