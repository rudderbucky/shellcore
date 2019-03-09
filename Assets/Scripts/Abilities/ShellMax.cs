using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellMax : PassiveAbility {

	public int index;

	public void Initialize() {
		ID = index + 18;
	}
	public override void SetDestroyed(bool input)
    {
        float[] maxHealths = Core.GetMaxHealth();
		maxHealths[index] -= 250 * abilityTier;
		Core.SetMaxHealth(maxHealths);
        base.SetDestroyed(input);
    }

    protected override void Execute()
    {
        float[] maxHealths = Core.GetMaxHealth();
		maxHealths[index] += 250 * abilityTier;
		Core.SetMaxHealth(maxHealths);
    }
}
