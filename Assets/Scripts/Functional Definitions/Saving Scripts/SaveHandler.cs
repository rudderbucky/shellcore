﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveHandler : MonoBehaviour
{
    public PlayerCore player;
    public TaskManager taskManager;
    PlayerSave save;
    private static string CurrentSavePath;

    public void Initialize()
    {
        string currentPath = Application.persistentDataPath + "\\TestSave";
        CurrentSavePath = Application.persistentDataPath + "\\CurrentSavePath";
        if (File.Exists(CurrentSavePath))
        {
            currentPath = File.ReadAllLines(CurrentSavePath)[0];
        }

        if (File.Exists(currentPath))
        {
            // Load
            string json = File.ReadAllText(currentPath);
            save = JsonUtility.FromJson<PlayerSave>(json);
            if (save.timePlayed != 0)
            {
                player.spawnPoint = save.position;
                player.Dimension = save.lastDimension;
            }

            if (SectorManager.testJsonPath != null)
            {
                save.resourcePath = SectorManager.testJsonPath;
            }
            else if (save.resourcePath == "")
            {
                save.resourcePath = SectorManager.jsonPath;
            }

            player.cursave = save;

            SectorManager.instance.LoadSectorFile(save.resourcePath);
            taskManager.Initialize(true); // Re-init
            DialogueSystem.InitCanvases();

            player.blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
            player.blueprint.name = "Player Save Blueprint";
            if (!string.IsNullOrEmpty(save.currentPlayerBlueprint))
            {
                JsonUtility.FromJsonOverwrite(save.currentPlayerBlueprint, player.blueprint);
            }
            else
            {
                Debug.LogError("Save should have been given a currentPlayerBlueprint by now.");
                player.blueprint.parts = new List<EntityBlueprint.PartInfo>();
                player.blueprint.coreSpriteID = "core1_light";
                player.blueprint.coreShellSpriteID = "core1_shell";
                player.blueprint.baseRegen = CoreUpgraderScript.GetRegens(player.blueprint.coreShellSpriteID);
                player.blueprint.shellHealth = CoreUpgraderScript.defaultHealths;
            }

            player.abilityCaps = save.abilityCaps;
            player.shards = save.shards;
            player.SetCredits(save.credits);
            player.reputation = save.reputation;
            if (save.presetBlueprints.Length != 5)
            {
                save.presetBlueprints = new string[5];
            }

            Camera.main.GetComponent<CameraScript>().Initialize(player);

            taskManager.taskVariables.Clear();
            for (int i = 0; i < save.taskVariableNames.Length; i++)
            {
                taskManager.taskVariables.Add(save.taskVariableNames[i], save.taskVariableValues[i]);
            }
        }
        else
        {
            Debug.LogError("There was not a save or test save that was ready on load.");
            save = new PlayerSave
            {
                presetBlueprints = new string[5],
                currentHealths = new float[] {1000, 250, 500},
                partInventory = new List<EntityBlueprint.PartInfo>()
            };

            player.blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
            player.blueprint.name = "Player Save Blueprint";
            player.blueprint.coreSpriteID = "core1_light";
            player.blueprint.coreShellSpriteID = "core1_shell";
            player.blueprint.baseRegen = CoreUpgraderScript.GetRegens(player.blueprint.coreShellSpriteID);
            player.blueprint.shellHealth = CoreUpgraderScript.defaultHealths;
            player.blueprint.parts = new List<EntityBlueprint.PartInfo>();
            player.cursave = save;
            player.abilityCaps = CoreUpgraderScript.minAbilityCap;
        }
    }

    public void Save()
    {
        save.timePlayed += Time.timeSinceLevelLoad / 60;
        if (SaveMenuHandler.migratedTimePlayed != null)
        {
            save.timePlayed += (int)SaveMenuHandler.migratedTimePlayed;
            SaveMenuHandler.migratedTimePlayed = null;
        }

        save.position = player.spawnPoint;
        save.lastDimension = player.LastDimension;
        save.currentHealths = player.CurrentHealth;
        if (player.CurrentHealth[1] <= 0)
        {
            save.currentHealths = player.GetMaxHealth();
        }

        save.currentPlayerBlueprint = JsonUtility.ToJson(player.blueprint);
        save.credits = player.GetCredits();
        save.abilityCaps = player.abilityCaps;
        save.shards = player.shards;
        if (save.resourcePath == "" || save.resourcePath.Contains("main"))
        {
            save.resourcePath = SectorManager.instance.resourcePath;
        }

        save.characters = SectorManager.instance.characters;
        save.version = VersionNumberScript.version;


        for (int i = 0; i < taskManager.traversers.Count; i++)
        {
            var traverser = taskManager.traversers[i];
            var missionName = traverser.nodeCanvas.missionName;
            var lastCheckpoint = traverser.lastCheckpointName;
            save.missions.Find((m) => m.name == traverser.nodeCanvas.missionName).checkpoint = lastCheckpoint;
        }

        // Calculate the save episode by finding the maximal active mission's epsiode.
        save.episode = 0;
        foreach (var mission in save.missions)
        {
            if (mission.status != Mission.MissionStatus.Inactive)
            {
                if (save.episode < mission.episode)
                {
                    save.episode = mission.episode;
                }
            }
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

        string currentPath = File.ReadAllLines(CurrentSavePath)[0];
        string saveJson = JsonUtility.ToJson(save);
        File.WriteAllText(currentPath, saveJson);
    }
}
