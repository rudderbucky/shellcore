using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/Set Music")]
    public class MusicNode : Node
    {
        public override string GetName { get { return "MusicNode"; } }
        public override string Title { get { return "Set Music"; } }

        public override Vector2 DefaultSize { get { return new Vector2(200, 150); } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public string musicID;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.Label("Music ID:");
            musicID = GUILayout.TextArea(musicID);
        }

        public override int Traverse()
        {
            // TODO: background music
            return -1;
        }
    }
}