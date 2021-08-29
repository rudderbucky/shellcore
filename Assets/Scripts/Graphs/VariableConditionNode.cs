using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Variable", typeof(QuestCanvas), typeof(SectorCanvas))]
    public class VariableConditionNode : Node, ICondition
    {
        readonly string[] modes = new string[]
        {
            "EqualTo",
            "GreaterThan",
            "LesserThan"
        };

        public delegate void VariableChangedDelegate(string variable);

        public static VariableChangedDelegate OnVariableUpdate;

        public const string ID = "VariableConditionNode";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Variable condition"; }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(200, 200); }
        }

        private ConditionState state;

        public ConditionState State
        {
            get { return state; }
            set { state = value; }
        }

        //Data
        public string variableName;
        public int value;
        public int mode;

        [ConnectionKnob("Output Right", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob outputRight;

        public override void NodeGUI()
        {
            GUILayout.Label("Variable Name:");
            variableName = GUILayout.TextField(variableName);
            GUILayout.Label("Value:");
            value = RTEditorGUI.IntField(value);

            GUILayout.Label("Comparison mode:");
            mode = GUILayout.SelectionGrid(mode, modes, 1, GUILayout.Width(128f));
        }

        public void Init(int index)
        {
            OnVariableUpdate += VariableUpdate;
            int i = TaskManager.Instance.GetTaskVariable(variableName);
            state = ConditionState.Listening;
            switch (mode)
            {
                case 0:
                    if (i == value)
                    {
                        state = ConditionState.Completed;
                    }

                    break;
                case 1:
                    if (i > value)
                    {
                        state = ConditionState.Completed;
                    }

                    break;
                case 2:
                    if (i < value)
                    {
                        state = ConditionState.Completed;
                    }

                    break;
            }
        }

        public void DeInit()
        {
            state = ConditionState.Uninitialized;
            OnVariableUpdate -= VariableUpdate;
        }

        void VariableUpdate(string variable)
        {
            if (variableName == variable)
            {
                int i = TaskManager.Instance.GetTaskVariable(variableName);
                switch (mode)
                {
                    case 0:
                        if (i == value)
                        {
                            state = ConditionState.Completed;
                            outputRight.connection(0).body.Calculate();
                        }

                        break;
                    case 1:
                        if (i > value)
                        {
                            state = ConditionState.Completed;
                            outputRight.connection(0).body.Calculate();
                        }

                        break;
                    case 2:
                        if (i < value)
                        {
                            state = ConditionState.Completed;
                            outputRight.connection(0).body.Calculate();
                        }

                        break;
                }
            }
        }
    }
}
