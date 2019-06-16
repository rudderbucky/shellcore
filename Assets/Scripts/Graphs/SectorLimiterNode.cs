using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/SectorLimiter")]
    public class SectorLimiterNode : Node
    {
        public static string LimitedSector;
        public static Node StartPoint;

        //Node things
        public const string ID = "SectorLimiterNode";
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Sector Limiter"; } }
        public override Vector2 DefaultSize { get { return new Vector2(208, height); } }

        public string sectorName;
        public bool freeSector;

        float height = 40f;

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left, 20)]
        public ConnectionKnob input;

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right, 20)]
        public ConnectionKnob output;

        public override void NodeGUI()
        {
            height = 40f;
            freeSector = RTEditorGUI.Toggle(freeSector, "Free sector");
            if(!freeSector)
            {
                height = 84f;
                GUILayout.Label("Sector Name:");
                sectorName = GUILayout.TextField(sectorName, GUILayout.Width(200f));
            }
        }

        public override int Traverse()
        {
            LimitedSector = sectorName ?? "";

            if(sectorName == "" || freeSector)
            {
                SectorManager.OnSectorLoad = null;
                return 0;
            }
            else
            {
                //TODO: save task system variables
                SectorManager.OnSectorLoad = SectorUpdate;
                SectorUpdate(SectorManager.instance.current.sectorName);
                return -1;
            }
        }

        void SectorUpdate(string name)
        {
            if(name == sectorName)
            {
                TaskManager.Instance.setNode(output);
            }
            else
            {
                Node current = TaskManager.Instance.GetCurrentNode();
                if (current != this)
                {
                    if (current is TimelineNode)
                        TaskManager.Instance.StopAllCoroutines();
                    if (current is ConditionGroupNode)
                    {
                        var cgn = current as ConditionGroupNode;
                        cgn.DeInit();
                    }

                    // TODO: restore saved variables

                    TaskManager.Instance.setNode(this);
                }
            }
        }
    }
}