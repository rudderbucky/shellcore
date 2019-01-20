using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBuilder : MonoBehaviour {

	public List<EntityBlueprint.PartInfo> parts = new List<EntityBlueprint.PartInfo>();
	public PlayerCore player;
	public GameObject button;
	public Transform contents;
	public ShipBuilderCursorScript cursor;
	Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript> partDict;

	public void Export() {
		player.blueprint.parts = parts;
		player.Rebuild();
	}

	void Start() {
		Initialize();
	}
	public void Initialize() {
		cursor.LoadBlueprint(player.blueprint);
		List<EntityBlueprint.PartInfo> parts = player.GetInventory();
		var p1 = new EntityBlueprint.PartInfo();
		p1.partID = "SmallCenter1";
		p1.abilityType = Ability.AbilityType.Beam;
		parts.Add(p1);
		p1.partID = "MediumCenter3";
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
				partDict.Add(part, Instantiate(button, contents).GetComponent<ShipBuilderInventoryScript>());
				partDict[part].part = part;
				partDict[part].cursor = cursor;
				partDict[part].IncrementCount();
			} else partDict[part].IncrementCount();
		}
	}

	public EntityBlueprint.PartInfo CullSpatialValues(EntityBlueprint.PartInfo x) {
		var part = new EntityBlueprint.PartInfo();
		part.partID = x.partID;
		part.spawnID = x.spawnID;
		part.abilityType = x.abilityType;
		return part;
	}
	public void Dispatch(EntityBlueprint.PartInfo part) {
		var x = CullSpatialValues(part);
		if(partDict.ContainsKey(x)) {
			partDict[x].IncrementCount();
		} else {
			partDict.Add(x, Instantiate(button, contents).GetComponent<ShipBuilderInventoryScript>());
				partDict[x].part = x;
				partDict[x].cursor = cursor;
				partDict[x].IncrementCount();
		}
	}
	public void Deinitialize() {
		foreach(ShipBuilderInventoryScript script in partDict.Values) {
			Destroy(script.gameObject);
		}
		player.cursave.partInventory = parts;
		Export();
	}
}
