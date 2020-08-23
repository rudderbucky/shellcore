using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Set Part Drop Rate")]
    public class SetPartDropRateNode : Node
    {
        public override string GetName { get { return "SetPartDropRate"; } }
        public override string Title { get { return "Set Part Drop Rate"; } }
        public override bool AllowRecursion { get { return true; } }
        public override bool AutoLayout { get { return true; } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;
        public string dropRate;
        private float oldDropRate;
        public string sectorName;
        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Drop Rate");
            dropRate = RTEditorGUI.TextField(dropRate, GUILayout.MinWidth(400));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Sector Name");
            sectorName = RTEditorGUI.TextField(sectorName, GUILayout.MinWidth(400));
            GUILayout.EndHorizontal();

        }

        public override int Traverse()
        {
            oldDropRate = Entity.partDropRate;
            Entity.partDropRate = float.Parse(dropRate);
            SectorManager.OnSectorLoad += RestoreOldValue; 
            return 0;
        }

        public void RestoreOldValue(string sectorName)
        {
            if(sectorName != this.sectorName) 
            {
                Debug.Log("Left part drop rate sector");
                Entity.partDropRate = oldDropRate;
                SectorManager.OnSectorLoad -= RestoreOldValue;
            }
        }
    }
}
