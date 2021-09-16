using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/ConditionGroup")]
    public class ConditionGroupNode : Node
    {
        [System.Serializable]
        public struct ConditionGroup
        {
            public ConnectionKnob output;
            public ConnectionKnob input;
        }

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        List<ConditionGroup> groups = new List<ConditionGroup>();
        int groupCount = 0;

        //Node things
        public const string ID = "ConditionGroupNode";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Conditions"; }
        }

        public override bool AllowRecursion
        {
            get { return true; }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        private bool allConditionsPassed = false;

        public override int Traverse()
        {
            // Importing doesn't fill group data. Do it here for now. TODO: Fix
            allConditionsPassed = false;
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
                if (groups[i].input.connected())
                {
                    var connections = groups[i].input.connections;
                    for (int j = 0; j < connections.Count; j++)
                    {
                        if (connections[j].body is ICondition condition)
                        {
                            condition.Init(0);

                            // CheckEntityCondition needs to check on init if an entity exists to prevent repeated update polling
                            // so prevent continuous initialization of nodes if all conditions have passed
                            if (allConditionsPassed) return -1;
                        }
                    }
                }
            }

            Calculate();
            return -1;
        }

        public override bool Calculate()
        {
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].input.connected())
                {
                    int completed = 0;
                    int conditionCount = 0;
                    for (int j = 0; j < groups[i].input.connections.Count; j++)
                    {
                        if (groups[i].input.connections[j].body is ICondition condition)
                        {
                            conditionCount++;
                            if (condition.State == ConditionState.Completed)
                            {
                                completed++;
                            }
                        }
                    }

                    if (completed == conditionCount)
                    {
                        Debug.Log("All conditions passed!");
                        allConditionsPassed = true;
                        // Tell all condition nodes to unsub
                        DeInit();
                        // Continue to next node
                        if (groups[i].output.connected())
                        {
                            TaskManager.Instance.setNode(groups[i].output);
                        }

                        return true;
                    }
                }
            }

            return true;
        }

        public void DeInit()
        {
            for (int i = 0; i < groups.Count; i++)
            {
                for (int j = 0; j < groups[i].input.connections.Count; j++)
                {
                    if (groups[i].input.connection(j).body is ICondition node)
                    {
                        node.DeInit();
                    }
                }
            }
        }

        ConnectionKnobAttribute inputAttribute = new ConnectionKnobAttribute("Input", Direction.In, "Condition", ConnectionCount.Multi, NodeSide.Left);
        ConnectionKnobAttribute outputAttribute = new ConnectionKnobAttribute("Output ", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right);

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
                {
                    // Remove current label
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
        }
    }
}
