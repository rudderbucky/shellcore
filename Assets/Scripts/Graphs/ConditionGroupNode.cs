using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/ConditionGroup")]
    public class ConditionGroupNode : Node
    {
        [System.Serializable]
        public struct ConditionGroup
        {
            public ConnectionKnob output;
            public ConnectionKnob input;
        }

        [ConnectionKnob("Input Left", Direction.In, "Task", NodeSide.Left)]
        public ConnectionKnob input;

        List<ConditionGroup> groups = new List<ConditionGroup>();
        int groupCount = 0;

        //Node things
        public const string ID = "ConditionGroupNode";
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Conditions"; } }

        public override bool AllowRecursion { get { return true; } }
        public override bool AutoLayout { get { return true; } }

        public override bool Calculate()
        {
            for(int i = 0; i < groups.Count; i++)
            {
                if(groups[i].input.connected())
                {
                    int completed = 0;
                    int conditionCount = 0;
                    for(int j = 0; j < inputKnobs[i].connections.Count; j++)
                    {
                        if (!(inputKnobs[i].connections[j].body is ICondition))
                            continue;
                        ICondition condition = inputKnobs[i].connections[j].body as ICondition;
                        conditionCount++;
                        if (condition.State == ConditionState.Completed)
                        {
                            completed++;
                        }
                    }
                    if(completed == conditionCount)
                    {
                        // Continue to next node
                        if(groups[i].output.connected())
                            TaskManager.Instance.setNode(groups[i].output.connections[0].body);
                        // Tell all condition nodes to unsub
                        DeInit();
                        return true;
                    }
                }
            }
            return true;
        }

        void DeInit()
        {
            for (int i = 0; i < groups.Count; i++)
            {
                for (int j = 0; j < groups[i].input.connections.Count; j++)
                {
                    if (!(groups[i].input.connection(j).body is ICondition))
                        continue;
                    ICondition node = groups[i].input.connection(j).body as ICondition;
                    node.DeInit();
                }
            }
        }

        ConnectionKnobAttribute inputAttribute = new ConnectionKnobAttribute(" Input", Direction.In, "Condition", ConnectionCount.Multi, NodeSide.Left);
        ConnectionKnobAttribute outputAttribute = new ConnectionKnobAttribute("Output ", Direction.Out, "Task", ConnectionCount.Multi, NodeSide.Right);

        public override void NodeGUI()
        {
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
            }
            GUILayout.EndHorizontal();
        }
    }
}