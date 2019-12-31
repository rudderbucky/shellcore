using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSave {
	
	public string name;
    public string resourcePath;
	public Vector2 position;
	public float[] currentHealths;
	public string currentPlayerBlueprint;
	public string version;
	public List<EntityBlueprint.PartInfo> partInventory;
	public string[] presetBlueprints;
	public float timePlayed;
	public int credits;
    public string lastTaskNodeID;
    public string[] taskVariableNames;
    public int[] taskVariableValues;
    public string[] activeTaskIDs;
	public int[] abilityCaps;
	public int shards;
	public WorldData.CharacterData[] characters;
	public List<string> sectorsSeen;
	public float soundVolume;
}
