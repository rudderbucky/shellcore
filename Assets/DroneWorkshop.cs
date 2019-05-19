using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum DroneWorkshopPhase {
	SelectionPhase,
	BuildPhase
}
public class DroneWorkshop : ShipBuilder
{
	DroneWorkshopPhase phase;
    public void InitializeSelectionPhase() {
        //initialize window on screen
		if(initialized) CloseUI(false); // prevent initializing twice by closing UI if already initialized
		initialized = true;
		Activate();
		cursorScript.gameObject.SetActive(false);

		searcherString = "";
		contentsArray = new Transform[] {smallContents, mediumContents, largeContents};
		contentTexts = new GameObject[] {smallText, mediumText, largeText};
		foreach(GameObject obj in contentTexts) {
			obj.SetActive(false);
		}

		player.SetIsInteracting(true);
		partDict = new Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript>();

		// hide the buttons and yard tips if interacting with a trader


        List<EntityBlueprint.PartInfo> parts = player.GetInventory();
		if(parts != null) {
			for(int i = 0; i < parts.Count; i++) {
				parts[i] = CullSpatialValues(parts[i]);
			}
		}
		foreach(EntityBlueprint.PartInfo part in parts) {
            if(part.abilityID != 10) continue;

			if(!partDict.ContainsKey(part)) 
			{
				int size = ResourceManager.GetAsset<PartBlueprint>(part.partID).size;
				ShipBuilderInventoryScript invButton = Instantiate(buttonPrefab, 
					contentsArray[size]).GetComponent<ShipBuilderInventoryScript>();
				partDict.Add(part, invButton);
				contentTexts[size].SetActive(true);
				invButton.part = part;
				invButton.cursor = cursorScript;
				invButton.IncrementCount();
				invButton.mode = BuilderMode.Yard;
			} else partDict[part].IncrementCount();
		}
		if(player.GetTractorTarget() && player.GetTractorTarget().GetComponent<ShellPart>()) {
			var part = player.GetTractorTarget().GetComponent<ShellPart>().info;
			part = CullSpatialValues(part);
			if(!partDict.ContainsKey(part)) {
				int size = ResourceManager.GetAsset<PartBlueprint>(part.partID).size;
				var button = Instantiate(buttonPrefab, contentsArray[size]).GetComponent<ShipBuilderInventoryScript>();
				contentTexts[size].SetActive(true);
				button.cursor = cursorScript;
				button.part = part;
				button.IncrementCount();
				partDict.Add(part, button);
			} else partDict[part].IncrementCount();
			player.cursave.partInventory.Add(part);
			Destroy(player.GetTractorTarget().gameObject);
		}

		phase = DroneWorkshopPhase.SelectionPhase;
		// activate windows
		gameObject.SetActive(true);
	}
}
