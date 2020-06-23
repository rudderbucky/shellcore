using System.Linq;

/// <summary>
/// Heals all allies in range
/// </summary>
public class Retreat : Ability
{
    protected override void Awake()
    {
        base.Awake(); // base awake
                      // hardcoded values here
        ID = 28;
        energyCost = 200;
        cooldownDuration = 30;
        CDRemaining = cooldownDuration;
    }

    /// <summary>
    /// Heals all nearby allies
    /// </summary>
    protected override void Execute()
    {
        if (Core is Craft)
        {
            (Core as Craft).Respawn();
            Retreat r = Core.GetAbilities().First((a)=> { return a is Retreat; }) as Retreat;
            if (r != null)
            {
                r.isOnCD = true;
            }
        }
    }
}