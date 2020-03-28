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

    public override bool activateCheckpoint(string CPName)
    {
        if(CPName == null || CPName == "") return false;
        this.CPName = CPName;
        nodeCanvas.missionName = findRoot().missionName;
        if(base.activateCheckpoint(CPName)) return true;
        for (int i = 0; i < nodeCanvas.nodes.Count; i++)
        {
            var node = nodeCanvas.nodes[i];
            if (node is StartTaskNode && (node as StartTaskNode).taskName == CPName)
            {
                (node as StartTaskNode).forceTask = true;
                currentNode = node;
                return true;
            }
        }
        Debug.LogWarning("Could not find checkpoint: " + CPName);
        return false;
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

    public override void SetNode(Node node)
    {
        Debug.Log("Mission Canvas " + nodeCanvas.missionName + " now setting node: " + node);
        if(node is StartDialogueNode)
        {
            StartDialogueNode.missionCanvasNode = node as StartDialogueNode;
        }
        if(node is DialogueNode)
            (node as DialogueNode).state = NodeEditorGUI.NodeEditorState.Mission;
        if(node is EndDialogue)
            (node as EndDialogue).state = NodeEditorGUI.NodeEditorState.Mission;
        base.SetNode(node);
    }

    protected override void Traverse()
    {
        while (true)
        {
            Debug.Log("Mission Canvas " +  nodeCanvas.missionName + " now traversing: " + currentNode);
            if(currentNode is StartDialogueNode)
            {
                StartDialogueNode.missionCanvasNode = currentNode as StartDialogueNode;
            }
            if(currentNode is DialogueNode)
                (currentNode as DialogueNode).state = NodeEditorGUI.NodeEditorState.Mission;
            if(currentNode is EndDialogue)
                (currentNode as EndDialogue).state = NodeEditorGUI.NodeEditorState.Mission;
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
}
