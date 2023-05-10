using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Debug Node")]
    public class DebugNode : Node
    {
        public override string GetName
        {
            get { return "DebugNode"; }
        }

        public override string Title
        {
            get { return "Debug message"; }
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
        }

        public override int Traverse()
        {
            DevConsoleScript.Print(text);
            AudioManager.PlayClipByID("clip_healeffect");
            return 0;
        }
    }
}
