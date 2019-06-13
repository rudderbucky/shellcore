using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "TaskSystem/TimelineNode")]
    public class TimelineNode : Node
    {
        public const string ID = "TimelineNode";
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Timeline"; } }
        public override Vector2 DefaultSize { get { return new Vector2(200, height); } }

        float height = 100f;

        List<int> times = new List<int>();

        [ConnectionKnob("Input Left", Direction.In, "Task", NodeSide.Left, 20)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Right", Direction.Out, "Task", NodeSide.Right, 20)]
        public ConnectionKnob outputRight;

        ConnectionKnobAttribute outputAttribute = new ConnectionKnobAttribute("Output ", Direction.Out, "Task", ConnectionCount.Multi, NodeSide.Right);

        public override void NodeGUI()
        {
            for (int i = 0; i < times.Count; i++)
            {
                RTEditorGUI.Seperator();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                {
                    DeleteConnectionPort(outputKnobs[i]);
                    times.RemoveAt(i);
                    i--;
                    continue;
                }
                GUILayout.Label("Group " + i);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                times[i] = RTEditorGUI.IntField("Time:", times[i]);
                outputKnobs[i].DisplayLayout();
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false), GUILayout.MinWidth(100f)))
            {
                CreateConnectionKnob(outputAttribute);
                times.Add(times[times.Count - 1]);
            }
            GUILayout.EndHorizontal();
        }

        public override bool Calculate()
        {
            TaskManager.Instance.StartCoroutine(timer());

            return true;
        }

        IEnumerator timer()
        {
            int elapsed = 0;
            for (int i = 0; i < times.Count; i++)
            {
                yield return new WaitForSeconds(times[i] - elapsed);
                elapsed += times[i];
                TaskManager.Instance.setNode(outputPorts[i].connection(i).body); 
                //TODO: deactivate self (abstract function?)
            }
        }
    }
}