using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Heals the shell of the associated craft
/// </summary>
public class HealthHeal : Ability
{

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
        switch (type)
        {
            case HealingType.shell:
                abilityName = "Shell Boost";
                description = "Instantly heal 300 shell.";
                ID = 2;
                break;
            case HealingType.core:
                abilityName = "Core Heal";
                description = "Instantly heal 300 core.";
                ID = 11;
                break;
            case HealingType.energy:
                abilityName = "Energy";
                description = "Instantly heal 300 energy.";
                ID = 12;
                energyCost = 0;
                break;
        }
        cooldownDuration = 10;
        CDRemaining = 10;
    }

    /// <summary>
    /// Heals the shell of the core (doesn't heal and refunds the energy used if it would overheal)
    /// </summary>
    protected override void Execute()
    {
        if (Core.GetHealth()[(int)type] <= Core.GetMaxHealth()[(int)type]) // check for overheal
        {
            switch (type)
            {
                case HealingType.core:
                    Core.TakeDamage(-300, 1); // heal core
                    break;
                case HealingType.energy:
                    Core.TakeEnergy(-300);
                    break;
                case HealingType.shell:
                    Core.TakeDamage(-300, 0); // heal energy
                    break;
            }
            Core.TakeDamage(-25, 0); // heal
            isOnCD = true; // set on cooldown
        }
        else {
            Core.TakeEnergy(-energyCost); // refund energy
        }
    }
}