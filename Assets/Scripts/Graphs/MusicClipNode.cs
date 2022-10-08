using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Actions/Set Audio")]
    public class MusicClipNode : Node
    {
        public override string GetName
        {
            get { return "MusicClipNode"; }
        }

        public override string Title
        {
            get { return "Set Audio"; }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(200, 150f); }
        }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public string audioID = "";

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.Label("Audio ID: ");
            audioID = Utilities.RTEditorGUI.TextField(audioID);
        }

        public override int Traverse()
        {
            AudioManager.PlayClipByID(audioID);
            return 0;
        }
    }
}