using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellMax : PassiveAbility {

	public int index;

	void Start() {
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
