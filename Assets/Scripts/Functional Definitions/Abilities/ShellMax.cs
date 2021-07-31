public class ShellMax : PassiveAbility
{
    public int index;
    public static readonly int[] maxes = new int[3] { 1000, 400, 400 };

    public void Initialize()
    {
        switch (index)
        {
            case 0:
                ID = AbilityID.ShellMax;
                break;
            case 1:
                ID = AbilityID.CoreMax;
                break;
            case 2:
                ID = AbilityID.EnergyMax;
                break;
        }
    }

    public override void Deactivate()
    {
        var stacks = Core.PassiveMaxStacks;
        stacks[index] -= abilityTier;
        Core.PassiveMaxStacks = stacks;

        base.Deactivate();
    }

    protected override void Execute()
    {
        var stacks = Core.PassiveMaxStacks;
        stacks[index] += abilityTier;
        Core.PassiveMaxStacks = stacks;
    }
}
