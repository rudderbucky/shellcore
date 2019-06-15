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
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Set Variable"; } }
        public override Vector2 DefaultSize { get { return new Vector2(200, 150); } }

        public override bool ContinueCalculation { get { return true; } }

        //Data
        public string variableName;
        public int value;
        public bool action;

        ConnectionKnobAttribute flowIn = new ConnectionKnobAttribute("Input ", Direction.In, "TaskFlow", ConnectionCount.Multi, NodeSide.Left, 20);
        ConnectionKnobAttribute flowOut = new ConnectionKnobAttribute("Output ", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right, 20);
        ConnectionKnobAttribute actionIn = new ConnectionKnobAttribute("Input ", Direction.In, "Action", ConnectionCount.Multi, NodeSide.Left, 20);

        //[ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left, 20)]
        //public ConnectionKnob inputLeft;

        //[ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right, 20)]
        //public ConnectionKnob outputRight;

        public override void NodeGUI()
        {
            action = RTEditorGUI.Toggle(action, "Action");
            if(action && outputKnobs.Count == 1)
            {
                DeleteConnectionPort(outputKnobs[0]);
                DeleteConnectionPort(inputKnobs[0]);
                CreateConnectionPort(actionIn);
            }
            else if(!action && outputKnobs.Count == 0 && inputKnobs.Count == 1)
            {
                DeleteConnectionPort(inputKnobs[0]);
                CreateConnectionPort(flowIn);
                CreateConnectionPort(flowOut);
            }
            else if(inputKnobs.Count == 0)
            {
                if(action)
                {
                    CreateConnectionPort(actionIn);
                }
                else
                {
                    CreateConnectionPort(flowIn);
                    CreateConnectionPort(flowOut);
                }
            }

            if (action)
            {
                inputKnobs[0].DisplayLayout();
            }
            else
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
        }

        public override int Traverse()
        {
            TaskManager.Instance.SetTaskVariable(variableName, value);
            return 0;
        }
    }
}