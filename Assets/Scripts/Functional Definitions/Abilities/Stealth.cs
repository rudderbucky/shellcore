using UnityEngine;

/// <summary>
/// Makes the craft temporarily invisible
/// </summary>
public class Stealth : ActiveAbility
{
    Craft craft;

    protected override void Awake()
    {
        base.Awake(); // base awake
        ID = AbilityID.Stealth;
        cooldownDuration = 10;
        activeDuration = 4;
        energyCost = 100;
    }

    protected override void Start()
    {
        craft = Core as Craft;
        base.Start();
    }

    /// <summary>
    /// Makes the craft visible again
    /// </summary>
    public override void Deactivate()
    {
        base.Deactivate();
        if (craft.StealthStacks > 0)
        {
            craft.StealthStacks--;
        }
    }

    /// <summary>
    /// Makes the craft invisible
    /// </summary>
    protected override void Execute()
    {
        if (craft)
        {
            // change visibility
            craft.StealthStacks++;

            AudioManager.PlayClipByID("clip_activateability", transform.position);
            base.Execute();
        }
    }
}
