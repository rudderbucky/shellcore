using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSave {
	
	public string name;
	public Vector2 position;
	public float[] currentHealths;
	public string currentPlayerBlueprint;
	public string version;
	public List<EntityBlueprint.PartInfo> partInventory;
	public string[] presetBlueprints;
	public float timePlayed;
	public int credits;
	public string shellID = "core3_shell";
}
