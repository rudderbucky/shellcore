using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "TaskSystem/TestVariable")]
    public class TestVariableNode : Node
    {
        readonly string[] modes = new string[]
        {
            "EqualTo",
            "GreaterThan",
            "LesserThan"
        };

        //Node things
        public const string ID = "TestVariableNode";
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Test Variable"; } }
        public override Vector2 DefaultSize { get { return new Vector2(200, 256); } }

        public override bool AllowRecursion { get { return true; } }
        public override bool ContinueCalculation { get { return true; } }

        //Data
        string variableName;
        int value;
        int mode;

        [ConnectionKnob("Input", Direction.In, "Flow", NodeSide.Left, 32)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Comparison true", Direction.Out, "Flow", NodeSide.Right, 32)]
        public ConnectionKnob outputTrue;
        [ConnectionKnob("Comparison false", Direction.Out, "Flow", NodeSide.Right, 48)]
        public ConnectionKnob outputFalse;


        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            inputLeft.DisplayLayout();
            GUILayout.BeginVertical();
            outputTrue.DisplayLayout();
            outputFalse.DisplayLayout();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Label("Variable Name:");
            variableName = GUILayout.TextField(variableName);
            GUILayout.Label("Value:");
            value = RTEditorGUI.IntField(value);

            GUILayout.Label("Comparison mode:");
            mode = GUILayout.SelectionGrid(mode, modes, 1, GUILayout.Width(128f));
        }
    }
}