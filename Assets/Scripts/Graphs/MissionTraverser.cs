using NodeEditorFramework.Standard;
using NodeEditorFramework.IO;
using NodeEditorFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionTraverser : Traverser
{
    string CPName;
    new QuestCanvas nodeCanvas;
    public MissionTraverser(QuestCanvas canvas) : base(canvas)
    {
        nodeCanvas = canvas;
        startNodeName = "StartMissionNode";
    }

    public override void StartQuest()
    {
        // If the quest has been started, continue
        nodeCanvas.missionName = findRoot().missionName;
        if(CPName == (nodeCanvas.missionName + "_complete")) return;
        base.StartQuest();
        if(currentNode == null) TaskManager.Instance.RemoveTraverser(this);
    }

    public override void activateCheckpoint(string CPName)
    {
        this.CPName = CPName;
        nodeCanvas.missionName = findRoot().missionName;
        base.activateCheckpoint(CPName);
        for (int i = 0; i < nodeCanvas.nodes.Count; i++)
        {
            var node = nodeCanvas.nodes[i];
            if (node is StartTaskNode && (nodeCanvas.missionName + "_" + (node as StartTaskNode).taskID) == CPName)
            {
                (node as StartTaskNode).forceTask = true;
                SetNode(node);
            }
            Debug.Log(nodeCanvas.missionName + "_" + (node as StartTaskNode)?.taskID + " " + CPName);
        }
    }

    public new StartMissionNode findRoot()
    {
        return base.findRoot() as StartMissionNode; // root node of a QuestCanvas must be a StartMissionNode
    }

    public virtual void ActivateTask(string ID)
    {
        for (int i = 0; i < nodeCanvas.nodes.Count; i++)
        {
            var node = nodeCanvas.nodes[i] as StartTaskNode;
            if (node && node.taskID == ID)
            {
                node.StartTask();
            }
        }
    }
}
