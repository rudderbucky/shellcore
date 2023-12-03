/// <summary>
/// Temporarily increases the craft's regen
/// </summary>
public class ActiveRegen : ActiveAbility
{
    public static readonly float[] healAmounts = { 500, 100, 500 };
    public int index;

    public void Initialize()
    {
        cooldownDuration = 15;
        activeDuration = 8;
        energyCost = 100;
        chargeDuration = 3f;

        switch (index)
        {
            case 0:
                ID = AbilityID.ActiveShellRegen;
                break;
            case 1:
                ID = AbilityID.ActiveCoreRegen;
                break;
            case 2:
                ID = AbilityID.ActiveEnergyRegen;
                energyCost = 0;
                break;
        }
    }

    /// <summary>
    /// Returns the regen back to previous
    /// </summary>
    public override void Deactivate()
    {
        base.Deactivate();
        if (Core)
        {
            float[] regens = Core.GetRegens();
            regens[index] -= healAmounts[index] * abilityTier;
            Core.SetRegens(regens);
        }
    }

    /// <summary>
    /// Increases a regen
    /// </summary>
    protected override void Execute()
    {
        AudioManager.PlayClipByID("clip_activateability", transform.position);
        float[] regens = Core.GetRegens();
        regens[index] += healAmounts[index] * abilityTier;
        Core.SetRegens(regens);
        base.Execute();
    }
}
