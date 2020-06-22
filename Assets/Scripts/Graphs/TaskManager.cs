using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NodeEditorFramework.Standard;
using NodeEditorFramework.IO;
using NodeEditorFramework;
using System;

public interface IDialogueOverrideHandler
{
    List<string> GetSpeakerIDList();
    Dictionary<string, Stack<UnityAction>> GetInteractionOverrides();
    void SetNode(ConnectionPort node);
    void SetNode(Node node);
    void SetSpeakerID(string ID); 
}

public class TaskManager : MonoBehaviour, IDialogueOverrideHandler
{
    public static TaskManager Instance = null;
    public static Dictionary<string, Stack<UnityAction>> interactionOverrides = new Dictionary<string, Stack<UnityAction>>();

    public List<string> questCanvasPaths;
    public SaveHandler saveHandler;

    bool initialized = false;
    public List<MissionTraverser> traversers;
    List<Task> activeTasks = new List<Task>();
    public Dictionary<string, int> taskVariables = new Dictionary<string, int>();
    public static bool autoSaveEnabled;

    // objective locations for visualization of tasks in the main map and minimap
    public class ObjectiveLocation 
    {
        public Vector2 location;
        public bool exactLocation;
        public Entity followEntity;
        public ObjectiveLocation(Vector2 location, bool exactLocation, Entity followEntity = null)
        {
            this.location = location;
            this.exactLocation = exactLocation;
            this.followEntity = followEntity;
        }
    }

    public static List<ObjectiveLocation> objectiveLocations = new List<ObjectiveLocation>();

    // Move to Dialogue System?
    public static string speakerID = null;
    public static List<string> speakerIDList = new List<string>();
    public static Entity GetSpeaker() {
        var speakerObj = SectorManager.instance.GetEntity(speakerID);
        return speakerObj;
    }

