using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO: Node
public class TestCondition : TaskCondition
{
    public override void Init(NodeEditorFramework.Standard.StartTaskNode node)
    {
        Node = node;
        OnTrigger.AddListener(null);
    }

    public override void DeInit()
    {
        OnTrigger.RemoveListener(null);
    }
}

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Test")]
    public class TestCondition : Node
    {
        //Node things
        public const string ID = "StartNode";
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Start"; } }

        public override bool AllowRecursion { get { return true; } }
        public override bool ContinueCalculation { get { return true; } }

        [ConnectionKnob("Output Right", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob outputRight;
    }
}