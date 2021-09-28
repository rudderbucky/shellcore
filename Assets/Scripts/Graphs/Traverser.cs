using NodeEditorFramework;
using NodeEditorFramework.Standard;
using UnityEngine;

public class Traverser : NodeCanvasTraversal
{
    public string lastCheckpointName;
    protected string startNodeName;

    public Traverser(NodeCanvas canvas) : base(canvas)
    {
        nodeCanvas = canvas;
        if (!(this is TriggerTraverser)) canvas.Traversal = this;
        foreach (Node node in canvas.nodes)
        {
            ConnectionPortManager.UpdateConnectionPorts(node);
            foreach (ConnectionPort port in node.connectionPorts)
            {
                port.Validate(node);
            }
        }
    }

    protected virtual void Traverse()
    {
        while (true)
        {
            if (currentNode == null)
            {
                return;
            }

            int outputIndex = currentNode.Traverse();
            if (outputIndex == -1)
            {
                break;
            }

            if (!currentNode.outputKnobs[outputIndex].connected())
            {
                break;
            }

            currentNode = currentNode.outputKnobs[outputIndex].connections[0].body;
        }
    }

    public virtual void StartQuest()
    {
        if (currentNode != null)
        {
            Traverse();
            return;
        }

        // If there's no current node, find root node
        if (currentNode == null && nodeCanvas != null)
        {
            currentNode = findRoot();

            if (currentNode == null)
            {
                nodeCanvas = null;
                return;
            }
            //Start quest

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
        if (string.IsNullOrEmpty(CPName))
        {
            return false;
        }

        for (int i = 0; i < nodeCanvas.nodes.Count; i++)
        {
            if (nodeCanvas.nodes[i] is CheckpointNode node && node.checkpointName == CPName)
            {
                currentNode = node;
                return true;
            }
        }

        return false;
    }

    public override void SetNode(Node node)
    {
        currentNode = node;
        if (SystemLoader.AllLoaded)
        {
            Traverse();
        }
        else
        {
            Debug.LogWarning("Traverser failed to traverse because system failed to load. Abort.");
        }
    }

    protected void SetDialogueState(Node node, NodeEditorGUI.NodeEditorState state)
    {
        if (node is StartDialogueNode start)
        {
            start.state = state;
        }

        if (node is DialogueNode dialogue)
        {
            dialogue.state = state;
        }

        if (node is EndDialogue end)
        {
            end.state = state;
        }
        if (node is ClearDialogueNode clear)
        {
            clear.state = state;
        }
    }
}
