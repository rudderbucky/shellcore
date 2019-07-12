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
    public Text taskDescriptions;
    private List<QuestCanvas> questCanvases;
    private Node[] currentNodes;

    public Dictionary<string, int> taskVariables = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        startQuests();
    }

    private void Update() // TEMP - DELETE BEFORE FINAL RELEASE
    {
        if(Input.GetKeyDown(KeyCode.T) && TestCondition.TestTrigger != null)
        {
            TestCondition.TestTrigger.Invoke();
        }
    }

    public void AddTask(Task t)
    {
        activeTasks.Add(t);
        updateTaskList();
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

    void updateTaskList()
    {
        string taskList = "Current tasks:\n\n";

        for(int i = 0; i < activeTasks.Count; i++)
        {
            taskList += activeTasks[i].objectived + "\n\n";
        }

        taskDescriptions.text = taskList;
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

    // Traverse quest graph
    public void startQuests()
    {
        questCanvases = new List<QuestCanvas>();
        NodeCanvasManager.FetchCanvasTypes();
        NodeTypes.FetchNodeTypes();
        ConnectionPortManager.FetchNodeConnectionDeclarations();

        var XMLImport = new XMLImportExport();

        for (int i = 0; i < questCanvasPaths.Length; i++)
        {
            string finalPath = System.IO.Path.Combine(Application.streamingAssetsPath, questCanvasPaths[i]);
            var canvas = XMLImport.Import(finalPath) as QuestCanvas;
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

        currentNodes = new Node[questCanvases.Count];
        for (int i = 0; i < questCanvases.Count; i++)
        {
            startQuestline(i);
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
        if(connection.connected())
        {
            setNode(connection.connections[0].body);
        }
    }

    public void setNode(Node node)
    {
        //TODO: Traverser object for each canvas
        for(int i = 0; i < questCanvases.Count; i++)
        {
            if(questCanvases[i].nodes.Contains(node))
            {
                currentNodes[i] = node;
                break;
            }
        }
        Traverse();
    }

    private Node findRoot(int index)
    {
        for (int i = 0; i < questCanvases.Count; i++)
        {
            for (int j = 0; j < questCanvases[i].nodes.Count; j++)
            {
                if(questCanvases[i].nodes[j].GetID == "StartNode")
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
            int outputIndex = currentNodes[0].Traverse();
            if (outputIndex == -1)
                break;
            if (!currentNodes[0].outputKnobs[outputIndex].connected())
                break;
            currentNodes[0] = currentNodes[0].outputKnobs[outputIndex].connections[0].body;
        }
    }
}
