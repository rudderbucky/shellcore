using NodeEditorFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Enter Sector", typeof(SectorCanvas))]
    public class LoadSectorNode : Node
    {
        public static StartDialogueNode missionCanvasNode = null;
        public static StartDialogueNode dialogueCanvasNode = null;

        public override string GetName { get { return "LoadSectorNode"; } }
        public override string Title { get { return "Enter Sector"; } }

        public override bool AutoLayout { get { return true; } }
        public override bool AllowRecursion { get { return true; } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right)]
        public ConnectionKnob output;

        public string sectorName;

        public override void NodeGUI()
        {
            GUILayout.Label("Sector Name: ");
            sectorName = GUILayout.TextField(sectorName);
        }

        public override int Traverse()
        {
            return 0;
        }
    }
}
