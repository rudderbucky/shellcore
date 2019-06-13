using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(true, "TaskSsytem/PopupNode")]
    public abstract class PopupNode : Node
    {
        public override string GetID { get { return "PopupNode"; } }
        public override string Title { get { return "Popup dialogue"; } }

        public override Vector2 DefaultSize { get { return new Vector2(200, 100); } }

        [ConnectionKnob("Output", Direction.Out, "Task", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "Task", NodeSide.Left)]
        public ConnectionKnob input;

        public string text;
        public Color color;

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

        public override bool Calculate()
        {
            DialogueSystem.ShowPopup(text, color);
            return true;
        }
    }
}