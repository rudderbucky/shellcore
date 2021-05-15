using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IShipStatsDatabase {
	List<DisplayPart> GetParts();
	BuilderMode GetMode();
	int GetBuildValue();
	int GetBuildCost();
}
public class ShipBuilderShipStatsDisplay : MonoBehaviour {

	public ShipBuilderCursorScript cursorScript;
	public IShipStatsDatabase statsDatabase;
	public Text display;
	public Text regenDisplay;
	
	void Start() {
		if(statsDatabase == null) statsDatabase = cursorScript;
	}
	// Update is called once per frame
	void Update () {
		float[] totalHealths = new float[] {1000,250,500};
		float[] totalRegens = new float[] {60,0,60};
		float shipMass = 1;
		float enginePower = 200;
		float weight = Entity.coreWeight;
		float speed = Craft.initSpeed;
		foreach(DisplayPart part in statsDatabase.GetParts()) {
			switch(part.info.abilityID) {
				case 13:
					enginePower *= Mathf.Pow(1.1F, part.info.tier);
					speed += 15 * part.info.tier;
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
			weight += blueprint.mass * Entity.weightMultiplier;
		}
		string buildStat = "";
		if(statsDatabase.GetMode() == BuilderMode.Yard || statsDatabase.GetMode() == BuilderMode.Workshop) {
			buildStat = "\nTOTAL BUILD VALUE: \n" + statsDatabase.GetBuildValue() + " CREDITS";
		} else {	
			string colorTag = "<color=white>";
			if(cursorScript.buildCost > 0) colorTag = "<color=red>";
			else if(cursorScript.buildCost < 0) colorTag = "<color=lime>";
			buildStat = "TOTAL BUILD COST: " + "\n" + colorTag + statsDatabase.GetBuildCost() + " CREDITS</color>";
		}
		display.text = "SHELL: " + totalHealths[0] + "\n"
		+              "CORE: " + totalHealths[1] + "\n"
		+              "ENERGY: " + totalHealths[2] + "\n"
		+ 			   "SPEED: " + (int)Craft.GetPhysicsSpeed(speed, weight) + "\n"
		+			   "WEIGHT: " + (int)weight + "\n"
		+              buildStat;
		regenDisplay.text = "REGEN: " + totalRegens[0] + "\n\n" + "REGEN: " + totalRegens[2];
	}
}
