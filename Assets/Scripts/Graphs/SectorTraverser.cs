using NodeEditorFramework.Standard;
using NodeEditorFramework.IO;
using NodeEditorFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorTraverser : Traverser
{
    new SectorCanvas nodeCanvas;
    public LoadSectorNode startNode;
    public SectorTraverser(SectorCanvas canvas) : base(canvas)
    {
        nodeCanvas = canvas;
        startNodeName = "LoadSectorNode";
    }

    ~SectorTraverser()
    {
        SectorManager.OnSectorLoad -= LoadSector;
    }

    public override void SetNode(Node node)
    {
        Debug.Log("Sector Canvas " + nodeCanvas + " now setting node: " + node);
        SetDialogueState(currentNode, NodeEditorGUI.NodeEditorState.Dialogue);
        base.SetNode(node);
    }

    protected override void Traverse()
    {
        while (true)
        {
            //Debug.Log("Dialogue Canvas " +  nodeCanvas + " now traversing: " + currentNode);
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

    public override void StartQuest()
    {
        SectorManager.OnSectorLoad += LoadSector;
    }

    void LoadSector(string name)
    {
        if (name == startNode.sectorName)
        {
            currentNode = startNode;
            Traverse();
        }
        else
        {
            if (currentNode is TimelineNode)
                TaskManager.Instance.StopAllCoroutines();
            if (currentNode is ConditionGroupNode)
            {
                var cgn = currentNode as ConditionGroupNode;
                cgn.DeInit();
            }
        }
    }
}
