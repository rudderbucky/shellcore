using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Checkpoint")]
    public class CheckpointNode : Node
    {
        public static string LimitedSector;
        //public static SectorLimiterNode StartPoint;

        //Node things
        public const string ID = "CheckpointNode";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Checkpoint"; }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(208, 100); }
        }

        public string checkpointName;

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left, 20)]
        public ConnectionKnob input;

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right, 20)]
        public ConnectionKnob output;

        public override void NodeGUI()
        {
            GUILayout.Label("Checkpoint Name:");
            checkpointName = GUILayout.TextField(checkpointName, GUILayout.Width(200f));
        }

        public override int Traverse()
        {
            (Canvas.Traversal as Traverser).lastCheckpointName = checkpointName;
            TaskManager.Instance.AttemptAutoSave();
            return 0;
        }
    }
}
