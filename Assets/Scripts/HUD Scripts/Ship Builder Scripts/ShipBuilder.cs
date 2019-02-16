using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipBuilder : MonoBehaviour, IWindow {
	public GameObject SBPrefab;
	public Vector3 yardPosition;
	public Image shell;
	public Image core;
	public ShipBuilderCursorScript cursorScript;
	public GameObject buttonPrefab;
	public PlayerCore player;
	public Transform smallContents;
	public Transform mediumContents;
	public Transform largeContents;
	private Transform[] contentsArray;
	public GameObject smallText;
	public GameObject mediumText;
	public GameObject largeText;
	public GameObject[] contentTexts;
	private string searcherString;
	private bool[] displayingTypes;
	public Image reconstructImage;
	public Text reconstructText;
	Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript> partDict;

	public bool DecrementPartButton(EntityBlueprint.PartInfo info) {
		if(partDict.ContainsKey(CullSpatialValues(info)) && partDict[CullSpatialValues(info)].GetCount() > 0) {
			partDict[CullSpatialValues(info)].DecrementCount();
			return true;
		} else return false;
	}
	public static EntityBlueprint.PartInfo CullSpatialValues(EntityBlueprint.PartInfo x) {
		var part = new EntityBlueprint.PartInfo();
		part.partID = x.partID;
		part.secondaryData = x.secondaryData;
		part.abilityID = x.abilityID;
		part.tier = x.tier;
		return part;
	}
	public void DispatchPart(ShipBuilderPart part) {
		var culledInfo = CullSpatialValues(part.info);
		if(!partDict.ContainsKey(culledInfo)) {
			int size = ResourceManager.GetAsset<PartBlueprint>(part.info.partID).size;
			partDict.Add(culledInfo, Instantiate(buttonPrefab, contentsArray[size]).GetComponent<ShipBuilderInventoryScript>());
			contentTexts[size].SetActive(true);
			partDict[culledInfo].part = culledInfo;
			partDict[culledInfo].cursor = cursorScript;
		}
		partDict[culledInfo].IncrementCount();
		cursorScript.parts.Remove(part);
		Destroy(part.gameObject);
	}

	public static Bounds GetRect(RectTransform rectTransform) {
		Bounds rect = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform.parent, rectTransform);
		rect.center = rectTransform.anchoredPosition;
		rect.size = rect.size * 0.8F;
		return rect;
	}
	public void SetReconstructButton(bool val) {
		if(val) {
			reconstructImage.color = reconstructText.color = Color.green;
			reconstructText.text = "Reconstruct";
		} else {
			reconstructImage.color = reconstructText.color = Color.red;
			reconstructText.text = "A part is in an invalid position!";
		}
	}
	void UpdateChainHelper(ShipBuilderPart part) {
		var x = GetRect(part.rectTransform);
		foreach(ShipBuilderPart shipPart in cursorScript.parts) {
			if(!shipPart.isInChain) {
				var y = GetRect(shipPart.rectTransform);
				if(x.Intersects(y)) {
					shipPart.isInChain = true;
					UpdateChainHelper(shipPart);
				}
			}
		}
	}

	public void UpdateChain() {
		SetReconstructButton(true);
		var shellRect = GetRect(shell.rectTransform);
		foreach(ShipBuilderPart shipPart in cursorScript.parts) {
			shipPart.isInChain = false;
			var partBounds = GetRect(shipPart.rectTransform);
			if(partBounds.Intersects(shellRect)) {
				bool z = Mathf.Abs(shipPart.rectTransform.anchoredPosition.x - shell.rectTransform.anchoredPosition.x) <
				0.18F*(shipPart.rectTransform.sizeDelta.x + shell.rectTransform.sizeDelta.x) &&
				Mathf.Abs(shipPart.rectTransform.anchoredPosition.y - shell.rectTransform.anchoredPosition.y) <
				0.18F*(shipPart.rectTransform.sizeDelta.y + shell.rectTransform.sizeDelta.y);
				shipPart.isInChain = !z;
			}
		}
		foreach(ShipBuilderPart shipPart in cursorScript.parts) {
			if(shipPart.isInChain) UpdateChainHelper(shipPart);
		}
		foreach(ShipBuilderPart shipPart in cursorScript.parts) { 
			// perform the same calculation again to falsify parts that are too close to the shell
			// TODO: make this less st*pid
			var partBounds = GetRect(shipPart.rectTransform);
			if(partBounds.Intersects(shellRect)) {
				bool z = Mathf.Abs(shipPart.rectTransform.anchoredPosition.x - shell.rectTransform.anchoredPosition.x) <
				0.18F*(shipPart.rectTransform.sizeDelta.x + shell.rectTransform.sizeDelta.x) &&
				Mathf.Abs(shipPart.rectTransform.anchoredPosition.y - shell.rectTransform.anchoredPosition.y) <
				0.18F*(shipPart.rectTransform.sizeDelta.y + shell.rectTransform.sizeDelta.y);
				shipPart.isInChain = !z;
			}
		}
		foreach(ShipBuilderPart shipPart in cursorScript.parts) {
			if(!shipPart.isInChain || !shipPart.validPos) {
				SetReconstructButton(false);
				return;
			}
		}
	}
	public void Initialize() {
		cursorScript.gameObject.SetActive(false);
		searcherString = "";
		contentsArray = new Transform[] {smallContents, mediumContents, largeContents};
		contentTexts = new GameObject[] {smallText, mediumText, largeText};
		foreach(GameObject obj in contentTexts) {
			obj.SetActive(false);
		}
		displayingTypes = new bool[] {true, true, true, true, true};
		player.SetIsInteracting(true);
		partDict = new Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript>();
		shell.sprite = ResourceManager.GetAsset<Sprite>(player.blueprint.coreShellSpriteID);
		shell.color = FactionColors.colors[0];
		shell.rectTransform.sizeDelta = shell.sprite.bounds.size * 100;
		core.sprite = ResourceManager.GetAsset<Sprite>(player.blueprint.coreSpriteID);
		core.material = ResourceManager.GetAsset<Material>("material_color_swap");
		core.color = FactionColors.colors[0];
		core.preserveAspect = true;
		core.rectTransform.sizeDelta = core.sprite.bounds.size * 110;
		List<EntityBlueprint.PartInfo> parts = player.GetInventory();

		if(parts != null) {
			if(parts.Count == 0) {
				EntityBlueprint.PartInfo info = new EntityBlueprint.PartInfo();
				foreach(string name in ResourceManager.allPartNames) {
					info.partID = name;
					parts.Add(info);
					info.partID = name;
					parts.Add(info);
					info.partID = name;
					parts.Add(info);
				}
			}
			for(int i = 0; i < parts.Count; i++) {
				parts[i] = CullSpatialValues(parts[i]);
			}
		}
		foreach(EntityBlueprint.PartInfo part in parts) {
			if(!partDict.ContainsKey(part)) 
			{
				int size = ResourceManager.GetAsset<PartBlueprint>(part.partID).size;
				partDict.Add(part, Instantiate(buttonPrefab, contentsArray[size]).GetComponent<ShipBuilderInventoryScript>());
				contentTexts[size].SetActive(true);
				partDict[part].part = part;
				partDict[part].cursor = cursorScript;
				partDict[part].IncrementCount();
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
		LoadBlueprint(player.blueprint);
		cursorScript.gameObject.SetActive(true);
	}

	public void CloseUI() {
		CloseUI(false);
	}

	public void CloseUI(bool validClose) {
		player.SetIsInteracting(false);
		gameObject.SetActive(false);
		if(validClose) {
			player.cursave.partInventory = new List<EntityBlueprint.PartInfo>();
			foreach(EntityBlueprint.PartInfo info in partDict.Keys) {
				if(partDict[info].GetCount() > 0) {
					for(int i = 0; i < partDict[info].GetCount(); i++)
						player.cursave.partInventory.Add(info);
				}
			}
		}
		foreach(ShipBuilderInventoryScript button in partDict.Values) {
			Destroy(button.gameObject);
		}
		foreach(ShipBuilderPart part in cursorScript.parts) {
			Destroy(part.gameObject);
		}
		cursorScript.parts = new List<ShipBuilderPart>();
	}
	public void LoadBlueprint(EntityBlueprint blueprint) {
		foreach(EntityBlueprint.PartInfo part in blueprint.parts) {
			var p = Instantiate(SBPrefab, cursorScript.transform.parent).GetComponent<ShipBuilderPart>();
			p.cursorScript = cursorScript;
			cursorScript.parts.Add(p);
			p.info = part;
			p.SetLastValidPos(part.location);
		}
	}

	public void Deinitialize() {
		bool invalidState = false;
		foreach(ShipBuilderPart part in cursorScript.parts) {
			if(!part.validPos || !part.isInChain) {
				invalidState = true;
				break;
			}
		}
		if(!invalidState) {
			Export();
			CloseUI(true);
		}
	}

	public void Export() {
		player.blueprint.parts = new List<EntityBlueprint.PartInfo>();
		foreach(ShipBuilderPart part in cursorScript.parts) {
			player.blueprint.parts.Add(part.info);
		}
		player.Rebuild();
	}

	void Start() {
		foreach(PresetButton button in GetComponentsInChildren<PresetButton>()) {
			button.SBPrefab = SBPrefab;
			button.player = player;
			button.cursorScript = cursorScript;
			button.builder = this;
			button.Initialize();
		}
	}
	void Update() {
		if((player.transform.position - yardPosition).sqrMagnitude > 200)
			CloseUI(false);
	}

	public EntityBlueprint.PartInfo? GetButtonPartCursorIsOn() {
		foreach(ShipBuilderInventoryScript inv in partDict.Values) {
			if(RectTransformUtility.RectangleContainsScreenPoint(inv.GetComponent<RectTransform>(), Input.mousePosition) && inv.gameObject.activeSelf) {
				return inv.part;
			}
		}
		return null;
	}
	public void ChangeDisplayFactors() {
		foreach(GameObject obj in contentTexts) {
			obj.SetActive(false);
		}
		foreach(ShipBuilderInventoryScript inv in partDict.Values) {
			string partName = inv.part.partID.ToLower();
			string abilityName = AbilityUtilities.GetAbilityNameByID(inv.part.abilityID).ToLower();
			if(partName.Contains(searcherString) || abilityName.Contains(searcherString) || searcherString == "") {
				if(displayingTypes[(int)AbilityUtilities.GetAbilityTypeByID(inv.part.abilityID)]) {
					inv.gameObject.SetActive(true);
					contentTexts[ResourceManager.GetAsset<PartBlueprint>(inv.part.partID).size].SetActive(true);
				}
				else inv.gameObject.SetActive(false);
			} else inv.gameObject.SetActive(false);
		}
	}

	public string GetCurrentJSON() {
		EntityBlueprint blueprint = player.blueprint;
		blueprint.parts = new List<EntityBlueprint.PartInfo>();
		foreach(ShipBuilderPart part in cursorScript.parts) {
			blueprint.parts.Add(part.info);
		}
		return JsonUtility.ToJson(blueprint);
	}
	
	public void SetSearcherString(string searcher) {
		searcherString = searcher.ToLower();
		ChangeDisplayFactors();
	}
	public void UpdateDisplayingCategories(int type) {
		displayingTypes[type] = !displayingTypes[type];
		ChangeDisplayFactors();
	}
}
