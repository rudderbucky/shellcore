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
        description = "Passively increases the maximum allowed number of controlled units by " + commandUnitIncrease + ".";
    }

    public override void Deactivate()
    {
        (Core as IOwner).SetIntrinsicCommandLimit((Core as IOwner).GetIntrinsicCommandLimit() - commandUnitIncrease);
        base.Deactivate();
    }

    public override void SetDestroyed(bool input)
    {
        base.SetDestroyed(input);
    }

    protected override void Execute()
    {
        (Core as IOwner).SetIntrinsicCommandLimit((Core as IOwner).GetIntrinsicCommandLimit() + commandUnitIncrease);
        base.Execute();
    }
}
