using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Heals the shell of the associated craft
/// </summary>
public class ShellHeal : Ability
{
    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        ID = 2;
        cooldownDuration = 10;
        CDRemaining = 10;
        energyCost = 35;
    }

    /// <summary>
    /// Heals the shell of the core (doesn't heal and refunds the energy used if it would overheal)
    /// </summary>
    protected override void Execute()
    {
        if (Core.GetHealth()[0] <= Core.GetMaxHealth()[0] - 25) // check for overheal
        {
            Core.TakeDamage(-25, 0); // heal
            isOnCD = true; // set on cooldown
        }
        else {
            Core.TakeEnergy(-energyCost); // refund energy
        }
    }
}