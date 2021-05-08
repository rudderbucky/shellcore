using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Actions/SetVariable")]
    public class SetVariableNode : Node
    {
        //Node things
        public const string ID = "SetVariableNode";
        public override string GetName { get { return ID; } }

        public override string Title { get { return "Set Variable"; } }
        public override Vector2 DefaultSize { get { return new Vector2(200, 150); } }

        public override bool ContinueCalculation { get { return true; } }

        //Data
        public string variableName;
        public int value;
        bool incrementMode;

        ConnectionKnobAttribute flowIn = new ConnectionKnobAttribute("Input ", Direction.In, "TaskFlow", ConnectionCount.Multi, NodeSide.Left, 20);
        ConnectionKnobAttribute flowOut = new ConnectionKnobAttribute("Output ", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right, 20);

        ConnectionKnobAttribute dialogueIn = new ConnectionKnobAttribute("Input ", Direction.In, "Dialogue", ConnectionCount.Multi, NodeSide.Left, 20);
        ConnectionKnobAttribute dialogueOut = new ConnectionKnobAttribute("Output ", Direction.Out, "Dialogue", ConnectionCount.Single, NodeSide.Right, 20);

        ConnectionKnob input;
        ConnectionKnob output;

        //[ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left, 20)]
        //public ConnectionKnob inputLeft;

        //[ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right, 20)]
        //public ConnectionKnob outputRight;

        public override void NodeGUI()
        {
            if(input == null)
            {
                if (inputKnobs.Count > 0)
                {
                    input = inputKnobs[0];
                    output = outputKnobs[0];
                }
                else if (Canvas is DialogueCanvas)
                {
                    input = CreateConnectionKnob(dialogueIn);
                    output = CreateConnectionKnob(dialogueOut);
                }
                else
                {
                    input = CreateConnectionKnob(flowIn);
                    output = CreateConnectionKnob(flowOut);
                }
            }

            {
                GUILayout.BeginHorizontal();
                inputKnobs[0].DisplayLayout();
                outputKnobs[0].DisplayLayout();
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("Variable Name:");
            variableName = GUILayout.TextField(variableName);
            GUILayout.Label("Value:");
            value = RTEditorGUI.IntField(value);
            incrementMode = GUILayout.Toggle(incrementMode,"Increment mode: ");
        }

        public override int Traverse()
        {
            TaskManager.Instance.SetTaskVariable(variableName, value, incrementMode);
            return 0;
        }
    }
}