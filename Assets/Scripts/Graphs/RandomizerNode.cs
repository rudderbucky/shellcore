using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Randomizer")]
    public class RandomizerNode : Node
    {
        // Old name kept for backwards compatibility
        public override string GetName
        {
            get { return "DialogueRouletteNode"; }
        }

        public override string Title
        {
            get { return "Randomizer"; }
        }

        public override Vector2 MinSize
        {
            get { return new Vector2(200f, 100f); }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        public override bool AllowRecursion
        {
            get { return true; }
        }

        ConnectionKnobAttribute flowInAttribute = new ConnectionKnobAttribute("Input ", Direction.In, "TaskFlow", ConnectionCount.Multi, NodeSide.Left, 20);
        ConnectionKnobAttribute flowOutAttribute = new ConnectionKnobAttribute("Output ", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right);

        ConnectionKnobAttribute dialogueInAttribute = new ConnectionKnobAttribute("Input ", Direction.In, "Dialogue", ConnectionCount.Multi, NodeSide.Left, 20);
        ConnectionKnobAttribute dialogueOutAttribute = new ConnectionKnobAttribute("Output ", Direction.Out, "Dialogue", ConnectionCount.Single, NodeSide.Right);

        public ConnectionKnob input;
        public List<float> chances = new List<float>();
        public bool dialogue = false;

        public static bool PrintRandomRolls = false;

        public override void NodeGUI()
        {
            if (!(Canvas is DialogueCanvas))
            {
                bool newValue = GUILayout.Toggle(dialogue, "Dialogue mode");
                if (dialogue != newValue)
                {
                    dialogue = newValue;

                    int count = outputKnobs.Count;
                    while (outputKnobs.Count > 0)
                    {
                        DeleteConnectionPort(outputKnobs[0]);
                    }

                    for (int i = 0; i < count; i++)
                    {
                        CreateConnectionKnob(dialogue ? dialogueOutAttribute : flowOutAttribute);
                    }

                    DeleteConnectionPort(inputKnobs[0]);
                    CreateConnectionKnob(dialogue ? dialogueInAttribute : flowInAttribute);
                }
            }

            if (input == null)
            {
                if (inputKnobs.Count > 0)
                {
                    input = inputKnobs[0];
                }
                else if (Canvas is DialogueCanvas || dialogue)
                {
                    input = CreateConnectionKnob(dialogueInAttribute);
                }
                else
                {
                    input = CreateConnectionKnob(flowInAttribute);
                }
            }

            GUILayout.Label("Outputs and chance weights:");
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
                CreateConnectionKnob((dialogue || (Canvas is DialogueCanvas)) ? dialogueOutAttribute : flowOutAttribute);
                chances.Add(0);
            }

            GUILayout.EndHorizontal();
        }

        public override int Traverse()
        {
            float total = 0f;
            for (int i = 0; i < chances.Count; i++)
            {
                total += chances[i];
            }

            float roll = Random.Range(0, total);
            float originalRoll = roll;
            for (int i = 0; i < chances.Count; i++)
            {
                roll -= chances[i];
                if (roll <= 0)
                {
                    TaskManager.Instance.setNode(outputKnobs[i]);
                    if (PrintRandomRolls)
                    {
                        DevConsoleScript.Print("Total weight: " + total + ", Random roll: " + originalRoll + ", Connection index: " + i);
                    }

                    return -1;
                }
            }

            // This might be possible due to floating point inaccuracies
            TaskManager.Instance.setNode(outputKnobs[chances.Count - 1]);
            //Debug.LogWarning("Something went wrong with the roulette. Double check the entered floats!");
            return -1;
        }
    }
}
