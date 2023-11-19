using System.Collections.Generic;
using NodeEditorFramework;
using NodeEditorFramework.IO;
using NodeEditorFramework.Standard;
using UnityEngine;
using UnityEngine.Events;
using static CoreScriptsManager;

public interface IDialogueOverrideHandler
{
    Dictionary<string, Stack<InteractAction>> GetInteractionOverrides();
    void PushInteractionOverrides(string entityID, InteractAction action, Traverser traverser, Context context = null);
    void SetNode(ConnectionPort node);
    void SetNode(Node node);
    void SetSpeakerID(string ID);
}

public class InteractAction 
{
    public int taskHash;
    public string taskMissionName;
    public UnityAction action;
    public Traverser traverser;
    public bool prioritize;
    public Context context;
}

public class TaskManager : MonoBehaviour, IDialogueOverrideHandler
{
    public static TaskManager Instance = null;
    public static Dictionary<string, Stack<InteractAction>> interactionOverrides = new Dictionary<string, Stack<InteractAction>>();

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
        public Entity followEntity;
        public string missionName;
        public int dimension;
        public Color color = Color.red + Color.green / 2;

        public ObjectiveLocation(Vector2 location, string missionName, int dimension, Entity followEntity = null)
        {
            this.missionName = missionName;
            this.location = location;
            this.followEntity = followEntity;
            this.dimension = dimension;
        }
    }

    public static Dictionary<string, List<ObjectiveLocation>> objectiveLocations = new Dictionary<string, List<ObjectiveLocation>>();

    // Move to Dialogue System?
    public static string speakerID = null;
    public Dictionary<string, string> offloadingMissions = new Dictionary<string, string>();
    public Dictionary<string, List<string>> offloadingSectors = new Dictionary<string, List<string>>();

    public void ClearInteractionOverrides(string entityID)
    {
        if (TaskManager.interactionOverrides.ContainsKey(entityID))
        {
            var stack = TaskManager.interactionOverrides[entityID];
            if(stack.Count > 0)
            {
                TaskManager.interactionOverrides[entityID].Clear();
            }
        }
        else
        {
            Debug.LogWarning(entityID + " missing from interaction override dictionary!");
        }
    }

    public void PushInteractionOverrides(string entityID, InteractAction action, Traverser traverser, Context context = null) 
    {
        MissionTraverser missionTraverser = traverser as MissionTraverser;
        if (missionTraverser != null)
        {
            action.taskHash = missionTraverser.taskHash;
            action.traverser = traverser;
            action.taskMissionName = missionTraverser.nodeCanvas.missionName;
        }
        else if (context != null)
        {
            action.taskMissionName = context.missionName;
            action.context = context;
            action.taskHash = context.taskHash;
        }


        if (GetInteractionOverrides().ContainsKey(entityID))
        {
            GetInteractionOverrides()[entityID].Push(action);
        }
        else
        {
            var stack = new Stack<InteractAction>();
            stack.Push(action);
            GetInteractionOverrides().Add(entityID, stack);
        }
    }

    public static Entity GetSpeaker()
    {
        var speakerObj = SectorManager.instance.GetEntity(speakerID);
        return speakerObj;
    }

    public void Initialize(bool forceReInit = false)
    {
        if (Instance != null && Instance != this)
        {
            if (Instance.gameObject)
            {
                Destroy(Instance.gameObject);
            }
            Instance = null;
        }
        Instance = this;
        objectiveLocations = new Dictionary<string, List<ObjectiveLocation>>();
        speakerID = null;
        interactionOverrides = new Dictionary<string, Stack<InteractAction>>();

        // When adding new conditions with delegates, you MUST clear them out here
        MissionCondition.OnMissionStatusChange = null;
        SetPartDropRateNode.del = null;
        VariableConditionNode.OnVariableUpdate = null;
        WinBattleCondition.OnBattleLose = null;
        WinBattleCondition.OnBattleWin = null;
        WinSiegeCondition.OnSiegeWin = null;
        YardCollectCondition.OnYardCollect = null;
        UpgradeCoreCondition.OnCoreUpgrade = new UnityEvent();
        UsePartCondition.OnPlayerReconstruct = new UnityEvent();
        TestCondition.TestTrigger = new UnityEvent();
        initCanvases(forceReInit);
        questCanvasPaths = new List<string>();
        autoSaveEnabled = PlayerPrefs.GetString("TaskManager_autoSaveEnabled", "True") == "True";
    }

    void Update()
    {
        foreach (var ls in objectiveLocations.Values)
        {
            foreach (var loc in ls)
            {
                if (loc.followEntity)
                {
                    loc.location = loc.followEntity.transform.position;
                }
            }
        }
    }

    public static bool TraversersContainCheckpoint(string checkpointName)
    {
        foreach (var traverser in Instance.traversers)
        {
            if (traverser.lastCheckpointName == checkpointName)
            {
                return true;
            }
        }

        return false;
    }

    public void ClearCanvases(bool doNotClearCanvasPaths = false)
    {
        if (traversers != null)
        {
            for (int i = 0; i < traversers.Count; i++)
            {
                traversers[i].nodeCanvas.Destroy();
            }
        }

        if (sectorTraversers != null)
        {
            for (int i = 0; i < sectorTraversers.Count; i++)
            {
                sectorTraversers[i].nodeCanvas.Destroy();
            }
        }

        sectorTraversers = new List<SectorTraverser>();
        traversers = new List<MissionTraverser>();
        if (!doNotClearCanvasPaths)
        {
            questCanvasPaths.Clear();
        }
    }

    public void AddCanvasPath(string path)
    {
        questCanvasPaths.Add(path);
    }

    public static void StartQuests()
    {
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
        for (int i = 0; i < activeTasks.Count; i++)
        {
            if (activeTasks[i].taskID == taskID)
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
        if (incrementMode)
        {
            if (!taskVariables.ContainsKey(name))
            {
                Debug.LogFormat("Tried to read unknown task variable '{0}'", name);
                taskVariables[name] = 0;
            }
            taskVariables[name] += value;
        }
        else
        {
            taskVariables[name] = value;
        }

        if (VariableConditionNode.OnVariableUpdate != null)
        {
            VariableConditionNode.OnVariableUpdate.Invoke(name);
        }
    }

    public int GetTaskVariable(string name)
    {
        if (!taskVariables.ContainsKey(name))
        {
            Debug.LogFormat("Tried to read unknown task variable '{0}'", name);
            taskVariables[name] = 0;
        }

        return taskVariables[name];
    }

    void initCanvases(bool forceReInit)
    {
        if (initialized && !forceReInit)
        {
            return;
        }

        traversers = new List<MissionTraverser>();
        sectorTraversers = new List<SectorTraverser>();
        NodeCanvasManager.FetchCanvasTypes();
        NodeTypes.FetchNodeTypes();
        ConnectionPortManager.FetchNodeConnectionDeclarations();

        if (Instance)
        {
            Instance.ClearCanvases(true);
        }

        for (int i = 0; i < questCanvasPaths.Count; i++)
        {
            string finalPath = System.IO.Path.Combine(Application.streamingAssetsPath, questCanvasPaths[i]);

            if (finalPath.Contains(".taskdata"))
            {

                if (SaveHandler.instance?.GetSave()?.missions != null)
                {
                    var missionName = $"{System.IO.Path.GetFileNameWithoutExtension(finalPath)}";
                    var mission = SaveHandler.instance.GetSave().missions.Find(m => m.name == missionName);
                    if (mission != null)
                    {
                        if (mission.status == Mission.MissionStatus.Complete)
                            continue;
                        if (mission.prerequisites != null && SaveHandler.instance.GetSave().missions.Exists(m =>
                            mission.prerequisites.Contains(m.name)
                                && m.status != Mission.MissionStatus.Complete))
                        {
                            offloadingMissions.Add(missionName, finalPath);
                            continue;
                        }
                    }

                }

                var canvas = GetCanvas(finalPath) as QuestCanvas;

                if (canvas != null)
                {
                    traversers.Add(new MissionTraverser(canvas));
                }
            }
        }

        // reset all static condition variables
        SectorLimiterNode.LimitedSector = "";
        Entity.OnEntityDeath = null;
        UsePartCondition.OnPlayerReconstruct = new UnityEvent();
        WinBattleCondition.OnBattleWin = null;
        WinBattleCondition.OnBattleLose = null;
        SectorManager.SectorGraphLoad += startSectorGraph;

        initialized = true;
    }

    public static NodeCanvas GetCanvas(string path)
    {
        var XMLImport = new XMLImportExport();
        // Windows sucks man. But this should hopefully allow longer paths to work
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            path = System.IO.Path.Join("\\\\?\\", path);
        }

        if (!System.IO.File.Exists(path)) return null;
        return XMLImport.Import(path);
    }

    public void startNewQuest(string missionName)
    {
        if (!offloadingMissions.ContainsKey(missionName)) return;
        var mission = SaveHandler.instance.GetSave().missions.Find(m => m.name == missionName);
        if (mission.prerequisites != null && SaveHandler.instance.GetSave().missions.Exists(m =>
            mission.prerequisites.Contains(m.name)
                && m.status != Mission.MissionStatus.Complete))
            return;

        var path = offloadingMissions[missionName];
        offloadingMissions.Remove(missionName);
        var canvas = GetCanvas(path) as QuestCanvas;

        if (canvas != null)
        {
            var traverser = new MissionTraverser(canvas);
            if (traverser != null)
            {
                traversers.Add(traverser);
                var start = traverser.findRoot();
                if (start != null)
                {
                    start.TryAddMission();
                }

                traverser.StartQuest();
            }
        }

    }

    public void startSectorGraph(string sectorName)
    {
        if (!offloadingSectors.ContainsKey(sectorName)) return;
        var pathList = offloadingSectors[sectorName];
        offloadingSectors.Remove(sectorName);
        foreach (var path in pathList)
        {
            var canvas = GetCanvas(path) as SectorCanvas;
            if (canvas != null)
            {
                var traverser = new SectorTraverser(canvas);
                sectorTraversers.Add(traverser);
                if (traverser != null)
                {
                    var start = traverser.findRoot();
                    traverser.startNode = (LoadSectorNode)start;
                    traverser.StartQuest();
                }
            }
        }
    }

    // Traverse quest graph
    public void startQuests()
    {
        for (int i = 0; i < traversers.Count; i++)
        {
            var start = traversers[i].findRoot();
            if (start != null)
            {
                start.TryAddMission();
            }
        }

        // tasks
        var missions = PlayerCore.Instance.cursave.missions;

        foreach (var mission in missions)
        {
            if (traversers.Exists((t) => t.nodeCanvas.missionName == mission.name))
            {
                var traverser = traversers.Find((t) => t.nodeCanvas.missionName == mission.name);

                var tasks = mission.tasks.ToArray();
                if (mission.status != Mission.MissionStatus.Complete && mission.tasks.Count > 0)
                {
                    var task = mission.tasks[mission.tasks.Count - 1];
                    StartTaskNode start = traverser.nodeCanvas.nodes.Find((node) => { return node is StartTaskNode stn && stn.taskID == task.taskID; }) as StartTaskNode;
                    if (start != null)
                    {
                        traverser.ActivateTask(start.taskID);
                    }
                }

                if (traverser.findRoot().prerequisites != null)
                    mission.prerequisites = new List<string>(traverser.findRoot().prerequisites.ToArray());
                    
                if (traverser.findRoot().overrideCheckpoint)
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

    public void 
    setNode(ConnectionPort connection)
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
            if (canvas.nodes[i].GetID() == ID)
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
        if (node.Canvas is QuestCanvas)
        {
            (canvas.Traversal as MissionTraverser).SetNode(node);
        }
        else if (node.Canvas is DialogueCanvas)
        {
            (canvas.Traversal as DialogueTraverser).SetNode(node);
        }
        else
        {
            (canvas.Traversal as SectorTraverser).SetNode(node);
        }
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
        if (autoSaveEnabled && !loading)
        {
            saveHandler.Save();
        }
    }

    public void RemoveTraverser(MissionTraverser traverser)
    {
        traversers.Remove(traverser);
    }


    public Dictionary<string, Stack<InteractAction>> GetInteractionOverrides()
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
