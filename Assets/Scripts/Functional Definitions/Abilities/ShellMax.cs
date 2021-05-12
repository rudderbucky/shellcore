using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellMax : PassiveAbility {

	public int index;
	public static readonly int max = 250;

	public void Initialize() {
        switch(index)
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
        float[] maxHealths = Core.GetMaxHealth();
        var percentage = Core.GetHealth()[index] / maxHealths[index];
        maxHealths[index] -= max * abilityTier;
        Core.SetMaxHealth(maxHealths, false);

        // Remove a percentage of health from the ship, based on what max health the core had before destruction
        switch(index)
        {
            case 0:
                Core.TakeShellDamage(percentage * max * abilityTier, 0, null);
                break;
            case 1:
                Core.TakeCoreDamage(percentage * max * abilityTier);
                break;
            case 2:
                Core.TakeEnergy(percentage* max * abilityTier);
                break;
        }
        base.Deactivate();
    }

    protected override void Execute()
    {
        float[] maxHealths = Core.GetMaxHealth();
		maxHealths[index] += max * abilityTier;
		Core.SetMaxHealth(maxHealths, true);
    }
}
