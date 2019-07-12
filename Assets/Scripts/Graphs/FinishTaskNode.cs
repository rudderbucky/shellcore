using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Tasks/FinishTaskNode")]
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

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left, 20)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right, 20)]
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

        public void OnDialogue()
        {
            DialogueSystem.ShowPopup(rewardText);
            DialogueSystem.OnDialogueEnd = (int _) =>
            {
                TaskManager.Instance.setNode(outputRight);
                DialogueSystem.OnDialogueEnd = null;
            };
            if (outputUp.connected())
            {
                var taskNode = (outputUp.connection(0).body as StartTaskNode);
                if (taskNode)
                {
                    string taskID = taskNode.taskID;
                    TaskManager.Instance.endTask(taskID);
                    Debug.Log("Task complete!");
                    SectorManager.instance.player.credits += taskNode.creditReward; //Find a better way to get the player?
                }
            }
        }

        public override int Traverse()
        {
            if (TaskManager.interactionOverrides.ContainsKey(rewardGiverID))
            {
                TaskManager.interactionOverrides[rewardGiverID] = () => {
                    OnDialogue();
                    TaskManager.interactionOverrides.Remove(rewardGiverID);
                };
            }
            else
            {
                TaskManager.interactionOverrides.Add(rewardGiverID, () => {
                    OnDialogue();
                    TaskManager.interactionOverrides.Remove(rewardGiverID);
                });
            }
            return -1;
        }
    }
}