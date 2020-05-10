using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/Passive Dialogue Node")]
    public class PassiveDialogueNode : Node
    {
        public override string GetName { get { return "PassiveDialogueNode"; } }
        public override string Title { get { return "Passive Dialogue (Instant)"; } }

        public override Vector2 MinSize { get { return new Vector2(200f, 100f); } }
        public override bool AutoLayout { get { return true; } }
        public override bool AllowRecursion { get { return true; } }

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right)]
        public ConnectionKnob output;
        ConnectionKnobAttribute OutStyle = new ConnectionKnobAttribute("Output", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right);
        public string text;
        public string id;

        public override void NodeGUI()
        {
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.Label("Text:");
            text = GUILayout.TextArea(text);

            GUILayout.Label("ID:");
            id = GUILayout.TextArea(id);
        }

        public override int Traverse()
        {
            DialogueSystem.Instance.PushPassiveDialogue(id, text);
            Debug.Log("done");
            return 0;
        }
    }
}
