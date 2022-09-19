using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEngine;

[System.Serializable]
public struct RewardWrapper
{
    public int creditReward;
    public int reputationReward;
    public int shardReward;
    public bool partReward;
    public string partID;
    public int partAbilityID;
    public string partSecondaryData;
    public int partTier;
}

namespace NodeEditorFramework.Standard
{
    [Node(false, "Tasks/Finish Task Node", typeof(QuestCanvas))]
    public class FinishTaskNode : Node
    {
        //Node things
        public const string ID = "FinishTaskNode";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Finish Task"; }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        public override Vector2 MinSize
        {
            get { return new Vector2(208, 50); }
        }

        //Task related
        public string rewardGiverID;
        public string rewardText;
        public Color textColor = Color.white;
        public List<string> answers;

        float height = 0f;

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left, 20)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputRight;

        [ConnectionKnob("Output Up", Direction.Out, "Complete", ConnectionCount.Single, NodeSide.Top, 104F)]
        public ConnectionKnob outputUp;

        public bool useEntityColor = true;
        public bool speakToEntity = true;
        ConnectionKnobAttribute outputAttribute = new ConnectionKnobAttribute("Output ", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right);

        public override void NodeGUI()
        {
            /*
            if(answers == null)
            {
                answers = new List<string>();
                answers.Add("Ok");
            }
            
            if(outputRight && !outputRight.connected())
            {
                Destroy(outputRight);
            }
            */

            height = 180f;
            if ((speakToEntity = GUILayout.Toggle(speakToEntity, "Speak to entity")))
            {
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
                if (!(useEntityColor = GUILayout.Toggle(useEntityColor, "Use entity color")))
                {
                    GUILayout.Label("Text Color:");
                    float r, g, b;
                    GUILayout.BeginHorizontal();
                    r = RTEditorGUI.FloatField(textColor.r);
                    g = RTEditorGUI.FloatField(textColor.g);
                    b = RTEditorGUI.FloatField(textColor.b);
                    GUILayout.EndHorizontal();
                    textColor = new Color(r, g, b);
                }

                GUILayout.Label("Answers:");
                if (answers == null)
                {
                    answers = new List<string>();
                    answers.Add("Ok.");
                    if (outputKnobs.Count == 2 && outputRight != null)
                        CreateConnectionKnob(outputAttribute);
                    if (outputRight && outputRight.connected())
                    {
                        //Debug.Log(outputRight.connections[0]);
                        outputKnobs[2].ApplyConnection(outputRight.connections[0]);
                        //outputKnobs[2].connections.Add(outputRight.connections[0]);
                    }

                    outputKnobs[1].DisplayLayout();
                }

                if (outputRight)
                {
                    DeleteConnectionPort(outputRight);
                }

                for (int i = 0; i < answers.Count; i++)
                {
                    RTEditorGUI.Seperator();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                    {
                        DeleteConnectionPort(outputPorts[i + 1]);
                        answers.RemoveAt(i);
                        i--;
                        if (i == -1)
                        {
                            break;
                        }

                        continue;
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    answers[i] = RTEditorGUI.TextField(answers[i]);


                    outputKnobs[i + 1].DisplayLayout();

                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add", GUILayout.ExpandWidth(false), GUILayout.MinWidth(100f)))
                {
                    CreateConnectionKnob(outputAttribute);
                    answers.Add("");
                }

                GUILayout.EndHorizontal();
            }
            else
            {
                if (outputRight)
                {
                    DeleteConnectionPort(outputRight);
                }
                if (outputKnobs.Count == 1)
                {
                    CreateConnectionKnob(outputAttribute);
                }
                outputKnobs[1].DisplayLayout();
                answers = null;
                for (int i = 1; i < outputKnobs.Count - 1; i++)
                {
                    DeleteConnectionPort(outputPorts[i]);
                }
            }
        }

        void SetEntityID(string ID)
        {
            Debug.Log($"selected ID {ID}!");

            rewardGiverID = ID;
            WorldCreatorCursor.selectEntity -= SetEntityID;
        }

        public void OnClick(int index)
        {
            DialogueSystem.OnDialogueCancel -= OnCancel;
            DialogueSystem.OnDialogueEnd = null;

            // hack: just increment index again to avoid upper port
            if (outputRight && outputRight.connected())
            {
                TaskManager.Instance.setNode(outputRight);
            }
            else
            {
                TaskManager.Instance.setNode(outputPorts[index + 1]);
            }

            DialogueSystem.Instance.DialogueViewTransitionOut();
        }

        public void OnCancel()
        {
            OnClick(1);
        }

        public void OnDialogue()
        {
            // draw objectives
            if (TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Contains(objectiveLocation))
            {
                TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Remove(objectiveLocation);
            }

            TaskManager.DrawObjectiveLocations();

            DialogueSystem.ShowFinishTaskNode(this, SectorManager.instance.GetEntity(rewardGiverID));
            //DialogueSystem.ShowPopup(rewardText, textColor, );

            DialogueSystem.OnDialogueEnd = OnClick;
            DialogueSystem.OnDialogueCancel = OnCancel;
            if (outputUp.connected())
            {
                RewardPlayer();
            }
        }

        private void RewardPlayer()
        {
            if (outputUp.connection(0).body is StartTaskNode taskNode)
            {
                string taskID = taskNode.taskID;
                TaskManager.Instance.endTask(taskID);
                Debug.Log("Task complete!");
                SectorManager.instance.player.AddCredits(taskNode.creditReward);
                SectorManager.instance.player.reputation += taskNode.reputationReward;
                SectorManager.instance.player.shards += taskNode.shardReward;
                if (taskNode.partReward)
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

        public override int Traverse()
        {

            SectorManager.instance.player.alerter.showMessage("TASK COMPLETE", "clip_victory");
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
                    else
                    {
                        Debug.LogWarning(taskNode.entityIDforConfirmedResponse + " missing from interaction override dictionary!");
                    }
                }

                var mission = PlayerCore.Instance.cursave.missions.Find((x) => x.name == (Canvas as QuestCanvas).missionName);
            }

            if (!speakToEntity)
            {
                RewardPlayer();
                return 2;
            }

            TaskManager.speakerIDList.Add(rewardGiverID);
            if (TaskManager.interactionOverrides.ContainsKey(rewardGiverID))
            {
                Debug.Log("Contains key");
                TaskManager.interactionOverrides[rewardGiverID].Push(() =>
                {
                    TaskManager.interactionOverrides[rewardGiverID].Pop();
                    OnDialogue();
                });
            }
            else
            {
                var stack = new Stack<UnityEngine.Events.UnityAction>();
                stack.Push(() =>
                {
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
            foreach (var ent in AIData.entities)
            {
                if (!ent)
                {
                    continue;
                }

                if (ent.ID == rewardGiverID)
                {
                    TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Clear();
                    objectiveLocation = new TaskManager.ObjectiveLocation(
                        ent.transform.position,
                        true,
                        (Canvas as QuestCanvas).missionName,
                        SectorManager.instance.current.dimension,
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
