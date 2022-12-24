using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Tasks/Fail Task Node", typeof(QuestCanvas))]
    public class FailTaskNode : Node
    {
        //Node things
        public const string ID = "FailTaskNode";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Fail Task"; }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(208, height); }
        }

        float height = 0f;

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left, 20)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right, 20)]
        public ConnectionKnob outputRight;

        [ConnectionKnob("Output Up", Direction.Out, "Complete", ConnectionCount.Single, NodeSide.Top, 100f)]
        public ConnectionKnob outputUp;

        public override void NodeGUI()
        {
            height = 50f;
            GUILayout.BeginHorizontal();
            GUILayout.Label("");
            GUILayout.EndHorizontal();
        }

        public override int Traverse()
        {
            // Pop the pending text
            if (outputUp.connected())
            {
                if (outputUp.connection(0).body is StartTaskNode taskNode && !string.IsNullOrEmpty(taskNode.entityIDforConfirmedResponse))
                {
                    if (TaskManager.interactionOverrides.ContainsKey(taskNode.entityIDforConfirmedResponse))
                    {
                        var stack = TaskManager.interactionOverrides[taskNode.entityIDforConfirmedResponse];
                        if(stack.Count > 0)
                        {
                            TaskManager.interactionOverrides[taskNode.entityIDforConfirmedResponse].Pop();
                        }
                    }
                }
            }

            SectorManager.instance.player.alerter.showMessage("TASK FAILED", "clip_fail");
            if (outputUp.connected())
            {
                if (outputUp.connection(0).body is StartTaskNode taskNode)
                {
                    taskNode.forceTask = false; // you shouldn't force tasks you can fail
                    string taskID = taskNode.taskID;
                    TaskManager.Instance.endTask(taskID);
                    (Canvas.Traversal as MissionTraverser).taskHash++;
                }
            }

            return 0;
        }
    }
}
