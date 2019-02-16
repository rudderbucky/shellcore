using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSave {
	// TODO: save timePlayed functionality
	public string name;
	public Vector2 position;
	public float[] currentHealths;
	public string currentPlayerBlueprint;

	public List<EntityBlueprint.PartInfo> partInventory;
	public string[] presetBlueprints;
	public float timePlayed;
}
