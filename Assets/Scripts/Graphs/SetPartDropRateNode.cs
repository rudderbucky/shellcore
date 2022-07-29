using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Set Part Drop Rate")]
    public class SetPartDropRateNode : Node
    {
        public override string GetName
        {
            get { return "SetPartDropRate"; }
        }

        public override string Title
        {
            get { return "Set Part Drop Rate"; }
        }

        public override bool AllowRecursion
        {
            get { return true; }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public float dropRate;
        public string sectorName;
        public bool restoreOld;
        public static SectorManager.SectorLoadDelegate del;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (!(restoreOld = GUILayout.Toggle(restoreOld, "Restore old drop rate")))
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Drop Rate");
                dropRate = RTEditorGUI.FloatField(dropRate, GUILayout.MinWidth(400));
                if (dropRate < 0)
                {
                    dropRate = RTEditorGUI.FloatField(0, GUILayout.MinWidth(400));
                    Debug.LogWarning("Can't register negative numbers!");
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Sector Name");
                sectorName = RTEditorGUI.TextField(sectorName, GUILayout.MinWidth(400));
            }

            GUILayout.EndHorizontal();
        }

        public override int Traverse()
        {
            if (!restoreOld)
            {
                if (del != null)
                {
                    SectorManager.OnSectorLoad -= del;
                    del = null;
                }

                Entity.partDropRate = dropRate;
                del = RestoreOldValue;
                SectorManager.OnSectorLoad += del;
            }
            else if (del != null)
            {
                SectorManager.OnSectorLoad -= del;
                del = null;
                Entity.partDropRate = Entity.DefaultPartRate;
            }

            return 0;
        }

        public void RestoreOldValue(string sectorName)
        {
            if (sectorName != this.sectorName)
            {
                Debug.Log("Left part drop rate sector");
                Entity.partDropRate = Entity.DefaultPartRate;
            }
            else
            {
                Debug.Log("Entering part drop rate sector");
                Entity.partDropRate = dropRate;
            }
        }
    }
}
