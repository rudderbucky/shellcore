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
    public List<SectorTraverser> sectorTraversers;
    List<Task> activeTasks = new List<Task>();
    public Dictionary<string, int> taskVariables = new Dictionary<string, int>();
    public static bool autoSaveEnabled;
    public static bool loading = false;

    // objective locations for visualization of tasks in the main map and minimap
    public class ObjectiveLocation 
    {
        public Vector2 location;
        public bool exactLocation;
        public Entity followEntity;
        public string missionName;
        public ObjectiveLocation(Vector2 location, bool exactLocation, string missionName, Entity followEntity = null)
        {
            this.missionName = missionName;
            this.location = location;
            this.exactLocation = exactLocation;
            this.followEntity = followEntity;
        }
    }

    public static Dictionary<string, List<ObjectiveLocation>> objectiveLocations = new Dictionary<string, List<ObjectiveLocation>>();
    // Move to Dialogue System?
    public static string speakerID = null;
    public static List<string> speakerIDList = new List<string>();
    public static Entity GetSpeaker() {
        var speakerObj = SectorManager.instance.GetEntity(speakerID);
        return speakerObj;
    }

    public void Initialize(bool forceReInit = false)
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
        objectiveLocations = new Dictionary<string, List<ObjectiveLocation>>();
        speakerID = null;
        interactionOverrides = new Dictionary<string, Stack<UnityAction>>();
        initCanvases(forceReInit);
        questCanvasPaths = new List<string>();
        autoSaveEnabled = PlayerPrefs.GetString("TaskManager_autoSaveEnabled", "True") == "True";
    }

    void Update()
    {
        foreach(var ls in objectiveLocations.Values)
        {
            foreach(var loc in ls)
            {
                if(loc.followEntity)
                {
                    loc.location = loc.followEntity.transform.position;
                }
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

    public void ClearCanvases(bool doNotClearCanvasPaths = false)
    {
        if (traversers != null)
            for (int i = 0; i < traversers.Count; i++)
            {
                traversers[i].nodeCanvas.Destroy();
            }
        if (sectorTraversers != null)
            for (int i = 0; i < sectorTraversers.Count; i++)
            {
                sectorTraversers[i].nodeCanvas.Destroy();
            }
        sectorTraversers = new List<SectorTraverser>();
        traversers = new List<MissionTraverser>();
        if(!doNotClearCanvasPaths) questCanvasPaths.Clear();
    }

    public void AddCanvasPath(string path)
    {
        questCanvasPaths.Add(path);
    }

    public static void StartQuests() {
        loading = true;
        Instance.startQuests();
        loading = false;
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

    public void SetTaskVariable(string name, int value, bool incrementMode)
    {
        if(incrementMode)
            taskVariables[name] += value;
        else taskVariables[name] = value;
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

    void initCanvases(bool forceReInit)
    {
        if (initialized && !forceReInit)
            return;
        traversers = new List<MissionTraverser>();
        sectorTraversers = new List<SectorTraverser>();
        NodeCanvasManager.FetchCanvasTypes();
        NodeTypes.FetchNodeTypes();
        ConnectionPortManager.FetchNodeConnectionDeclarations();

        if(Instance)
        {
            Instance.ClearCanvases(true);
        }
            

        var XMLImport = new XMLImportExport();

        for (int i = 0; i < questCanvasPaths.Count; i++)
        {
            string finalPath = System.IO.Path.Combine(Application.streamingAssetsPath, questCanvasPaths[i]);
            
            if (finalPath.Contains(".taskdata"))
            {
                var canvas = XMLImport.Import(finalPath) as QuestCanvas;

                if (canvas != null)
                {
                    traversers.Add(new MissionTraverser(canvas));
                }
            }
            else if (finalPath.Contains(".sectordata"))
            {
                var canvas = XMLImport.Import(finalPath) as SectorCanvas;

                if (canvas != null)
                {
                    sectorTraversers.Add(new SectorTraverser(canvas));
                }
            }
        }

        // reset all static condition variables
        SectorLimiterNode.LimitedSector = "";
        Entity.OnEntityDeath = null;
        UsePartCondition.OnPlayerReconstruct = new UnityEvent();
        WinBattleCondition.OnBattleWin = null;

        initialized = true;
    }

    // Traverse quest graph
    public void startQuests()
    {
        for(int i = 0; i < traversers.Count; i++)
        {
            var start = traversers[i].findRoot();
            if (start != null)
                start.TryAddMission();
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

                var tasks = mission.tasks.ToArray();
                if (mission.status != Mission.MissionStatus.Complete && mission.tasks.Count > 0)
                {
                    var task = mission.tasks[mission.tasks.Count - 1];
                    StartTaskNode start = traverser.nodeCanvas.nodes.Find((node) => { return node is StartTaskNode && (node as StartTaskNode).taskID == task.taskID; }) as StartTaskNode;
                    if (start != null)
                    {
                        traverser.ActivateTask(start.taskID);
                    }
                }
            }   
        }

        for (int i = 0; i < traversers.Count; i++)
        {
            traversers[i].StartQuest();
        }

        for (int i = 0; i < sectorTraversers.Count; i++)
        {
            var start = sectorTraversers[i].findRoot();
            sectorTraversers[i].startNode = (LoadSectorNode)start;
            sectorTraversers[i].StartQuest();
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

    /**
        Sets the passed node's canvas's current node to the passed node.
        The Task Manager isn't modified by this method. 
        All changes are at the canvas level.
    */
    public void setNode(Node node)
    {
        NodeCanvas canvas = node.Canvas;
        // Debug.Log("Node: " + node.name + " Canvas: " + node.Canvas);
        if(node.Canvas is QuestCanvas)
            (canvas.Traversal as MissionTraverser).SetNode(node);
        else if(node.Canvas is DialogueCanvas)
            (canvas.Traversal as DialogueTraverser).SetNode(node);
        else
            (canvas.Traversal as SectorTraverser).SetNode(node);
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
        if(autoSaveEnabled && !loading) saveHandler.Save();
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
