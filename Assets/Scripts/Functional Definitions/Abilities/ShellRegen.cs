using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellRegen : PassiveAbility {

	public int index;
	public static readonly int[] regens = new int[] {100, 50, 20};
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
        float[] coreRegens = Core.GetRegens();
        coreRegens[index] -= regens[index] * abilityTier;
        Core.SetRegens(coreRegens);
        base.Deactivate();
    }


    protected override void Execute()
    {
        float[] coreRegens = Core.GetRegens();
		coreRegens[index] += regens[index] * abilityTier;
		Core.SetRegens(coreRegens);
    }
}
