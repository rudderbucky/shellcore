using NodeEditorFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/Roulette Node")]
    public class DialogueRouletteNode : Node
    {
        public override string GetName { get { return "DialogueRouletteNode"; } }
        public override string Title { get { return "Dialogue Roulette"; } }

        public override Vector2 MinSize { get { return new Vector2(200f, 100f); } }
        public override bool AutoLayout { get { return true; } }
        public override bool AllowRecursion { get { return true; } }

        [ConnectionKnob("Input Left", Direction.In, "Dialogue", NodeSide.Left)]
        public ConnectionKnob input;
        ConnectionKnobAttribute outputAttribute = new ConnectionKnobAttribute("Output ", Direction.Out, "Dialogue", ConnectionCount.Single, NodeSide.Right);
        public List<float> chances = new List<float>();

        public override void NodeGUI()
        {
            GUILayout.Label("Outputs and chances (floats should add to 1):");
            for (int i = 0; i < chances.Count; i++)
            {
                RTEditorGUI.Seperator();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                {
                    DeleteConnectionPort(outputPorts[i]);
                    chances.RemoveAt(i);
                    i--;
                    continue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                chances[i] = RTEditorGUI.FloatField(chances[i]);
                outputKnobs[i].DisplayLayout();
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false), GUILayout.MinWidth(100f)))
            {
                CreateConnectionKnob(outputAttribute);
                chances.Add(0);
            }
            GUILayout.EndHorizontal();
        }

        public override int Traverse()
        {
            var roll = Random.Range(0, 1f);
            for(int i = 0; i < chances.Count; i++)
            {
                roll -= chances[i];
                if(roll < 0)
                {
                    TaskManager.Instance.setNode(outputKnobs[i]);
                    return -1;
                }
            }
            Debug.LogWarning("Something went wrong with the roulette. Double check the entered floats!");
            return -1;
        }
    }
}
