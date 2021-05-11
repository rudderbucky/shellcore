using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command : PassiveAbility
{
    bool activated = false;
    public static readonly int commandUnitIncrease = 3;
    protected override void Awake()
    {
        ID = AbilityID.Command;
        base.Awake();
        abilityName = "Command";
        description = "Passively increases the maximum allowed number of controlled units by " + commandUnitIncrease + ".";
    }
    public override void SetDestroyed(bool input)
    {
        if(activated)
        {
            (Core as IOwner).SetIntrinsicCommandLimit((Core as IOwner).GetIntrinsicCommandLimit() - commandUnitIncrease);
            activated = false;
        }
        base.SetDestroyed(input);
    }

    protected override void Execute()
    {
        (Core as IOwner).SetIntrinsicCommandLimit((Core as IOwner).GetIntrinsicCommandLimit() + commandUnitIncrease);
        activated = true;
    }
}
