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
	public override void SetDestroyed(bool input)
    {
        float[] maxHealths = Core.GetMaxHealth();
		maxHealths[index] -= max * abilityTier;
		Core.SetMaxHealth(maxHealths, false);
        base.SetDestroyed(input);
    }

    protected override void Execute()
    {
        float[] maxHealths = Core.GetMaxHealth();
		maxHealths[index] += max * abilityTier;
		Core.SetMaxHealth(maxHealths, true);
    }
}