    public void Initialize()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        objectiveLocations = new List<ObjectiveLocation>();
        speakerID = null;
        initCanvases();
        questCanvasPaths = new List<string>();
        autoSaveEnabled = PlayerPrefs.GetString("TaskManager_autoSaveEnabled", "True") == "True";
        interactionOverrides = new Dictionary<string, Stack<UnityAction>>();
    }

    void Update()
    {
        foreach(var loc in objectiveLocations)
        {
            if(loc.followEntity)
            {
                loc.location = loc.followEntity.transform.position;
            }
        }
    }

    public static bool TraversersContainCheckpoint(string checkpointName)
    {
        foreach(var traverser in Instance.traversers)
        {
            if(traverser.lastCheckpointName == checkpointName) return true;
        }
        return false;
    }

    // TODO: add ability to set multiple paths
    public void SetCanvasPath(string path)
    {
        Debug.Log("Found Path");
        questCanvasPaths.Add(path);
    }

    public static void StartQuests() {
        Instance.startQuests();
    }
    public void AddTask(Task t)
    {
        activeTasks.Add(t);
        updateTaskList();
    }

    public void ActivateTask(string ID) // TODO: select canvas
    {
        for (int i = 0; i < traversers.Count; i++)
        {
            traversers[i].ActivateTask(ID);
        }
    }

    public Task[] getTasks()
    {
        return activeTasks.ToArray();
    }

    public void endTask(string taskID)
    {
        for(int i = 0; i < activeTasks.Count; i++)
        {
            if(activeTasks[i].taskID == taskID)
            {
                activeTasks.RemoveAt(i);
                updateTaskList();
                Debug.Log("Task removed.");
                break;
            }
        }
    }

    public void updateTaskList()
    {
        StatusMenu.taskInfo = getTasks();
    }

    public void SetTaskVariable(string name, int value)
    {
        taskVariables[name] = value;
        if (VariableConditionNode.OnVariableUpdate != null)
            VariableConditionNode.OnVariableUpdate.Invoke(name);
    }

    public int GetTaskVariable(string name)
    {
        if(taskVariables.ContainsKey(name))
        {
            return taskVariables[name];
        }
        Debug.LogWarningFormat("Tried to read unknown task variable '{0}'", name);
        return 0;
    }

    void initCanvases()
    {
        if (initialized)
            return;
        traversers = new List<MissionTraverser>();
        NodeCanvasManager.FetchCanvasTypes();
        NodeTypes.FetchNodeTypes();
        ConnectionPortManager.FetchNodeConnectionDeclarations();

        var XMLImport = new XMLImportExport();

        for (int i = 0; i < questCanvasPaths.Count; i++)
        {
            string finalPath = System.IO.Path.Combine(Application.streamingAssetsPath, questCanvasPaths[i]);
            Debug.Log("Canvas path [" + i + "] = " + finalPath);
            var canvas = XMLImport.Import(finalPath) as QuestCanvas;
            Debug.Log(canvas);
            if (canvas != null)
            {
                traversers.Add(new MissionTraverser(canvas));
            }
        }

        // reset all static condition variables
        SectorLimiterNode.LimitedSector = "";
        DestroyEntityCondition.OnUnitDestroyed = null;
        UsePartCondition.OnPlayerReconstruct = new UnityEvent();
        WinBattleCondition.OnBattleWin = null;

        initialized = true;
    }

    // Traverse quest graph
    public void startQuests()
    {
        for(int i = 0; i < traversers.Count; i++)
        {
            traversers[i].findRoot().TryAddMission();
        }

        // tasks
        var missions = PlayerCore.Instance.cursave.missions;
        foreach(var mission in missions)
        {
            if(traversers.Exists((t) => t.nodeCanvas.missionName == mission.name))
            {
                var traverser = traversers.Find((t) => t.nodeCanvas.missionName == mission.name);
                if(traverser.findRoot().overrideCheckpoint)
                {
                    traverser.activateCheckpoint(traverser.findRoot().overrideCheckpointName);
                }
                else
                {
                    traverser.activateCheckpoint(mission.checkpoint);
                }  
            }   
        }

        for (int i = 0; i < traversers.Count; i++)
        {
            traversers[i].StartQuest();
        }
    }

    public void setNode(ConnectionPort connection)
    {
        // Get canvas
        if (connection.connected())
        {
            setNode(connection.connections[0].body);
        }
    }

    public void setNode(NodeCanvas canvas, string ID)
    {
        for (int i = 0; i < canvas.nodes.Count; i++)
        {
            if(canvas.nodes[i].GetID() == ID)
            {
                setNode(canvas.nodes[i]);
            }
        }
    }

    public void setNode(Node node)
    {
        NodeCanvas canvas = node.Canvas;
        // Debug.Log("Node: " + node.name + " Canvas: " + node.Canvas);
        if(node.Canvas is QuestCanvas)
            (canvas.Traversal as MissionTraverser).SetNode(node);
        else
            (canvas.Traversal as DialogueTraverser).SetNode(node);
    }

    public static void DrawObjectiveLocations()
    {
        MapMakerScript.DrawObjectiveLocations();
        MinimapArrowScript.DrawObjectiveLocations();
    }

    public void LoadCheckpoint(string name)
    {
        Debug.Log("name");
        for (int i = 0; i < traversers.Count; i++)
        {
            traversers[i].nodeCanvas.nodes.Find((x) => { return (x is CheckpointNode && (x as CheckpointNode).checkpointName == name); });
        }
    }

    public void AttemptAutoSave()
    {
        if(autoSaveEnabled) saveHandler.Save();
    }

    public void RemoveTraverser(MissionTraverser traverser)
    {
        traversers.Remove(traverser);
    }

    public List<string> GetSpeakerIDList()
    {
        return speakerIDList;
    }

    public Dictionary<string, Stack<UnityAction>> GetInteractionOverrides()
    {
        return interactionOverrides;
    }

    public void SetNode(ConnectionPort node)
    {
        setNode(node);
    }

    public void SetSpeakerID(string ID)
    {
        speakerID = ID;
    }

    public void SetNode(Node node)
    {
        setNode(node);
    }
}
