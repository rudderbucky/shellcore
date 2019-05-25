using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum DroneWorkshopPhase {
	SelectionPhase,
	BuildPhase
}
public class DroneWorkshop : GUIWindowScripts, IBuilderInterface
{
	DroneWorkshopPhase phase;
	public Vector3 yardPosition;
	bool initialized;
	public ShipBuilderCursorScript cursorScript;
	Transform[] contentsArray;
	public Transform smallContents;
	public Transform mediumContents;
	public Transform largeContents;
	GameObject[] contentTexts;
	public GameObject smallText;
	public GameObject mediumText;
	public GameObject largeText;
	public PlayerCore player;
	protected Dictionary<EntityBlueprint.PartInfo, DWInventoryButton> partDict;
	public GameObject displayButtonPrefab;
	public DWSelectionDisplayHandler selectionDisplay;

    public void InitializeSelectionPhase() {
        //initialize window on screen
		if(initialized) CloseUI(false); // prevent initializing twice by closing UI if already initialized
		initialized = true;
		Activate();
		cursorScript.gameObject.SetActive(false);
		cursorScript.SetBuilder(this);

		contentsArray = new Transform[] {smallContents, mediumContents, largeContents};
		contentTexts = new GameObject[] {smallText, mediumText, largeText};
		foreach(GameObject obj in contentTexts) {
			obj.SetActive(false);
		}

		GetComponentInChildren<ShipBuilderPartDisplay>().Initialize(this);
		player.SetIsInteracting(true);
		partDict = new Dictionary<EntityBlueprint.PartInfo, DWInventoryButton>();

		// hide the buttons and yard tips if interacting with a trader

        List<EntityBlueprint.PartInfo> parts = player.GetInventory();
		if(parts != null) {
			for(int i = 0; i < parts.Count; i++) {
				parts[i] = ShipBuilder.CullSpatialValues(parts[i]);
			}
		}

		AddParts(parts);
		AddParts(player.blueprint.parts);

		if(player.GetTractorTarget() && player.GetTractorTarget().GetComponent<ShellPart>()) {
			var part = player.GetTractorTarget().GetComponent<ShellPart>().info;
			part = ShipBuilder.CullSpatialValues(part);
			if(part.abilityID == 10) {
				int size = ResourceManager.GetAsset<PartBlueprint>(part.partID).size;
				var button = Instantiate(displayButtonPrefab, contentsArray[size]).GetComponent<DWInventoryButton>();
				button.handler = selectionDisplay;
				contentTexts[size].SetActive(true);
				button.part = part;
				partDict.Add(part, button);
			}
			player.cursave.partInventory.Add(part);
			Destroy(player.GetTractorTarget().gameObject);
		}

		phase = DroneWorkshopPhase.SelectionPhase;
		// activate windows
		gameObject.SetActive(true);
	}

	public void AddParts(List<EntityBlueprint.PartInfo> parts) {
		foreach(EntityBlueprint.PartInfo part in parts) {
            if(part.abilityID != 10) continue;
			int size = ResourceManager.GetAsset<PartBlueprint>(part.partID).size;
			DWInventoryButton invButton = Instantiate(displayButtonPrefab, 
				contentsArray[size]).GetComponent<DWInventoryButton>();
			invButton.handler = selectionDisplay;
			partDict.Add(part, invButton);
			contentTexts[size].SetActive(true);
			invButton.part = part;
		}
	}
    public BuilderMode GetMode()
    {
        throw new System.NotImplementedException();
    }

    public void DispatchPart(ShipBuilderPart part, ShipBuilder.TransferMode mode)
    {
        throw new System.NotImplementedException();
    }

	public void CloseUI(bool val) {
		player.SetIsInteracting(false);
		base.CloseUI();
	}
    public void UpdateChain()
    {
        throw new System.NotImplementedException();
    }

	public EntityBlueprint.PartInfo? GetButtonPartCursorIsOn() {
		foreach(DWInventoryButton inv in partDict.Values) {
			if(RectTransformUtility.RectangleContainsScreenPoint(inv.GetComponent<RectTransform>(), Input.mousePosition) 
				&& inv.gameObject.activeSelf) {
				return inv.part;
			}
		}
		return null;
	}
}
