using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellRegen : PassiveAbility {

	public int index;
	public static readonly int regen = 50;
	public void Initialize() {
		switch(index)
        {
            case 0:
                ID = AbilityID.ShellRegen;
                break;
            case 1:
                ID = AbilityID.CoreRegen;
                break;
            case 2:
                ID = AbilityID.EnergyRegen;
                break;
        }
	}
	public override void SetDestroyed(bool input)
    {
        float[] regens = Core.GetRegens();
		regens[index] -= regen * abilityTier;
		Core.SetRegens(regens);
        base.SetDestroyed(input);
    }

    protected override void Execute()
    {
        float[] regens = Core.GetRegens();
		regens[index] += regen * abilityTier;
		Core.SetRegens(regens);
    }
}
