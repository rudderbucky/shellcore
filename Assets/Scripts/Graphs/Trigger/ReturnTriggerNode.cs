using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Trigger/Return Trigger")]
    public class ReturnTriggerNode : Node
    {
        public override string GetName
        {
            get { return "ReturnTriggerNode"; }
        }

        public override string Title
        {
            get { return "Return Trigger"; }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        public override bool AllowRecursion
        {
            get { return true; }
        }

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

    
        public override int Traverse()
        {
            Debug.LogWarning("Return trigger traversal called. This is not supposed to happen.");
            return -1;
        }

    }
}