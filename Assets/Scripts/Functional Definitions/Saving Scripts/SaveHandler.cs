using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
public class SaveHandler : MonoBehaviour {

	public PlayerCore player;
    public TaskManager taskManager;
	PlayerSave save;

	public void Initialize() {
		string currentPath;
		if(!File.Exists(Application.persistentDataPath + "\\CurrentSavePath")) {
			currentPath = Application.persistentDataPath + "\\TestSave";
		}
		else currentPath = File.ReadAllLines(Application.persistentDataPath + "\\CurrentSavePath")[0];
		if(File.Exists(currentPath)) {
            // Load
			string json = File.ReadAllText(currentPath);
			save = JsonUtility.FromJson<PlayerSave>(json);
			player.blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
			player.blueprint.name = "Player Save Blueprint";
			if(save.currentPlayerBlueprint != null && save.currentPlayerBlueprint != "") {
				JsonUtility.FromJsonOverwrite(save.currentPlayerBlueprint, player.blueprint);
			} else {
				player.blueprint.baseRegen = new float[] {60,0,60};
				player.blueprint.shellHealth = new float[] {1000,250,500};
				player.blueprint.coreSpriteID = "core1_light";
				player.blueprint.coreShellSpriteID = "core1_shell";
			}
			player.abilityCaps = save.abilityCaps;
			player.shards = save.shards;
			player.cursave = save;
			player.credits = save.credits;
			player.reputation = save.reputation;
			if(save.presetBlueprints.Length != 5) {
				save.presetBlueprints = new string[5];
			}

            if(save.timePlayed != 0) player.spawnPoint = save.position;

            player.Rebuild();
            Camera.main.GetComponent<CameraScript>().Initialize(player);
            GameObject.Find("AbilityUI").GetComponent<AbilityHandler>().Initialize(player);


            //SectorManager.instance.LoadSectorFile(save.resourcePath);

            for (int i = 0; i < save.taskVariableNames.Length; i++)
            {
                taskManager.taskVariables.Add(save.taskVariableNames[i], save.taskVariableValues[i]);
            }
		} else {
			save = new PlayerSave();
			save.presetBlueprints = new string[5];
			save.currentHealths = new float[] {1000,250,500};
			save.partInventory = new List<EntityBlueprint.PartInfo>();

			player.blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
			player.blueprint.name = "Player Save Blueprint";
			player.blueprint.baseRegen = new float[] {60,0,60};
			player.blueprint.shellHealth = new float[] {1000,250,500};
			player.blueprint.parts = new List<EntityBlueprint.PartInfo>();
			player.blueprint.coreSpriteID = "core1_light";
			player.blueprint.coreShellSpriteID = "core1_shell";
			player.cursave = save;
			player.abilityCaps = new int[] {10, 10, 10, 10};
		}
	}
	
	public void Save() {
		save.timePlayed += Time.timeSinceLevelLoad / 60;
		if(SaveMenuHandler.migratedTimePlayed != null)
		{
			save.timePlayed += (int)SaveMenuHandler.migratedTimePlayed;
			SaveMenuHandler.migratedTimePlayed = null;
		} 
		string currentPath = File.ReadAllLines(Application.persistentDataPath + "\\CurrentSavePath")[0];
		save.position = player.spawnPoint;
		save.currentHealths = player.currentHealth;
		if(player.currentHealth[1] <= 0) save.currentHealths = player.GetMaxHealth();
		save.currentPlayerBlueprint = JsonUtility.ToJson(player.blueprint);
		save.credits = player.credits;
        save.abilityCaps = player.abilityCaps;
        save.shards = player.shards;
        save.resourcePath = SectorManager.instance.resourcePath;
		save.characters = SectorManager.instance.characters;
		save.version = VersionNumberScript.version;
		
		
        for (int i = 0; i < taskManager.traversers.Count; i++)
        {
			var traverser = taskManager.traversers[i];
			var missionName = traverser.nodeCanvas.missionName;
			var lastCheckpoint = traverser.lastCheckpointName;
            save.missions.Find((m) => m.name == traverser.nodeCanvas.missionName).checkpoint = lastCheckpoint;
        }

        Dictionary<string, int> variables = taskManager.taskVariables;
        string[] keys = new string[taskManager.taskVariables.Count];
        int[] values = new int[taskManager.taskVariables.Count];
        int index = 0;
        foreach (var pair in taskManager.taskVariables)
        {
            keys[index] = pair.Key;
            values[index] = pair.Value;
            index++;
        }
        save.taskVariableNames = keys;
        save.taskVariableValues = values;
		save.reputation = player.reputation;

		string saveJson = JsonUtility.ToJson(save);
		File.WriteAllText(currentPath, saveJson);
	}
}
