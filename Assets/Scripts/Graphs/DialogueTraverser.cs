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
}
