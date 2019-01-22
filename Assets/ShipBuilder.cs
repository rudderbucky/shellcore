using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBuilder : MonoBehaviour {
	public GameObject SBPrefab;
	public UnityEngine.UI.Image shell;
	public UnityEngine.UI.Image core;
	public ShipBuilderCursorScript cursorScript;
	public GameObject buttonPrefab;
	public PlayerCore player;
	public Transform viewportContents;
	Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript> partDict;
	void Start() {
		Initialize();
	}
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
		//builder.Dispatch(part.info);
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
		var p1 = new EntityBlueprint.PartInfo();
		p1.partID = "SmallCenter1";
		p1.abilityType = Ability.AbilityType.Beam;
		parts.Add(p1);
		p1.partID = "MediumCenter3";
		parts.Add(p1);
		parts.Add(p1);
		parts.Add(p1);
		if(parts != null) {
			for(int i = 0; i < parts.Count; i++) {
				parts[i] = CullSpatialValues(parts[i]);
			}
		}
		partDict = new Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript>();
		foreach(EntityBlueprint.PartInfo part in parts) {
			if(!partDict.ContainsKey(part)) 
			{
				partDict.Add(part, Instantiate(buttonPrefab, viewportContents).GetComponent<ShipBuilderInventoryScript>());
				partDict[part].part = part;
				partDict[part].cursor = cursorScript;
				partDict[part].IncrementCount();
			} else partDict[part].IncrementCount();
		}

		LoadBlueprint(player.blueprint);
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
		foreach(ShipBuilderInventoryScript button in partDict.Values) {
			Destroy(button.gameObject);
			player.cursave.partInventory = new List<EntityBlueprint.PartInfo>();
			foreach(EntityBlueprint.PartInfo info in partDict.Keys) {
				player.cursave.partInventory.Add(info);
			}
		}
		Export();
	}

	public void Export() {
		player.blueprint.parts = new List<EntityBlueprint.PartInfo>();
		foreach(ShipBuilderPart part in cursorScript.parts) {
			player.blueprint.parts.Add(part.info);
		}
		player.Rebuild();
	}
}
