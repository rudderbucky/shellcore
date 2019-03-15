using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    //Singleton pattern
    //static TaskManager instance;

    static List<Task> tasks;

    static List<NodeEditorFramework.Standard.QuestGraph> questGraphs;

    static Task GetTask(string ID)
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

    static Dictionary<string, int> taskVariables;

    static void setTaskVariable(string name, int value)
    {
        taskVariables[name] = value;
    }

    static int getTaskVariable(string name)
    {
        if(taskVariables.ContainsKey(name))
        {
            return taskVariables[name];
        }
        Debug.LogWarningFormat("Tried to read unknown task variable '{0}'", name);
        return 0;
    }
}
