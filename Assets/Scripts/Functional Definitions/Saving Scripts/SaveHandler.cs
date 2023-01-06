﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveHandler : MonoBehaviour
{
    public PlayerCore player;
    public TaskManager taskManager;
    PlayerSave save;
    public static SaveHandler instance;

    public PlayerSave GetSave()
    {
        return save;
    }
    public void Initialize()
    {
        instance = this;
        string currentPath;
        var CurrentSavePath = System.IO.Path.Combine(Application.persistentDataPath, "CurrentSavePath");
        if (!File.Exists(CurrentSavePath))
        {
            currentPath = System.IO.Path.Combine(Application.persistentDataPath, "TestSave");
        }
        else
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

            if (save.factions != null)
                for (int i = 0; i < save.factions.Length; i++)
                {
                    FactionManager.SetFactionRelations(save.factions[i], save.relations[i]);
                }

            SectorManager.instance.LoadSectorFile(save.resourcePath);
            save.missions.RemoveAll(m => !taskManager.questCanvasPaths.Exists(p =>
                System.IO.Path.GetFileNameWithoutExtension(p) == m.name));
            taskManager.Initialize(true); // Re-init
            DialogueSystem.InitCanvases();


            player.blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
            player.blueprint.name = "Player Save Blueprint";
            if (!string.IsNullOrEmpty(save.currentPlayerBlueprint))
            {
                var print = SectorManager.TryGettingEntityBlueprint(save.currentPlayerBlueprint);
                player.blueprint = print;
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
            StartCoroutine(Autobackup());
        }
        else
        {
            Debug.LogError("There was not a save or test save that was ready on load.");
            save = new PlayerSave();
            save.presetBlueprints = new string[5];
            save.currentHealths = new float[] { 1000, 250, 500 };
            save.partInventory = new List<EntityBlueprint.PartInfo>();

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

    private void UpdateSaveData(PlayerSave playerSave)
    {
        playerSave.timePlayed += (Time.timeSinceLevelLoad - timeSinceLastSave) / 60;
        if (SaveMenuHandler.migratedTimePlayed != null)
        {
            playerSave.timePlayed += (int)SaveMenuHandler.migratedTimePlayed;
        }

        playerSave.position = player.spawnPoint;
        playerSave.lastDimension = player.LastDimension;
        playerSave.currentHealths = player.CurrentHealth;
        if (player.CurrentHealth[1] <= 0)
        {
            playerSave.currentHealths = player.GetMaxHealth();
        }

        playerSave.currentPlayerBlueprint = JsonUtility.ToJson(player.blueprint);
        playerSave.credits = player.GetCredits();
        playerSave.abilityCaps = player.abilityCaps;
        playerSave.shards = player.shards;
        if (playerSave.resourcePath == "" || playerSave.resourcePath.Contains("main"))
        {
            playerSave.resourcePath = SectorManager.instance.resourcePath;
        }

        playerSave.characters = SectorManager.instance.characters;
        playerSave.version = VersionNumberScript.version;

        List<int> factions = new List<int>();
        List<int> relations = new List<int>();
        for (int i = 0; i < FactionManager.FactionArrayLength; i++)
        {
            if (FactionManager.FactionExists(i))
            {
                factions.Add(i);
                relations.Add(FactionManager.GetFactionRelations(i));
            }
        }
        playerSave.factions = factions.ToArray();
        playerSave.relations = relations.ToArray();


        for (int i = 0; i < taskManager.traversers.Count; i++)
        {
            var traverser = taskManager.traversers[i];
            var missionName = traverser.nodeCanvas.missionName;
            var lastCheckpoint = traverser.lastCheckpointName;
            playerSave.missions.Find((m) => m.name == traverser.nodeCanvas.missionName).checkpoint = lastCheckpoint;
        }

        // Calculate the save episode by finding the maximal active mission's epsiode.
        playerSave.episode = 0;
        foreach (var mission in playerSave.missions)
        {
            if (mission.status != Mission.MissionStatus.Inactive)
            {
                if (playerSave.episode < mission.episode)
                {
                    playerSave.episode = mission.episode;
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

        playerSave.taskVariableNames = keys;
        playerSave.taskVariableValues = values;
        playerSave.reputation = player.reputation;
    }

    private float timeSinceLastSave;

    public void Save()
    {
        UpdateSaveData(save);
        timeSinceLastSave = Time.timeSinceLevelLoad;
        SaveMenuHandler.migratedTimePlayed = null;

        string currentPath = File.ReadAllLines(System.IO.Path.Combine(Application.persistentDataPath, "CurrentSavePath"))[0];
        string saveJson = JsonUtility.ToJson(save);
        File.WriteAllText(currentPath, saveJson);
    }

    public void BackupSave()
    {
        string currentPath = File.ReadAllLines(System.IO.Path.Combine(Application.persistentDataPath, "CurrentSavePath"))[0];
        string backupPath = currentPath + " - Backup";

        PlayerSave saveCopy = JsonUtility.FromJson<PlayerSave>(JsonUtility.ToJson(save));
        UpdateSaveData(saveCopy);

        saveCopy.name += " - Backup";

        string saveJson = JsonUtility.ToJson(saveCopy);
        File.WriteAllText(backupPath, saveJson);
    }

    IEnumerator Autobackup()
    {
        while (true)
        {
            yield return new WaitForSeconds(20 * 60);
            BackupSave();
        }
    }
}
