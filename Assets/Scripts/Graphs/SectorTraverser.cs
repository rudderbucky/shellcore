﻿using NodeEditorFramework;
using NodeEditorFramework.Standard;
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
        SectorManager.SectorGraphLoad -= LoadSector;
    }

    public override void SetNode(Node node)
    {
        Debug.Log($"Sector Canvas {nodeCanvas} now setting node: {node}");
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

    public new LoadSectorNode findRoot()
    {
        return base.findRoot() as LoadSectorNode; // root node of a SectorCanvas must be a EnterSectorNode
    }

    public override void StartQuest()
    {
        SectorManager.SectorGraphLoad += LoadSector;
        if (SectorManager.instance.current != null)
        {
            LoadSector(SectorManager.instance.current.sectorName);
        }
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
            {
                TaskManager.Instance.StopAllCoroutines();
            }

            if (currentNode is ConditionGroupNode cgn)
            {
                cgn.DeInit();
            }
        }
    }
}
