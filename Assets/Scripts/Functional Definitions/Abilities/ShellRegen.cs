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

    public override void Deactivate()
    {
        float[] regens = Core.GetRegens();
        regens[index] -= regen * abilityTier;
        Core.SetRegens(regens);
        base.Deactivate();
    }


    protected override void Execute()
    {
        float[] regens = Core.GetRegens();
		regens[index] += regen * abilityTier;
		Core.SetRegens(regens);
    }
}
