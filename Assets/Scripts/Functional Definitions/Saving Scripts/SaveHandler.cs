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
			if(save.presetBlueprints.Length != 5) {
				save.presetBlueprints = new string[5];
			}

            player.Rebuild();
            Camera.main.GetComponent<CameraScript>().Initialize(player);
            GameObject.Find("AbilityUI").GetComponent<AbilityHandler>().Initialize(player);

            //SectorManager.instance.LoadSectorFile(save.resourcePath);

            // tasks
            taskManager.setNode(save.lastTaskNodeID);
            for (int i = 0; i < save.activeTaskIDs.Length; i++)
            {
                taskManager.ActivateTask(save.activeTaskIDs[i]);
            }

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

        // tasks
        var limiterNode = NodeEditorFramework.Standard.SectorLimiterNode.StartPoint;
        if (limiterNode != null)
            Debug.Log("limiter found!");

        save.lastTaskNodeID = limiterNode == null ? taskManager.lastTaskNodeID : limiterNode.GetID();

        Dictionary<string, int> variables = limiterNode == null
            ? taskManager.taskVariables
            : limiterNode.GetVariables();
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

        var tasks = taskManager.getTasks();
        string[] taskIDs = new string[tasks.Length];
        for (int i = 0; i < tasks.Length; i++)
        {
            taskIDs[i] = tasks[i].taskID;
        }
        save.activeTaskIDs = taskIDs;

		string saveJson = JsonUtility.ToJson(save);
		File.WriteAllText(currentPath, saveJson);
	}
}
