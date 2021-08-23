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

    public override void Activate()
    {
        if (State != AbilityState.Active && State != AbilityState.Destroyed)
        {
            State = AbilityState.Active;
            Execute();
        }
    }

    public override void Tick()
    {
    }
}
