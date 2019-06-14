using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Standard;
using NodeEditorFramework.IO;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance = null;

    List<Task> activeTasks = new List<Task>();

    public string[] questCanvasPaths;
    private List<QuestCanvas> questCanvases;
    private NodeEditorFramework.Node[] currentNodes;

    public Dictionary<string, int> taskVariables;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;

        startQuests();
    }

    private void Update() // TEMP - DELETE BEFORE COMMIT
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            TestCondition.TestTrigger.Invoke();
        }
    }

    public void AddTask(Task t)
    {
        activeTasks.Add(t);
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
                break;
            }
        }
    }

    public void SetTaskVariable(string name, int value)
    {
        taskVariables[name] = value;
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
        NodeEditorFramework.NodeCanvasManager.FetchCanvasTypes();
        NodeEditorFramework.NodeTypes.FetchNodeTypes();
        NodeEditorFramework.ConnectionPortManager.FetchNodeConnectionDeclarations();

        var XMLImport = new XMLImportExport();

        for (int i = 0; i < questCanvasPaths.Length; i++)
        {
            var canvas = XMLImport.Import(questCanvasPaths[i]) as QuestCanvas;
            if (canvas != null)
            {
                questCanvases.Add(canvas);
                foreach (NodeEditorFramework.Node node in canvas.nodes)
                {
                    NodeEditorFramework.ConnectionPortManager.UpdateConnectionPorts(node);
                    foreach (NodeEditorFramework.ConnectionPort port in node.connectionPorts)
                        port.Validate(node);
                }
            }
        }



        currentNodes = new NodeEditorFramework.Node[questCanvases.Count];
        for (int i = 0; i < questCanvases.Count; i++)
        {
            startQuestline(i);
        }
    }

    // COPIED FOR REFERENCE - DELETE BEFORE COMMIT

//    private static void setupBaseFramework()
//    {
//        CheckEditorPath();

//        // Init Resource system. Can be called anywhere else, too, if it's needed before.
//        ResourceManager.SetDefaultResourcePath(editorPath + "Resources/");

//        // Run fetching algorithms searching the script assemblies for Custom Nodes / Connection Types / NodeCanvas Types
//        ConnectionPortStyles.FetchConnectionPortStyles();
//        NodeTypes.FetchNodeTypes();
//        NodeCanvasManager.FetchCanvasTypes();
//        ConnectionPortManager.FetchNodeConnectionDeclarations();
//        ImportExportManager.FetchIOFormats();

//        // Setup Callback system
//        NodeEditorCallbacks.SetupReceivers();
//        NodeEditorCallbacks.IssueOnEditorStartUp();

//        // Init input
//        NodeEditorInputSystem.SetupInput();

//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.update -= Update;
//        UnityEditor.EditorApplication.update += Update;
//#endif

//        initiatedBase = true;
//    }

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

    public void setNode(NodeEditorFramework.Node node)
    {
        //TODO: differentiate better between quests
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

    private NodeEditorFramework.Node findRoot(int index)
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
