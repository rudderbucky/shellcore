using NodeEditorFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/Dialogue Node")]
    public class DialogueNode : Node
    {
        public override string GetID { get { return "DialogueNode"; } }
        public override string Title { get { return "Dialogue"; } }

        public override Vector2 MinSize { get { return new Vector2(200f, 100f); } }
        public override bool AutoLayout { get { return true; } }
        public override bool AllowRecursion { get { return true; } }

        [ConnectionKnob("Input Left", Direction.In, "Dialogue", NodeSide.Left)]
        public ConnectionKnob input;

        [ConnectionKnob("Cancel", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob cancel;

        ConnectionKnobAttribute outputAttribute = new ConnectionKnobAttribute("Output ", Direction.Out, "Dialogue", ConnectionCount.Single, NodeSide.Right);

        public Color textColor = Color.white;
        public string text;
        public List<string> answers = new List<string>();

        public override void NodeGUI()
        {
            GUILayout.Label("Text:");
            text = GUILayout.TextArea( text);
            GUILayout.Label("Answers:");
            for (int i = 0; i < answers.Count; i++)
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
                outputKnobs[i + 1].DisplayLayout();
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

        public void OnClick(int index)
        {
            if(outputPorts[index].connected())
                TaskManager.Instance.setNode(outputPorts[index]);
            else
                TaskManager.Instance.setNode(StartDialogueNode.dialogueStartNode);
        }

        public override int Traverse()
        {
            DialogueSystem.ShowDialogueNode(this);
            return -1;
        }
    }
}
