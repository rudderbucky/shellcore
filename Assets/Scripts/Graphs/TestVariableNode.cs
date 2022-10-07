using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Test Variable", typeof(QuestCanvas), typeof(SectorCanvas))]
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

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Test Variable (DEPRECATED)"; }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(200, 300); }
        }

        public override bool AllowRecursion
        {
            get { return true; }
        }

        //Data
        public string variableName;
        public int value;
        public int mode;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left, 32)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Comparison true", Direction.Out, "TaskFlow", NodeSide.Right, 32)]
        public ConnectionKnob outputTrue;

        [ConnectionKnob("Comparison false", Direction.Out, "TaskFlow", NodeSide.Right, 48)]
        public ConnectionKnob outputFalse;

        public override void NodeGUI()
        {
            GUILayout.Label("This node is deprecated. Use 'Flow/Condition Check Node' instead");

            GUILayout.Space(20);

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
            mode = GUILayout.SelectionGrid(mode, modes, 1, GUILayout.Width(144f));
        }

        public override int Traverse()
        {
            int i = TaskManager.Instance.GetTaskVariable(variableName);
            switch (mode)
            {
                case 0: return (i == value) ? 0 : 1;
                case 1: return (i > value) ? 0 : 1;
                case 2: return (i < value) ? 0 : 1;
                default:
                    return 0;
            }
        }
    }
}
