using System.Collections;
using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEngine.Events;
using UnityEngine;

//TODO: move elsewhere
public abstract class TaskCondition
{
    public NodeEditorFramework.Standard.StartTaskNode Node;

    public UnityEvent OnTrigger;

    public abstract void Init(NodeEditorFramework.Standard.StartTaskNode node); // Set listeners & check if the condition is already met
    public abstract void DeInit(); // Unsubscribe from listeners
    public string sectorName;
}

public class EntityUnityEvent : UnityEvent<Entity>
{
    public Entity entityParameter;
}

public class Task : ScriptableObject
{
    public string taskID;
    public string description;
    public float creditReward;
    public EntityBlueprint.PartInfo partReward;
    public string taskGiverID;
    public string rewardGiverID;

    List<TaskCondition> successConditions = new List<TaskCondition>();
    List<TaskCondition> failConditions = new List<TaskCondition>();
}