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
        if (Core is IOwner owner)
        {
            owner.SetIntrinsicCommandLimit(owner.GetIntrinsicCommandLimit() - commandUnitIncrease * Mathf.Max(1, abilityTier));
        }

        base.Deactivate();
    }

    protected override void Execute()
    {
        if (Core is IOwner owner)
        {
            owner.SetIntrinsicCommandLimit(owner.GetIntrinsicCommandLimit() + commandUnitIncrease * Mathf.Max(1, abilityTier));
        }

        base.Execute();
    }
}
