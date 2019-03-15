using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "TaskSystem/StartTask")]
    public class StartTaskNode : Node
    {
        //Node things
        public const string ID = "StartTaskNode";
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Start Task"; } }
        public override Vector2 DefaultSize { get { return new Vector2(200, 100); } }

        public override bool AllowRecursion { get { return true; } }
        public override bool ContinueCalculation { get { return true; } }

        //Task related
        string TaskID;

        [ConnectionKnob("Input Left", Direction.Out, "Flow", NodeSide.Left, 20)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Right", Direction.In, "Flow", NodeSide.Right, 20)]
        public ConnectionKnob outputRight;

        public override void NodeGUI()
        {
            GUILayout.Label("Task ID:");
            TaskID = GUILayout.TextField(TaskID);
        }
    }
}