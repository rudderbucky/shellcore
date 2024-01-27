using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CoreScriptsManager;
using static CoreScriptsSequence;

public class TaskFlow : MonoBehaviour
{

    public static void FinishTask(Context context, bool playSound)
    {
        if (playSound) SectorManager.instance.player.alerter.showMessage("TASK COMPLETE", "clip_victory");
        RewardPlayer(context.missionName);
        context.taskHash++;
    }

    public static void RewardPlayer(string missionName)
    {
        var mission = PlayerCore.Instance.cursave.missions.Find((x) => x.name == missionName);
        if (mission.tasks.Count == 0) return;
        var latestTask = mission.tasks[mission.tasks.Count - 1];
        string taskID = latestTask.taskID;
        TaskManager.Instance.endTask(taskID);
        Debug.Log("Task complete!");
        SectorManager.instance.player.AddCredits((int)latestTask.creditReward);
        SectorManager.instance.player.reputation += latestTask.reputationReward;
        SectorManager.instance.player.cursave.shards += (int)latestTask.shardReward;
        if (!string.IsNullOrEmpty(latestTask.partReward.partID))
        {
            SectorManager.instance.player.cursave.partInventory.Add(
                new EntityBlueprint.PartInfo
                {
                    partID = latestTask.partReward.partID,
                    abilityID = latestTask.partReward.abilityID,
                    tier = latestTask.partReward.tier,
                    secondaryData = latestTask.partReward.secondaryData
                });
        }
    }
}
