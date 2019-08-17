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
        public override string GetName { get { return ID; } }

        public override string Title { get { return "Finish Task"; } }
        public override Vector2 DefaultSize { get { return new Vector2(208, height); } }

        //Task related
        public string rewardGiverName;
        public string rewardText;
        public Color textColor = Color.white;

        float height = 0f;

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left, 20)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right, 20)]
        public ConnectionKnob outputRight;

        [ConnectionKnob("Output Up", Direction.Out, "Complete", ConnectionCount.Single, NodeSide.Top, 100f)]
        public ConnectionKnob outputUp;

        public override void NodeGUI()
        {
            height = 180f;
            GUILayout.Label("Reward giver name:");
            rewardGiverName = GUILayout.TextField(rewardGiverName, GUILayout.Width(200f));
            GUILayout.Label("Reward text:");
            rewardText = GUILayout.TextArea(rewardText, GUILayout.Width(200f));
            height += GUI.skin.textArea.CalcHeight(new GUIContent(rewardText), 200f);
            GUILayout.Label("Text Color:");
            float r, g, b;
            GUILayout.BeginHorizontal();
            r = RTEditorGUI.FloatField(textColor.r);
            g = RTEditorGUI.FloatField(textColor.g);
            b = RTEditorGUI.FloatField(textColor.b);
            GUILayout.EndHorizontal();
            textColor = new Color(r, g, b);
        }

        public void OnDialogue()
        {
            DialogueSystem.ShowPopup(rewardText, textColor, TaskManager.GetSpeaker());
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
                    SectorManager.instance.player.credits += taskNode.creditReward;
                    SectorManager.instance.player.reputation += taskNode.reputationReward;
                    if(taskNode.partReward)
                    {
                        SectorManager.instance.player.cursave.partInventory.Add(
                            new EntityBlueprint.PartInfo
                            {
                                partID = taskNode.partID,
                                abilityID = taskNode.partAbilityID,
                                tier = taskNode.partTier
                            });
                    }
                }
            }
        }

        public override int Traverse()
        {
            SectorManager.instance.player.alerter.showMessage("TASK COMPLETE", "clip_victory");
            if (TaskManager.interactionOverrides.ContainsKey(rewardGiverName))
            {
                TaskManager.interactionOverrides[rewardGiverName] = () => {
                    OnDialogue();
                    TaskManager.interactionOverrides.Remove(rewardGiverName);
                };
            }
            else
            {
                TaskManager.interactionOverrides.Add(rewardGiverName, () => {
                    OnDialogue();
                    TaskManager.interactionOverrides.Remove(rewardGiverName);
                });
            }
            return -1;
        }
    }
}