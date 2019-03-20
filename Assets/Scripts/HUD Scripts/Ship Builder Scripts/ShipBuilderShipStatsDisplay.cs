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
			switch(part.info.abilityID) {
				case 13:
					enginePower *= Mathf.Pow(1.1F, part.info.tier);
					break;
				case 17:
					totalRegens[0] += 50 * part.info.tier;
					break;
				case 18:
					totalHealths[0] += 250 * part.info.tier;
					break;
				case 19:
					totalRegens[2] += 50 * part.info.tier;
					break;
				case 20:
					totalHealths[2] += 250 * part.info.tier;
					break;
			}
			PartBlueprint blueprint = ResourceManager.GetAsset<PartBlueprint>(part.info.partID);
			totalHealths[0] += blueprint.health / 2;
			totalHealths[1] += blueprint.health / 4;
			shipMass += blueprint.mass;
		}
		string colorTag = "<color=white>";
		if(cursorScript.buildCost > 0) colorTag = "<color=red>";
		else if(cursorScript.buildCost < 0) colorTag = "<color=lime>";
		display.text = "SHELL: " + totalHealths[0] + "\n"
		+              "CORE: " + totalHealths[1] + "\n"
		+              "ENERGY: " + totalHealths[2] + "\n"
		+              "MASS: " + Mathf.RoundToInt(shipMass * 100) / 100F 
		+ 			   "\nENGINE POWER: " + enginePower
		+              "\nTOTAL BUILD COST: " + "\n" + colorTag + cursorScript.buildCost + " CREDITS</color>";
		regenDisplay.text = "REGEN: " + totalRegens[0] + "\n\n" + "REGEN: " + totalRegens[2];
	}
}
