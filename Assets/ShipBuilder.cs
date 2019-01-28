using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipBuilder : MonoBehaviour {
	public GameObject SBPrefab;
	public Vector3 yardPosition;
	public Image shell;
	public Image core;
	public ShipBuilderCursorScript cursorScript;
	public GameObject buttonPrefab;
	public PlayerCore player;
	public Transform viewportContents;
	Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript> partDict;
	public static EntityBlueprint.PartInfo CullSpatialValues(EntityBlueprint.PartInfo x) {
		var part = new EntityBlueprint.PartInfo();
		part.partID = x.partID;
		part.spawnID = x.spawnID;
		part.abilityType = x.abilityType;
		return part;
	}
	public void DispatchPart(ShipBuilderPart part) {
		var culledInfo = CullSpatialValues(part.info);
		if(!partDict.ContainsKey(culledInfo)) {
			partDict.Add(culledInfo, Instantiate(buttonPrefab, viewportContents).GetComponent<ShipBuilderInventoryScript>());
			partDict[culledInfo].part = culledInfo;
			partDict[culledInfo].cursor = cursorScript;
		}
		partDict[culledInfo].IncrementCount();
		cursorScript.parts.Remove(part);
		Destroy(part.gameObject);
	}

	public bool IsInChain(ShipBuilderPart part) {
		var x = part.rectTransform.rect;
		x.center = part.rectTransform.anchoredPosition;
		var shellRect = shell.rectTransform.rect;
		if(x.Overlaps(shellRect)) return true;
		else {
			foreach(ShipBuilderPart shipPart in cursorScript.parts) {
				if(shipPart.isInChain && part != shipPart) {
					var y = shipPart.rectTransform.rect;
					y.center = shipPart.rectTransform.anchoredPosition;
					if(x.Overlaps(y) && shipPart.validPos) return true;
				}
			}
			return false;
		}
	}
	public void Initialize() {
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
			for(int i = 0; i < parts.Count; i++) {
				parts[i] = CullSpatialValues(parts[i]);
			}
		}
		foreach(EntityBlueprint.PartInfo part in parts) {
			if(!partDict.ContainsKey(part)) 
			{
				partDict.Add(part, Instantiate(buttonPrefab, viewportContents).GetComponent<ShipBuilderInventoryScript>());
				partDict[part].part = part;
				partDict[part].cursor = cursorScript;
				partDict[part].IncrementCount();
			} else partDict[part].IncrementCount();
		}
		if(player.GetTractorTarget() && player.GetTractorTarget().GetComponent<ShellPart>()) {
			var part = player.GetTractorTarget().GetComponent<ShellPart>().info;
			part = CullSpatialValues(part);
			if(!partDict.ContainsKey(part)) {
				var button = Instantiate(buttonPrefab, viewportContents).GetComponent<ShipBuilderInventoryScript>();
				button.cursor = cursorScript;
				button.part = part;
				button.IncrementCount();
				partDict.Add(part, button);
			} else partDict[part].IncrementCount();
			Destroy(player.GetTractorTarget().gameObject);
		}

		LoadBlueprint(player.blueprint);
	}

	public void CloseUI(bool validClose) {
		gameObject.SetActive(false);
		if(validClose) {
			player.cursave.partInventory = new List<EntityBlueprint.PartInfo>();
			foreach(EntityBlueprint.PartInfo info in partDict.Keys) {
				if(partDict[info].GetCount() > 0) {
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
		} else CloseUI(false);
	}

	public void Export() {
		player.blueprint.parts = new List<EntityBlueprint.PartInfo>();
		foreach(ShipBuilderPart part in cursorScript.parts) {
			player.blueprint.parts.Add(part.info);
		}
		player.Rebuild();
	}

	void Update() {
		if((player.transform.position - yardPosition).sqrMagnitude > 100)
			CloseUI(false);
	}
}
