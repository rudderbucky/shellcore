using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using NodeEditorFramework.Standard;
using NodeEditorFramework.IO;
using NodeEditorFramework;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance = null;
    public static Dictionary<string, UnityAction> interactionOverrides = new Dictionary<string, UnityAction>();

    List<Task> activeTasks = new List<Task>();

    public string[] questCanvasPaths;
    private List<QuestCanvas> questCanvases;
    private Node[] currentNodes;

    public Dictionary<string, int> taskVariables = new Dictionary<string, int>();

    bool initialized = false;

    public string lastTaskNodeID;

    public static string speakerName;

    public static Entity GetSpeaker() {
        var speakerObj = SectorManager.instance.GetObject(speakerName);
        return speakerObj?.GetComponent<Entity>();
    }

    public void Initialize()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
        initCanvases();
    }

    // TODO: add ability to set multiple paths
    public void SetCanvasPath(string path)
    {
        Debug.Log("Found Path");
        questCanvasPaths = new string[] {path};
    }

    public static void StartQuests() {
        Instance.startQuests();
    }
    public void AddTask(Task t)
    {
        activeTasks.Add(t);
        updateTaskList();
    }

    public void ActivateTask(string ID)
    {
        for (int i = 0; i < questCanvases[0].nodes.Count; i++)
        {
            var node = questCanvases[0].nodes[i] as StartTaskNode;
            if (node && node.taskID == ID)
            {
                node.StartTask();
            }
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
        questCanvases = new List<QuestCanvas>();
        NodeCanvasManager.FetchCanvasTypes();
        NodeTypes.FetchNodeTypes();
        ConnectionPortManager.FetchNodeConnectionDeclarations();

        var XMLImport = new XMLImportExport();

        for (int i = 0; i < questCanvasPaths.Length; i++)
        {
            string finalPath = System.IO.Path.Combine(Application.streamingAssetsPath, questCanvasPaths[i]);
            Debug.Log(finalPath);
            var canvas = XMLImport.Import(finalPath) as QuestCanvas;
            Debug.Log(canvas);
            if (canvas != null)
            {
                questCanvases.Add(canvas);
                foreach (Node node in canvas.nodes)
                {
                    ConnectionPortManager.UpdateConnectionPorts(node);
                    foreach (ConnectionPort port in node.connectionPorts)
                        port.Validate(node);
                }
            }
        }

        Debug.Log(questCanvases.Count);

        // reset all static condition variables
        SectorLimiterNode.LimitedSector = "";
        DestroyEntityCondition.OnUnitDestroyed = null;
        UsePartCondition.OnPlayerReconstruct = new UnityEvent();
        WinBattleCondition.OnBattleWin = null;

        currentNodes = new Node[questCanvases.Count];
        initialized = true;
    }

    // Traverse quest graph
    public void startQuests()
    {
        if(lastTaskNodeID == null || lastTaskNodeID == "")
        {
            for (int i = 0; i < questCanvases.Count; i++)
            {
                startQuestline(i);
            }
        }
        else
        {
            Traverse();
        }
    }

    public Node GetCurrentNode()
    {
        return currentNodes[0];
    }

    public void startQuestline(int index)
    {
        // If there's no current node, find root node
        if(currentNodes[index] == null && questCanvases[index] != null)
        {
            currentNodes[index] = findRoot(index);
            if(currentNodes[index] == null)
            {
                questCanvases[index] = null;
                return;
            }
            //Start quest
            setNode(currentNodes[index]);
        }
    }

    public void setNode(ConnectionPort connection)
    {
        if (connection.connected())
        {
            setNode(connection.connections[0].body);
        }
    }

    public void setNode(string ID)
    {
        for (int i = 0; i < questCanvases[0].nodes.Count; i++)
        {
            Debug.Log(questCanvases[0] + " " +  questCanvases[0].nodes + " " + questCanvases[0].nodes[i]);
            if(questCanvases[0].nodes[i].GetID() == ID)
            {
                setNode(questCanvases[0].nodes[i]);
            }
        }
    }

    public void setNode(Node node)
    {
        //TODO: Traverser object for each canvas, multiple simultaneous quests
        for (int i = 0; i < questCanvases.Count; i++)
        {
            if(questCanvases[i].nodes.Contains(node))
            {
                currentNodes[i] = node;
                lastTaskNodeID = currentNodes[0].GetID();
                break;
            }
        }
        if(SystemLoader.AllLoaded)
            Traverse();
    }

    private Node findRoot(int index)
    {
        for (int i = 0; i < questCanvases.Count; i++)
        {
            for (int j = 0; j < questCanvases[i].nodes.Count; j++)
            {
                if(questCanvases[i].nodes[j].GetName == "StartNode")
                {
                    return questCanvases[i].nodes[j];
                }
            }
        }
        return null;
    }

    private void Traverse()
    {
        while(true)
        {
            if (currentNodes == null)
                return;
            int outputIndex = currentNodes[0].Traverse();
            if (outputIndex == -1)
                break;
            if (!currentNodes[0].outputKnobs[outputIndex].connected())
                break;
            currentNodes[0] = currentNodes[0].outputKnobs[outputIndex].connections[0].body;
        }
    }
}
