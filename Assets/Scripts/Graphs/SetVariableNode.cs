using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "TaskSystem/SetVariable")]
    public class SetVariableNode : Node
    {
        //Node things
        public const string ID = "SetVariableNode";
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Set Variable"; } }
        public override Vector2 DefaultSize { get { return new Vector2(200, 140); } }

        public override bool ContinueCalculation { get { return true; } }

        //Data
        string variableName;
        int value;

        [ConnectionKnob("Input Left", Direction.In, "Task", NodeSide.Left, 20)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Right", Direction.Out, "Task", NodeSide.Right, 20)]
        public ConnectionKnob outputRight;

        public override void NodeGUI()
        {
            inputLeft.DisplayLayout();
            outputRight.DisplayLayout();
            GUILayout.Label("Variable Name:");
            variableName = GUILayout.TextField(variableName);
            GUILayout.Label("Value:");
            value = RTEditorGUI.IntField(value);
        }

        public override bool Calculate()
        {
            TaskManager.Instance.SetTaskVariable(variableName, value);
            return true;
        }
    }
}