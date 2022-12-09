using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/Popup Node")]
    public class PopupNode : Node
    {
        public override string GetName
        {
            get { return "PopupNode"; }
        }

        public override string Title
        {
            get { return "Popup dialogue"; }
        }

        float height = 0f;

        public override Vector2 DefaultSize
        {
            get { return new Vector2(200, height); }
        }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public string text;
        public Color color = Color.white;

        public override void NodeGUI()
        {
            height = 150f;
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.Label("Text:");
            text = GUILayout.TextArea(text);
            height += GUI.skin.textArea.CalcHeight(new GUIContent(text), 200f);

            GUILayout.Label("Color:");
            float r, g, b;
            GUILayout.BeginHorizontal();
            r = RTEditorGUI.FloatField(color.r);
            if (color.r < 0 || color.r > 1)
            {
                r = RTEditorGUI.FloatField(color.r = 1);
                Debug.LogWarning("Can't register this numbers!");
            }
            g = RTEditorGUI.FloatField(color.g);
            if (color.g < 0 || color.g > 1)
            {
                g = RTEditorGUI.FloatField(color.g = 1);
                Debug.LogWarning("Can't register this numbers!");
            }
            b = RTEditorGUI.FloatField(color.b);
            if (color.b < 0 || color.b > 1)
            {
                b = RTEditorGUI.FloatField(color.b = 1);
                Debug.LogWarning("Can't register this numbers!");
            }
            GUILayout.EndHorizontal();
            color = new Color(r, g, b);
        }

        public override int Traverse()
        {
            // DialogueSystem.OnDialogueEnd += OnDialogueEnd;
            DialogueSystem.ShowPopup(text, color);
            return 0;
        }

        void OnDialogueEnd(int _)
        {
            // DialogueSystem.OnDialogueEnd -= OnDialogueEnd;
            TaskManager.Instance.setNode(output);
        }
    }
}
