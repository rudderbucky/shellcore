using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// !!! WARNING: EXTREME MEGA SPACE CANCER AHEAD !!!
// Since spawns do not use ability IDs, HUNDREDS of lines of code need to be special cased for drone blueprint strings! (See AbilityHandler.cs)
// Life sucks sometimes. If you're still reading this try listening to this https://www.youtube.com/watch?v=AlIdlrlv6Ak
[System.Serializable]
public struct AbilityHotkeyStruct
{
	public List<AbilityID> skills;
	public List<string> spawns;
	public List<AbilityID> weapons;
	public List<AbilityID> passive;

	public IList GetList(int index)
	{
		switch(index)
		{
			case 0:
				return skills;
			case 1:
				return spawns;
			case 2:
				return weapons;
			case 3:
				return passive;
		}
		return null;
	}
}

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
    public string[] taskVariableNames;
    public int[] taskVariableValues;
	public int[] abilityCaps;
	public int shards;
	public WorldData.CharacterData[] characters;
	public List<string> sectorsSeen;
	public List<EntityBlueprint.PartInfo> partsSeen;
	public List<EntityBlueprint.PartInfo> partsObtained;
	public List<Mission> missions;
	public int reputation;

	// Episode count is slightly non-trivial.
	// Episode 1 is 0, episode 2 is 1, and so on.
	// This allows EP1 missions to not have to be rewritten,
	// As well as for side missions to not change the episode number.
	public int episode;

	// Contains IDs of unlocked party members (there will be a node that unlocks members adding IDs into this list)
	public List<string> unlockedPartyIDs;
	public AbilityHotkeyStruct abilityHotkeys;
}
