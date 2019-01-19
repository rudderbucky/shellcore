using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBuilder : MonoBehaviour {

	public List<EntityBlueprint.PartInfo> parts = new List<EntityBlueprint.PartInfo>();
	public PlayerCore player;

	public void Export() {
		player.blueprint.parts = parts;
		player.Rebuild();
	}
}
