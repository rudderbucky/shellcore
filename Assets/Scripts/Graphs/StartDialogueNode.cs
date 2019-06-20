using NodeEditorFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(true, "Dialogue/Start Dialogue")]
    public class StartDialogueNode : Node
    {
        public override string GetID { get { return "StartDialogueNode"; } }
        public override string Title { get { return "Start Dialogue"; } }

        public override bool AutoLayout { get { return true; } }

        [ConnectionKnob("Input Left", Direction.In, "Dialogue", NodeSide.Left)]
        public ConnectionKnob input;

        ConnectionKnobAttribute outputAttribute = new ConnectionKnobAttribute("Output ", Direction.Out, "Dialogue", ConnectionCount.Single, NodeSide.Right);

        public Color speakerColor;
        public string speakerTitle;
        public string text;
        public List<string> answers = new List<string>();

        public override void NodeGUI()
        {
            GUILayout.Label("Title:");
            speakerTitle = RTEditorGUI.TextField(speakerTitle, GUILayout.MinHeight(64f), GUILayout.MinWidth(200f));
            GUILayout.Label("Text:");
            text = GUILayout.TextArea(text);
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
        }

        public override int Traverse()
        {
            return -1;
        }
    }
}
