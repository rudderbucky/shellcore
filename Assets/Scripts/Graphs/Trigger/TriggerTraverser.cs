using System.Collections;
using System.Collections.Generic;
using NodeEditorFramework;
using NodeEditorFramework.Standard;
using UnityEngine;

public class TriggerTraverser : Traverser
{
    public StartTriggerNode startTriggerNode;
    private Traverser nextTraverser;
    private Node nextNode;
    string triggerName;
    public TriggerTraverser(string triggerName, NodeCanvas canvas, Traverser nextTraverser, Node nextNode) : base(canvas)
    {
        this.triggerName = triggerName;
        nodeCanvas = canvas;
        this.nextTraverser = nextTraverser;
        this.nextNode = nextNode;
        startNodeName = "StartTriggerNode";
    }

    public new StartTriggerNode findRoot()
    {
        return nodeCanvas.nodes.Find(x => x is StartTriggerNode trigger && trigger.triggerName == triggerName) as StartTriggerNode;
    }

    public override void StartQuest()
    {
        currentNode = findRoot();
        Traverse();
    }

    protected override void Traverse()
    {
        while (true)
        {

            if (currentNode == null)
            {
                if (TriggerManager.instance.traversers.Contains(this))
                {
                    TriggerManager.instance.traversers.Remove(this);
                }


                return;
            }

            if (currentNode is ConditionGroupNode groupNode)
            {
                groupNode.SetTriggerTraverser(this);
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

            if (currentNode is ReturnTriggerNode)
            {
                if (TriggerManager.instance.traversers.Contains(this))
                {
                    TriggerManager.instance.traversers.Remove(this);
                }
                if (nextNode != null)
                {
                    nextTraverser.SetNode(nextNode);
                }

                return;
            }
        }
    }
}
