using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance = null;
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    public List<Task> tasks;

    public NodeEditorFramework.Standard.QuestGraph[] questGraphs;
    private NodeEditorFramework.Node[] currentNodes;

    public Task GetTask(string ID)
    {
        for (int i = 0; i < tasks.Count; i++)
        {
            if(tasks[i].taskID == ID)
            {
                return tasks[i];
            }
        }
        return null;
    }

    public Dictionary<string, int> taskVariables;

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
        for (int i = 0; i < questGraphs.Length; i++)
        {
            startQuestline(i);
        }
    }

    public void startQuestline(int index)
    {
        // If there's no current node, find root node
        if(currentNodes[index] == null && questGraphs[index] != null)
        {
            currentNodes[index] = findRoot(index);
            if(currentNodes[index] == null)
            {
                questGraphs[index] = null;
                return;
            }
            //Start quest
            setNode(currentNodes[index]);
        }
    }

    public void setNode(NodeEditorFramework.Node node)
    {
        for(int i = 0; i < questGraphs.Length; i++)
        {
            if(questGraphs[i].nodes.Contains(node))
            {
                currentNodes[i] = node;
                break;
            }
        }
        node.Calculate();
    }

    private NodeEditorFramework.Node findRoot(int index)
    {
        for (int i = 0; i < questGraphs.Length; i++)
        {
            for (int j = 0; j < questGraphs[i].nodes.Count; j++)
            {
                if(questGraphs[i].nodes[j].GetID == "StartNode")
                {
                    return questGraphs[i].nodes[j];
                }
            }
        }
        return null;
    }
}
