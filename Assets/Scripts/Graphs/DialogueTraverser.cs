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
        SetDialogueState(currentNode, NodeEditorGUI.NodeEditorState.Dialogue);
        base.SetNode(node);
    }

    protected override void Traverse()
    {
        while (true)
        {
            SetDialogueState(currentNode, NodeEditorGUI.NodeEditorState.Dialogue);
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
