using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/TimelineNode")]
    public class TimelineNode : Node
    {
        public const string ID = "TimelineNode";
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Timeline"; } }

        public override bool AutoLayout { get { return true; } }
        public override Vector2 MinSize { get { return new Vector2(180f, 64f); } }

        public List<int> times = new List<int>();

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left, 20)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right, 20)]
        public ConnectionKnob outputRight;

        ConnectionKnobAttribute outputAttribute = new ConnectionKnobAttribute("Output ", Direction.Out, "Action", ConnectionCount.Single, NodeSide.Right);

        public override void NodeGUI()
        {
            GUILayout.BeginVertical(GUILayout.MinWidth(300f));
            for (int i = 0; i < times.Count; i++)
            {
                RTEditorGUI.Seperator();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                {
                    DeleteConnectionPort(outputKnobs[i + 1]);
                    times.RemoveAt(i);
                    i--;
                    continue;
                }
                GUILayout.Label("Event " + i);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                times[i] = RTEditorGUI.IntField("Time:", times[i]);
                outputKnobs[i + 1].DisplayLayout();
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false), GUILayout.MinWidth(100f)))
            {
                CreateConnectionKnob(outputAttribute);
                times.Add(times.Count);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        public override int Traverse()
        {
            TaskManager.Instance.StartCoroutine(timer());
            return -1;
        }

        IEnumerator timer()
        {
            int elapsed = 0;
            for (int i = 0; i < times.Count; i++)
            {
                yield return new WaitForSeconds(times[i] - elapsed);
                elapsed += times[i];
                if(outputPorts[i + 1].connected())
                {
                    for (int j = 0; j < outputPorts[i+1].connections.Count; j++)
                    {
                        outputPorts[i + 1].connections[j].body.Traverse();
                    }
                }
            }
            TaskManager.Instance.setNode(outputRight);
        }
    }
}