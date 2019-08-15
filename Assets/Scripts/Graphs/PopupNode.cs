using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/PopupNode")]
    public class PopupNode : Node
    {
        public override string GetName { get { return "PopupNode"; } }
        public override string Title { get { return "Popup dialogue"; } }

        public override Vector2 DefaultSize { get { return new Vector2(200, 150); } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public string text;
        public Color color = Color.white;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.Label("Text:");
            text = GUILayout.TextArea(text);

            GUILayout.Label("Color:");
            float r, g, b;
            GUILayout.BeginHorizontal();
            r = RTEditorGUI.FloatField(color.r);
            g = RTEditorGUI.FloatField(color.g);
            b = RTEditorGUI.FloatField(color.b);
            GUILayout.EndHorizontal();
            color = new Color(r, g, b);
        }

        public override int Traverse()
        {
            DialogueSystem.OnDialogueEnd += OnDialogueEnd;
            DialogueSystem.ShowPopup(text, color);
            return -1;
        }

        void OnDialogueEnd(int _)
        {
            DialogueSystem.OnDialogueEnd -= OnDialogueEnd;
            TaskManager.Instance.setNode(output);
        }
    }
}