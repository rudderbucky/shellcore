using NodeEditorFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/Dialogue Node")]
    public class DialogueNode : Node
    {
        public override string GetName { get { return "DialogueNode"; } }
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

        public NodeEditorGUI.NodeEditorState state;
        public override void NodeGUI()
        {
            GUILayout.Label("Text:");
            text = GUILayout.TextArea( text);
            GUILayout.Label("Text Color:");
            float r, g, b;
            GUILayout.BeginHorizontal();
            r = RTEditorGUI.FloatField(textColor.r);
            g = RTEditorGUI.FloatField(textColor.g);
            b = RTEditorGUI.FloatField(textColor.b);
            GUILayout.EndHorizontal();
            textColor = new Color(r, g, b);

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
            DialogueSystem.OnDialogueEnd -= OnClick;
            if(outputPorts[index].connected())
                TaskManager.Instance.setNode(outputPorts[index]);
            else
            {
                TaskManager.Instance.setNode(state == NodeEditorGUI.NodeEditorState.Mission 
                    ? StartDialogueNode.missionCanvasNode : StartDialogueNode.dialogueCanvasNode);
            }

        }

        public override int Traverse()
        {
            if(state == NodeEditorGUI.NodeEditorState.Mission)
                DialogueSystem.ShowDialogueNode(this, TaskManager.GetSpeaker());
            else DialogueSystem.ShowDialogueNode(this, DialogueSystem.GetSpeaker());
            DialogueSystem.OnDialogueEnd += OnClick;
            return -1;
        }
    }
}
