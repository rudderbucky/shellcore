using NodeEditorFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/Condition Check Node")]
    public class DialogueConditionCheckNode : Node
    {
        public override string GetName { get { return "DialogueConditionCheckNode"; } }
        public override string Title { get { return "Dialogue Condition Check"; } }

        public override Vector2 MinSize { get { return new Vector2(200f, 100f); } }
        public override bool AutoLayout { get { return true; } }
        public override bool AllowRecursion { get { return true; } }

        [ConnectionKnob("Input Left", Direction.In, "Dialogue", NodeSide.Left)]
        public ConnectionKnob input;

        [ConnectionKnob("Output ", Direction.Out, "Dialogue", NodeSide.Right)]
        public ConnectionKnob outputPass;
        
        [ConnectionKnob("Output ", Direction.Out, "Dialogue", ConnectionCount.Single, NodeSide.Right, 20)]
        public ConnectionKnob outputFail;
        public string checkpointName;

        public override void NodeGUI()
        {
            // TODO: Add more condition check features
            GUILayout.Label("Top knob is pass, below it is fail");
            GUILayout.Label("Checkpoint for passing (other features coming soon):");
            GUILayout.BeginHorizontal();
            checkpointName = GUILayout.TextArea(checkpointName);
            GUILayout.EndHorizontal();
        }

        public override int Traverse()
        {
            if(TaskManager.TraversersContainCheckpoint(checkpointName))
            {
                TaskManager.Instance.setNode(outputPass);
            }
            else TaskManager.Instance.setNode(outputFail);
            return -1;
        }
    }
}
