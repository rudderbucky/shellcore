using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellMax : PassiveAbility {

	public int index;
	public static readonly int max = 250;

	public void Initialize() {
		ID = index + 18;
        if (ID == 19) // Avoid ID conflict
            ID = 23;
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
