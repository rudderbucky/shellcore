using System.Collections;
using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Actions/Set Sector Type", typeof(SectorCanvas))]
    public class SectorTypeNode : Node
    {
        public override string GetName { get { return "SectorTypeNode"; } }
        public override string Title { get { return "Set Sector Type"; } }

        public override Vector2 DefaultSize { get { return new Vector2(200, 240); } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public int sectorType = 0;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();

            string[] types = System.Enum.GetNames(typeof(Sector.SectorType));

            GUILayout.Label("Sector Type:");
            sectorType = GUILayout.SelectionGrid(sectorType, types, 1, GUILayout.Width(144f));
        }

        public override int Traverse()
        {
            SectorManager.instance.overrideProperties.type = (Sector.SectorType)sectorType;

            string[] types = System.Enum.GetNames(typeof(Sector.SectorType));
            Debug.Log("Sector type set to: " + types[sectorType] + " (" + sectorType + ")");

            return 0;
        }
    }
}
