using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellRegen : PassiveAbility {

	public int index;
	void Start() {
		ID = index + 17;
	}
	public override void SetDestroyed(bool input)
    {
        float[] regens = Core.GetRegens();
		regens[index] -= 50 * abilityTier;
		Core.SetRegens(regens);
        base.SetDestroyed(input);
    }

    protected override void Execute()
    {
        float[] regens = Core.GetRegens();
		regens[index] += 50 * abilityTier;
		Core.SetRegens(regens);
    }
}
