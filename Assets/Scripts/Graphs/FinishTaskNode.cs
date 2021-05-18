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
        public string rewardGiverID;
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
            GUILayout.Label("Reward giver ID:");
            rewardGiverID = GUILayout.TextField(rewardGiverID, GUILayout.Width(200f));
            if (WorldCreatorCursor.instance != null)
            {
                if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                {
                    WorldCreatorCursor.selectEntity += SetEntityID;
                    WorldCreatorCursor.instance.EntitySelection();
                }
            }
            GUILayout.Label("Reward text:");
            rewardText = GUILayout.TextArea(rewardText, GUILayout.ExpandHeight(false), GUILayout.Width(200f));
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

        void SetEntityID(string ID)
        {
            Debug.Log("selected ID " + ID + "!");

            rewardGiverID = ID;
            WorldCreatorCursor.selectEntity -= SetEntityID;
        }

        public void OnDialogue()
        {
            // draw objectives
            TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Remove(objectiveLocation);
            TaskManager.DrawObjectiveLocations();

            DialogueSystem.ShowPopup(rewardText, textColor, SectorManager.instance.GetEntity(rewardGiverID));
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
                    SectorManager.instance.player.AddCredits(taskNode.creditReward);
                    SectorManager.instance.player.reputation += taskNode.reputationReward;
                    if(taskNode.partReward)
                    {
                        SectorManager.instance.player.cursave.partInventory.Add(
                            new EntityBlueprint.PartInfo
                            {
                                partID = taskNode.partID,
                                abilityID = taskNode.partAbilityID,
                                tier = taskNode.partTier,
                                secondaryData = taskNode.partSecondaryData
                            });
                    }
                }
            }
        }

        public override int Traverse()
        {
            // Pop the pending text
            if (outputUp.connected())
            {
                var taskNode = (outputUp.connection(0).body as StartTaskNode);
                if (taskNode && taskNode.entityIDforConfirmedResponse != null && taskNode.entityIDforConfirmedResponse != "")
                {
                    if (TaskManager.interactionOverrides.ContainsKey(taskNode.entityIDforConfirmedResponse))
                    {
                        TaskManager.interactionOverrides[taskNode.entityIDforConfirmedResponse].Pop();
                    }
                    else
                    {
                        Debug.LogWarning(taskNode.entityIDforConfirmedResponse + " missing from interaction override dictionary!");
                    }
                }

                var mission = PlayerCore.Instance.cursave.missions.Find((x) => x.name == (Canvas as QuestCanvas).missionName);
            }

            SectorManager.instance.player.alerter.showMessage("TASK COMPLETE", "clip_victory");


            TaskManager.speakerIDList.Add(rewardGiverID);
            if (TaskManager.interactionOverrides.ContainsKey(rewardGiverID))
            {
                Debug.Log("Contains key");
                TaskManager.interactionOverrides[rewardGiverID].Push(() => {
                    TaskManager.interactionOverrides[rewardGiverID].Pop();
                    OnDialogue();
                });
            }
            else
            {
                var stack = new Stack<UnityEngine.Events.UnityAction>();
                stack.Push(() => {
                        TaskManager.interactionOverrides[rewardGiverID].Pop();
                        OnDialogue();
                    });
                TaskManager.interactionOverrides.Add(rewardGiverID, stack);
                Debug.Log("ADDED " + rewardGiverID);
            }
            TryAddObjective();

            return -1;
        }

        TaskManager.ObjectiveLocation objectiveLocation;
        void TryAddObjective()
		{
            foreach(var ent in AIData.entities)
            {
                if(!ent) continue;
                if(ent.ID == rewardGiverID)
                {
                    TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Clear();
                    objectiveLocation = new TaskManager.ObjectiveLocation(
                        ent.transform.position,
                        true,
                        (Canvas as QuestCanvas).missionName,
                        ent
                    );
                    TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Add(objectiveLocation);
                    TaskManager.DrawObjectiveLocations();
                    break;
                }
            }
		}
    }
}