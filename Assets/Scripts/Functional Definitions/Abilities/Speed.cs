using UnityEngine;

public class Speed : PassiveAbility
{
    public static readonly float boost = 10;

    protected override void Awake()
    {
        ID = AbilityID.Speed;
        base.Awake();
        abilityName = "Speed";
        description = "Passively increases speed.";
    }

    public override void Deactivate()
    {
        if (Core is Craft craft)
        {
            craft.speed -= boost * abilityTier;
            craft.CalculatePhysicsConstants();
        }

        base.Deactivate();
    }

    protected override void Execute()
    {
        if (Core is Craft craft)
        {
            craft.speed += boost * abilityTier;
            craft.CalculatePhysicsConstants();
        }
        else if (Core)
        {
            Debug.LogError("Why did you add a Speed part to a non-moving entity? Weirdo!");
        }
    }
}
