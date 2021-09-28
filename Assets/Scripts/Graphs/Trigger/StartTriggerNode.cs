using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Trigger/Start Trigger")]
    public class StartTriggerNode : Node
    {
        public override string GetName
        {
            get { return "StartTriggerNode"; }
        }

        public override string Title
        {
            get { return "Start Trigger"; }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        public override bool AllowRecursion
        {
            get { return true; }
        }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right)]
        public ConnectionKnob output;
        public string triggerName;

        public override void NodeGUI()
        {
            GUILayout.Label("Trigger Name: ");
            triggerName = GUILayout.TextField(triggerName);
        }
    }
}