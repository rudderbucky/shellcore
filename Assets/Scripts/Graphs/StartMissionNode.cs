using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Start Mission")]
    public class StartMissionNode : Node
    {
        //Node things
        public const string ID = "StartMissionNode";
        public override string GetName { get { return ID; } }

        public override string Title { get { return "Start Mission"; } }
        public override Vector2 DefaultSize { get { return new Vector2(128, 64); } }

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputRight;

        public string missionName;
        public override void NodeGUI()
        {
            /*

            while (outputKnobs.Count > groupCount)
            {
                groups.Add(new ConditionGroup
                {
                    output = outputKnobs[groupCount],
                    input = inputKnobs[groupCount + 1]
                });
                groupCount++;
            }

            for (int i = 0; i < groups.Count; i++)
            {
                RTEditorGUI.Seperator();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                { // Remove current label
                    DeleteConnectionPort(groups[i].input);
                    DeleteConnectionPort(groups[i].output);
                    groups.RemoveAt(i);
                    i--;
                    GUILayout.EndHorizontal();
                    continue;
                }
                GUILayout.Label("Group " + i);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                groups[i].input.DisplayLayout();
                groups[i].output.DisplayLayout();
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false), GUILayout.MinWidth(100f)))
            {
                ConditionGroup group = new ConditionGroup
                {
                    input = CreateConnectionKnob(inputAttribute),
                    output = CreateConnectionKnob(outputAttribute)
                };
                groups.Add(group);
                groupCount++;
            }
            GUILayout.EndHorizontal();
            */
        }
    }
}