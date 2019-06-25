using NodeEditorFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(true, "Dialogue/DialogueNode")]
    public class DialogueNode : Node
    {
        public override string GetID { get { return "DialogueNode"; } }
        public override string Title { get { return "Dialogue"; } }

        public override bool AutoLayout { get { return true; } }

        [ConnectionKnob("Input Left", Direction.In, "Dialogue", NodeSide.Left)]
        public ConnectionKnob input;

        [ConnectionKnob("Cancel", Direction.Out, "Flow", NodeSide.Right)]
        public ConnectionKnob cancel;

        ConnectionKnobAttribute outputAttribute = new ConnectionKnobAttribute("Output ", Direction.Out, "Dialogue", ConnectionCount.Single, NodeSide.Right);

        public Color speakerColor;
        public string speakerTitle;
        public string text;
        public List<string> answers = new List<string>();
        //TODO: list of outputs or some other way to distinguish between choices and cancel

        public override void NodeGUI()
        {
            GUILayout.Label("Title:");
            speakerTitle = RTEditorGUI.TextField(speakerTitle, GUILayout.MinHeight(64f), GUILayout.MinWidth(200f));
            GUILayout.Label("Text:");
            text = GUILayout.TextArea( text);
            GUILayout.Label("Answers:");
            for (int i = 0; i < outputKnobs.Count; i++)
            {
                RTEditorGUI.Seperator();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                {
                    DeleteConnectionPort(outputPorts[i]);
                    answers.RemoveAt(i);
                    i--;
                    continue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                answers[i] = RTEditorGUI.TextField(answers[i]);
                outputKnobs[i].DisplayLayout();
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false), GUILayout.MinWidth(100f)))
            {
                CreateConnectionKnob(outputAttribute);
                answers.Add("");
            }
            GUILayout.EndHorizontal();
            cancel.DisplayLayout();
        }

        public override int Traverse()
        {
            return -1;
        }
    }
}
