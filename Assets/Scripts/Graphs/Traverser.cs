using NodeEditorFramework.Standard;
using NodeEditorFramework.IO;
using NodeEditorFramework;
using System;
using UnityEngine;

public class Traverser : NodeCanvasTraversal
{
    public SectorLimiterNode limiterStart;
    public string lastCheckpointName;

    public Traverser(NodeCanvas canvas) : base(canvas)
    {
        nodeCanvas = canvas;
        canvas.Traversal = this;
        foreach (Node node in canvas.nodes)
        {
            ConnectionPortManager.UpdateConnectionPorts(node);
            foreach (ConnectionPort port in node.connectionPorts)
                port.Validate(node);
        }
    }

    void Traverse()
    {
        while (true)
        {
            if (currentNode == null)
                return;
            int outputIndex = currentNode.Traverse();
            if (outputIndex == -1)
                break;
            if (!currentNode.outputKnobs[outputIndex].connected())
                break;
            currentNode = currentNode.outputKnobs[outputIndex].connections[0].body;
        }
    }

    string CPName;
    public void StartQuest()
    {
        // If the quest has been started, continue
        (nodeCanvas as QuestCanvas).missionName = (findRoot() as StartMissionNode).missionName;
        if(CPName == ((nodeCanvas as QuestCanvas).missionName + "_complete")) return;
        if (currentNode != null)
        {
            Debug.Log("Continuing quest from " + currentNode.GetName);
            Traverse();
            return;
        }

        // If there's no current node, find root node
        if (currentNode == null && nodeCanvas != null)
        {
            currentNode = findRoot();
            Debug.Log("Root found for canvas " + nodeCanvas.canvasName + ":" + currentNode);
            if (currentNode == null)
            {
                nodeCanvas = null;
                TaskManager.Instance.RemoveTraverser(this);
                return;
            }
            //Start quest
            Debug.Log("Starting...");
            SetNode(currentNode);
        }
    }

    public Node findRoot()
    {
        var startNodeName = nodeCanvas as QuestCanvas ? "StartMissionNode" : "StartDialogueNode";
        for (int j = 0; j < nodeCanvas.nodes.Count; j++)
        {
            if (nodeCanvas.nodes[j].GetName == startNodeName)
            {
                return nodeCanvas.nodes[j];
            }
        }
        return null;
    }

    public void activateCheckpoint(string CPName)
    {
        this.CPName = CPName;
        (nodeCanvas as QuestCanvas).missionName = (findRoot() as StartMissionNode).missionName;
        for (int i = 0; i < nodeCanvas.nodes.Count; i++)
        {
            var node = nodeCanvas.nodes[i];
            if (node is CheckpointNode && (node as CheckpointNode).checkpointName == CPName)
            {
                SetNode(node);
            }
            else if (node is StartTaskNode && ((nodeCanvas as QuestCanvas).missionName + "_" + (node as StartTaskNode).taskID) == CPName)
            {
                (node as StartTaskNode).forceTask = true;
                SetNode(node);
            }
            Debug.Log((nodeCanvas as QuestCanvas).missionName + "_" + (node as StartTaskNode)?.taskID + " " + CPName);
        }
    }

    public void ActivateTask(string ID)
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

    public override void SetNode(Node node)
    {
        currentNode = node;
        if (SystemLoader.AllLoaded)
            Traverse();
        else Debug.LogWarning("Traverser failed to traverse because system failed to load. Abort.");
    }
}