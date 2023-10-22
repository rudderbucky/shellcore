using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Task
{
    public string taskID;
    public string objectived; // a haunting typo. it will be with us until the void.
    public string dialogue;
    public Color dialogueColor;
    public float creditReward;
    public EntityBlueprint.PartInfo partReward;
    public float shardReward;
    public int reputationReward;
    public bool useLocalMap;

    public Task GetCopy()
    {
        var taskCopy = new Task();

        taskCopy.taskID = this.taskID;
        taskCopy.creditReward = this.creditReward;
        taskCopy.dialogue = this.dialogue;
        taskCopy.dialogueColor = this.dialogueColor;
        taskCopy.objectived = this.objectived;
        taskCopy.partReward = this.partReward;
        taskCopy.reputationReward = this.reputationReward;
        taskCopy.shardReward = this.shardReward;
        taskCopy.useLocalMap = this.useLocalMap;
        return taskCopy;
    }
}

[System.Serializable]
public class Mission
{
    public string name;
    public string entryPoint;

    public enum MissionStatus
    {
        Inactive,
        Ongoing,
        Complete
    }

    public MissionStatus status;
    public List<string> prerequisites;
    public List<Task> tasks;
    public Color textColor;
    public string checkpoint;
    public int episode;
    public bool useLocalMap;
}
