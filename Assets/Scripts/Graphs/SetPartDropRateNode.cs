using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "AI/Set Part Drop Rate")]
    public class SetPartDropRateNode : Node
    {
        public override string GetName { get { return "SetPartDropRate"; } }
        public override string Title { get { return "Set Part Drop Rate"; } }
        public override bool AllowRecursion { get { return true; } }
        public override bool AutoLayout { get { return true; } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;
        public float dropRate;
        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            dropRate = RTEditorGUI.FloatField("Drop Rate", dropRate, GUILayout.MinWidth(400));

        }

        public override int Traverse()
        {
            Entity.partDropRate = dropRate;
            return 0;
        }
    }
}
