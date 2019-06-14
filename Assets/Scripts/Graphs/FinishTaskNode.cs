using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

//State: success, failure, ended?

namespace NodeEditorFramework.Standard
{
    [Node(false, "TaskSystem/FinishTaskNode")]
    public class FinishTaskNode : Node
    {
        //Node things
        public const string ID = "FinishTaskNode";
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Finish Task"; } }
        public override Vector2 DefaultSize { get { return new Vector2(208, height); } }

        //Task related
        public string rewardGiverID;
        public string rewardText;

        float height = 220f;

        [ConnectionKnob("Input Left", Direction.In, "Task", NodeSide.Left, 20)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Right", Direction.Out, "Task", NodeSide.Right, 20)]
        public ConnectionKnob outputRight;

        [ConnectionKnob("Output Up", Direction.Out, "Complete", ConnectionCount.Single, NodeSide.Top, 100f)]
        public ConnectionKnob outputUp;

        public override void NodeGUI()
        {
            GUILayout.Label("Reward giver ID:");
            rewardGiverID = GUILayout.TextField(rewardGiverID, GUILayout.Width(200f));
            GUILayout.Label("Reward text:");
            rewardText = GUILayout.TextField(rewardText, GUILayout.Width(200f));
        }

        public override int Traverse()
        {
            if (outputUp.connected())
            {
                var taskNode = (outputUp.connection(0).body as StartTaskNode);
                if(taskNode)
                {
                    string taskID = taskNode.taskID;
                    TaskManager.Instance.endTask(taskID);
                }
            }
            return 0;
        }
    }
}