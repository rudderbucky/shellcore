using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ShipBuilderShipStatsDisplay : MonoBehaviour {

	public ShipBuilderCursorScript cursorScript;
	public Text display;
	public Text regenDisplay;
	
	// Update is called once per frame
	void Update () {
		float[] totalHealths = new float[] {1000,250,500};
		float[] totalRegens = new float[] {60,0,60};
		float shipMass = 1;
		float enginePower = 200;
		foreach(ShipBuilderPart part in cursorScript.parts) {
			if(part.info.abilityID == 13) enginePower += 100;
			PartBlueprint blueprint = ResourceManager.GetAsset<PartBlueprint>(part.info.partID);
			totalHealths[0] += blueprint.health / 2;
			totalHealths[1] += blueprint.health / 4;
			shipMass += blueprint.mass;
		}

		display.text = "SHELL: " + totalHealths[0] + "\n"
		+              "CORE: " + totalHealths[1] + "\n"
		+              "ENERGY: " + totalHealths[2] + "\n"
		+              "MASS: " + Mathf.RoundToInt(shipMass * 100) / 100F 
		+ 			   "\nENGINE POWER: " + enginePower
		+              "\nTOTAL VALUE: " + "\nX CREDITS";
		regenDisplay.text = "REGEN: " + totalRegens[0] + "\n\n" + "REGEN: " + totalRegens[2];
	}
}
