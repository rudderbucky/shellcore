using NodeEditorFramework.Standard;
using NodeEditorFramework.IO;
using NodeEditorFramework;
using System;
using UnityEngine;

public class Traverser : NodeCanvasTraversal
{
    public string lastCheckpointName;
    protected string startNodeName;

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

    protected virtual void Traverse()
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

    public virtual void StartQuest()
    {
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
                return;
            }
            //Start quest
            Debug.Log("Starting...");
            SetNode(currentNode);
        }
    }

    public virtual Node findRoot()
    {
        for (int j = 0; j < nodeCanvas.nodes.Count; j++)
        {
            if (nodeCanvas.nodes[j].GetName == startNodeName)
            {
                return nodeCanvas.nodes[j];
            }
        }
        return null;
    }

    public virtual bool activateCheckpoint(string CPName)
    {
        if(CPName == null || CPName == "") return false;
        for (int i = 0; i < nodeCanvas.nodes.Count; i++)
        {
            var node = nodeCanvas.nodes[i];
            if (node is CheckpointNode && (node as CheckpointNode).checkpointName == CPName)
            {
                currentNode = node;
                return true;
            }
        }
        return false;
    }

    public override void SetNode(Node node)
    {
        // Debug.Log(currentNode + " Setting Node");
        currentNode = node;
        if (SystemLoader.AllLoaded)
            Traverse();
        else Debug.LogWarning("Traverser failed to traverse because system failed to load. Abort.");
    }
}