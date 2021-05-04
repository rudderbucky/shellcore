using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Heals the shell of the associated craft
/// </summary>
public class HealthHeal : Ability
{

    public void Initialize() {
        switch (type)
        {
            case HealingType.shell:
                abilityName = "Shell Boost";
                description = "Instantly heal " + heals[0] + " shell.";
                ID = AbilityID.ShellBoost;
                break;
            case HealingType.core:
                abilityName = "Core Heal";
                description = "Instantly heal " + heals[1] + " core.";
                ID = AbilityID.CoreHeal;
                break;
            case HealingType.energy:
                abilityName = "Energy";
                description = "Instantly heal " + heals[2] + " energy.";
                ID = AbilityID.Energy;
                energyCost = 0;
                break;
        }
    }
    public enum HealingType
    {
        shell,
        core,
        energy
    }

    public HealingType type;

    protected override void Awake()
    {
        base.Awake(); // base awake
                      // hardcoded values here

        energyCost = 35;

        // remember, the ID change works here 
        // because awake only triggers AFTER the thread which instantiated the ability is completely executed
        cooldownDuration = 10;
    }

    public static readonly int[] heals = new int[] {500, 500, 300};

    /// <summary>
    /// Heals the shell of the core (doesn't heal and refunds the energy used if it would overheal)
    /// </summary>
    protected override void Execute()
    {
        if (Core.GetHealth()[(int)type] < Core.GetMaxHealth()[(int)type]) 
        {
            AudioManager.PlayClipByID("clip_healeffect", transform.position);
            switch (type)
            {
                case HealingType.core:
                    Core.TakeCoreDamage(-heals[1] * abilityTier); // heal core
                    break;
                case HealingType.energy:
                    Core.TakeEnergy(-heals[2] * abilityTier);
                    break;
                case HealingType.shell:
                    Core.TakeShellDamage(-heals[0] * abilityTier, 0, GetComponentInParent<Entity>()); // heal energy
                    break;
            }
            base.Execute();
        }
        else {
            Core.TakeEnergy(-energyCost); // refund energy
            startTime = Time.time - cooldownDuration;
        }
    }
}