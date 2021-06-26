using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command : PassiveAbility
{
    public static readonly int commandUnitIncrease = 3;
    protected override void Awake()
    {
        ID = AbilityID.Command;
        base.Awake();
        abilityName = "Command";
        description = "Passively increases the maximum allowed number of controlled units";
    }

    public override void Deactivate()
    {
        if(Core is IOwner){
            (Core as IOwner).SetIntrinsicCommandLimit((Core as IOwner).GetIntrinsicCommandLimit() - commandUnitIncrease * Mathf.Max(1,abilityTier));
        }
        base.Deactivate();
    }

    public override void SetDestroyed(bool input)
    {
        base.SetDestroyed(input);
    }

    protected override void Execute()
    {
        if(Core is IOwner){
            (Core as IOwner).SetIntrinsicCommandLimit((Core as IOwner).GetIntrinsicCommandLimit() + commandUnitIncrease * Mathf.Max(1,abilityTier));
        }
        base.Execute();
    }
}
