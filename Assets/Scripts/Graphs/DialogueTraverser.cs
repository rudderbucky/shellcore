using NodeEditorFramework.Standard;
using NodeEditorFramework.IO;
using NodeEditorFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTraverser : Traverser
{
    new DialogueCanvas nodeCanvas;
    public DialogueTraverser(DialogueCanvas canvas) : base(canvas)
    {
        nodeCanvas = canvas;
        startNodeName = "StartDialogueNode";
    }

    public override void SetNode(Node node)
    {
        Debug.Log("Dialogue Canvas " + nodeCanvas + " now setting node: " + node);
        if(node is StartDialogueNode)
        {
            (node as StartDialogueNode).state = NodeEditorGUI.NodeEditorState.Dialogue;
        }
        if(node as DialogueNode)
            (node as DialogueNode).state = NodeEditorGUI.NodeEditorState.Dialogue;
        if(node as EndDialogue)
            (node as EndDialogue).state = NodeEditorGUI.NodeEditorState.Dialogue;
        base.SetNode(node);
    }

    protected override void Traverse()
    {
        while (true)
        {
            //Debug.Log("Dialogue Canvas " +  nodeCanvas + " now traversing: " + currentNode);
            if(currentNode is StartDialogueNode)
            {
                (currentNode as StartDialogueNode).state = NodeEditorGUI.NodeEditorState.Dialogue;
            }
            if(currentNode as DialogueNode)
                (currentNode as DialogueNode).state = NodeEditorGUI.NodeEditorState.Dialogue;
            if(currentNode as EndDialogue)
                (currentNode as EndDialogue).state = NodeEditorGUI.NodeEditorState.Dialogue;
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
