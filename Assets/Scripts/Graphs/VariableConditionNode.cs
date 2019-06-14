using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Variable", typeof(QuestCanvas))]
    public class VariableConditionNode : Node, ICondition
    {
        readonly string[] modes = new string[]
        {
            "EqualTo",
            "GreaterThan",
            "LesserThan"
        };

        public delegate void VariableChangedDelegate(string variable);
        public static VariableChangedDelegate OnUnitDestroyed;

        public const string ID = "VariableConditionNode";
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Variable condition"; } }
        public override Vector2 DefaultSize { get { return new Vector2(200, 256); } }

        private ConditionState state;
        public ConditionState State { get { return state; } set { state = value; } }

        //Data
        string variableName;
        int value;
        int mode;

        [ConnectionKnob("Output Right", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob outputRight;

        [ConnectionKnob("Input", Direction.In, "Task", NodeSide.Left, 32)]
        public ConnectionKnob inputLeft;

        public override void NodeGUI()
        {
            inputLeft.DisplayLayout();
            GUILayout.Label("Variable Name:");
            variableName = GUILayout.TextField(variableName);
            GUILayout.Label("Value:");
            value = RTEditorGUI.IntField(value);

            GUILayout.Label("Comparison mode:");
            mode = GUILayout.SelectionGrid(mode, modes, 1, GUILayout.Width(128f));
        }

        public void Init(int index)
        {
            OnUnitDestroyed += OnVariableChange;
        }

        public void DeInit()
        {
            OnUnitDestroyed -= OnVariableChange;
        }

        void OnVariableChange(string variable)
        {
            if (variableName == variable)
            {
                int i = TaskManager.Instance.GetTaskVariable(variableName);
                switch (mode)
                {
                    case 0: if (i == value) { outputRight.connection(0).body.Calculate(); } break;
                    case 1: if (i > value) { outputRight.connection(0).body.Calculate(); } break;
                    case 2: if (i < value) { outputRight.connection(0).body.Calculate(); } break;
                }
            }
        }
    }
}