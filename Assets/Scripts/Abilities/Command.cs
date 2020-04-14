using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command : PassiveAbility
{
    bool activated = false;
    public static readonly int commandUnitIncrease = 3;
    protected override void Awake()
    {
        ID = 21;
        base.Awake();
        abilityName = "Command";
        description = "Passively increases the maximum allowed number of controlled units by " + commandUnitIncrease + ".";
    }
    public override void SetDestroyed(bool input)
    {
        if(activated)
            (Core as ShellCore).intrinsicCommandLimit -= commandUnitIncrease;
        base.SetDestroyed(input);
    }

    protected override void Execute()
    {
        (Core as ShellCore).intrinsicCommandLimit += commandUnitIncrease;
        activated = true;
    }
}
