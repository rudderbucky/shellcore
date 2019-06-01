using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
	protected Dictionary<DWInventoryButton, EntityBlueprint.PartInfo> partDict;
	public GameObject displayButtonPrefab;
	public DWSelectionDisplayHandler selectionDisplay;
	public GameObject selectionPhaseParent;
	public GameObject buildPhaseParent;
	public Image coreImage;
	public Image shellImage;
	public GameObject partPrefab;

    public void InitializeSelectionPhase() {

		selectionPhaseParent.SetActive(true);
		buildPhaseParent.SetActive(false);
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
		partDict = new Dictionary<DWInventoryButton, EntityBlueprint.PartInfo>();

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
				button.workshop = this;
				contentTexts[size].SetActive(true);
				button.part = part;
				partDict.Add(button, part);
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
			invButton.workshop = this;
			partDict.Add(invButton, part);
			contentTexts[size].SetActive(true);
			invButton.part = part;
		}
	}
    public BuilderMode GetMode()
    {
        return BuilderMode.Yard;
    }

    public void DispatchPart(ShipBuilderPart part, ShipBuilder.TransferMode mode)
    {
        throw new System.NotImplementedException();
    }

	public void CloseUI(bool val) {
		player.SetIsInteracting(false);
		base.CloseUI();
	}

	public override void CloseUI() {
		CloseUI(false);
	}

    public void UpdateChain()
    {
        
    }

	public EntityBlueprint.PartInfo? GetButtonPartCursorIsOn() {
		foreach(DWInventoryButton inv in partDict.Keys) {
			if(RectTransformUtility.RectangleContainsScreenPoint(inv.GetComponent<RectTransform>(), Input.mousePosition) 
				&& inv.gameObject.activeSelf) {
				return inv.part;
			}
		}
		return null;
	}

	public static DroneSpawnData ParseDronePart(EntityBlueprint.PartInfo part) {
		if(part.abilityID != 10) Debug.Log("Passed part is not a drone spawner!");
		var data = ScriptableObject.CreateInstance<DroneSpawnData>();
		JsonUtility.FromJsonOverwrite(part.secondaryData, data);
		return data;
	}
	public void InitializeBuildPhase(EntityBlueprint blueprint) {
		selectionPhaseParent.SetActive(false);
		buildPhaseParent.SetActive(true);
		cursorScript.gameObject.SetActive(true);
		cursorScript.SetMode(BuilderMode.Workshop);
		LoadBlueprint(blueprint);
	}

	public void LoadBlueprint(EntityBlueprint blueprint) {
		shellImage.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
		if(shellImage.sprite) {
			shellImage.enabled = true;
			shellImage.color = FactionColors.colors[0];
			shellImage.rectTransform.sizeDelta = shellImage.sprite.bounds.size * 100;

			// orient shell image so relative center stays the same regardless of shell tier
			shellImage.rectTransform.anchoredPosition = -shellImage.sprite.pivot + shellImage.rectTransform.sizeDelta / 2;
		} else shellImage.enabled = false;

		coreImage.rectTransform.anchoredPosition = -shellImage.rectTransform.anchoredPosition;
		coreImage.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);
		if(coreImage.sprite) {
			coreImage.material = ResourceManager.GetAsset<Material>("material_color_swap");
			coreImage.color = FactionColors.colors[0];
			coreImage.preserveAspect = true;
			coreImage.rectTransform.sizeDelta = coreImage.sprite.bounds.size * 110;
		} else coreImage.enabled = false;

		foreach(EntityBlueprint.PartInfo part in blueprint.parts) {
			var p = Instantiate(partPrefab, cursorScript.transform.parent).GetComponent<ShipBuilderPart>();
			p.cursorScript = cursorScript;
			cursorScript.parts.Add(p);
			p.info = part;
			p.SetLastValidPos(part.location);
			p.isInChain = true;
			p.validPos = true;
			p.InitializeMode(BuilderMode.Workshop);
		}
	}
	
	/// prevent dragging the window if the mouse is on the grid
	public override void OnPointerDown(PointerEventData eventData) {
		if(RectTransformUtility.RectangleContainsScreenPoint(cursorScript.grid, Input.mousePosition)) return;
		base.OnPointerDown(eventData);
	}
}
