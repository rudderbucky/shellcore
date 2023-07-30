using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Tasks/Start Task", typeof(QuestCanvas))]
    public class StartTaskNode : Node
    {
        //Node things
        public const string ID = "StartTaskNode";

        public override string GetName
        {
            get { return ID; }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        public override string Title
        {
            get { return "Start Task"; }
        }

        public override Vector2 MinSize
        {
            get { return new Vector2(208, 50); }
        }

        //Task related
        public string taskID
        {
            get { return "Task_" + GetID(); }
        }

        public string dialogueText = "";
        public Color dialogueColor = Color.white;
        public string objectiveList = "";

        public int creditReward = 100;

        //public EntityBlueprint.PartInfo partReward;
        public bool partReward = false;
        public string entityIDforConfirmedResponse;
        public string partID = "";
        public int partAbilityID = 0;
        public int partTier = 1;
        public string partSecondaryData = "";
        public int reputationReward = 0;
        public int shardReward = 0;
        public string taskName = "";
        public string acceptResponse;
        public string declineResponse;
        public string taskConfirmedDialogue;
        public string actionResponse;
        public bool useEntityColor = true;
        bool init = false;
        Texture2D partTexture;
        float height = 320f;
        public bool forceTask = false;

        [ConnectionKnob("Input Left", Direction.In, "Dialogue", NodeSide.Left)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Accept", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputAccept;

        [ConnectionKnob("Output Decline", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputDecline;

        [ConnectionKnob("Input Up", Direction.In, "Complete", NodeSide.Top, 104)]
        public ConnectionKnob inputUp;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            inputLeft.DisplayLayout();
            outputAccept.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            outputDecline.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Task Name:");

            GUILayout.EndHorizontal();
            //height = 270f;
            GUILayout.BeginHorizontal();

            taskName = GUILayout.TextArea(taskName, GUILayout.Width(200f));
            GUILayout.EndHorizontal();

            GUILayout.Label("Dialogue:");
            dialogueText = GUILayout.TextArea(dialogueText, GUILayout.Width(200f));
            height += GUI.skin.textArea.CalcHeight(new GUIContent(dialogueText), 200f);
            if (!(useEntityColor = GUILayout.Toggle(useEntityColor, "Use entity color")))
            {
                GUILayout.Label("Text Color:");
                float r, g, b;
                GUILayout.BeginHorizontal();
                r = RTEditorGUI.FloatField(dialogueColor.r);
                if (dialogueColor.r < 0 || dialogueColor.r > 1)
                {
                    r = RTEditorGUI.FloatField(dialogueColor.r = 1);
                    Debug.LogWarning("Can't register this numbers!");
                }
                g = RTEditorGUI.FloatField(dialogueColor.g);
                if (dialogueColor.g < 0 || dialogueColor.g > 1)
                {
                    g = RTEditorGUI.FloatField(dialogueColor.g = 1);
                    Debug.LogWarning("Can't register this numbers!");
                }
                b = RTEditorGUI.FloatField(dialogueColor.b);
                if (dialogueColor.b < 0 || dialogueColor.b > 1)
                {
                    b = RTEditorGUI.FloatField(dialogueColor.b = 1);
                    Debug.LogWarning("Can't register this numbers!");
                }
                GUILayout.EndHorizontal();
                dialogueColor = new Color(r, g, b);
            }

            GUILayout.BeginHorizontal();
            if (forceTask == false)
            {
                GUILayout.Label("Accept Player Response:");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                acceptResponse = GUILayout.TextArea(acceptResponse, GUILayout.Width(200f));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Decline Player Response:");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                declineResponse = GUILayout.TextArea(declineResponse, GUILayout.Width(200f));
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("Objective List:");
            objectiveList = GUILayout.TextArea(objectiveList, GUILayout.Width(200f));
            creditReward = RTEditorGUI.IntField("Credit Reward: ", creditReward);
            reputationReward = RTEditorGUI.IntField("Reputation Reward: ", reputationReward);
            shardReward = RTEditorGUI.IntField("Shard Reward: ", shardReward);

            partReward = RTEditorGUI.Toggle(partReward, "Part reward", GUILayout.Width(200f));
            if (partReward)
            {
                height += 320f;
                GUILayout.Label("Part ID:");
                partID = GUILayout.TextField(partID, GUILayout.Width(200f));
                if (ResourceManager.Instance != null && partID != null && (GUI.changed || !init))
                {
                    init = true;
                    PartBlueprint partBlueprint = ResourceManager.GetAsset<PartBlueprint>(partID);
                    if (partBlueprint != null)
                    {
                        partTexture = ResourceManager.GetAsset<Sprite>(partBlueprint.spriteID).texture;
                    }
                    else
                    {
                        partTexture = null;
                    }
                }

                if (partTexture != null)
                {
                    GUILayout.Label(partTexture);
                    height += partTexture.height + 8f;
                }
                else
                {
                    NodeEditorGUI.nodeSkin.label.normal.textColor = Color.red;
                    GUILayout.Label("<Part not found>");
                    NodeEditorGUI.nodeSkin.label.normal.textColor = NodeEditorGUI.NE_TextColor;
                }

                partAbilityID = RTEditorGUI.IntField("Ability ID", partAbilityID, GUILayout.Width(200f));
                if (partAbilityID < 0)
                {
                    partAbilityID = RTEditorGUI.IntField("Ability ID", 0, GUILayout.Width(200f));
                    Debug.LogWarning("This identification does not exist!");
                }
                string abilityName = AbilityUtilities.GetAbilityNameByID(partAbilityID, null);
                if (abilityName != "Name unset")
                {
                    GUILayout.Label("Ability: " + abilityName);
                    height += 24f;
                }
                partTier = RTEditorGUI.IntField("Part tier", partTier, GUILayout.Width(200f));
                GUILayout.Label("Part Secondary Data:");
                partSecondaryData = GUILayout.TextArea(partSecondaryData, GUILayout.Width(200f));
            }
            else
            {
                height += 160f;
            }

            forceTask = Utilities.RTEditorGUI.Toggle(forceTask, "Force Task Acceptance");
            height += GUI.skin.textArea.CalcHeight(new GUIContent(dialogueText), 50f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Entity ID for Confirmed Dialogue:");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            entityIDforConfirmedResponse = GUILayout.TextField(entityIDforConfirmedResponse, GUILayout.Width(200f));
            GUILayout.EndHorizontal();

            GUILayout.Label("Task Confirmed Dialogue:");
            taskConfirmedDialogue = GUILayout.TextArea(taskConfirmedDialogue, GUILayout.Width(200f));
            height += GUI.skin.textArea.CalcHeight(new GUIContent(taskConfirmedDialogue), 200f);
            GUILayout.Label("Player Confirmed Response:");
            actionResponse = GUILayout.TextArea(actionResponse, GUILayout.Width(200f));
        }

        public void OnClick(int index)
        {
            DialogueSystem.OnDialogueEnd -= OnClick;
            Debug.LogWarning(StartDialogueNode.missionCanvasNode.EntityID);
            TaskManager.interactionOverrides[StartDialogueNode.missionCanvasNode.EntityID].Pop();
            if (index != 0)
            {
                StartDialogueNode.missionCanvasNode = null;
                StartTask();
                TaskManager.Instance.setNode(outputAccept);
            }
            else
            {
                if (outputDecline.connected())
                {
                    TaskManager.Instance.setNode(outputDecline);
                }
                else
                {
                    TaskManager.Instance.setNode(StartDialogueNode.missionCanvasNode);
                }
            }
        }

        public void OnConfirmed()
        {
            Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
            dialogue.nodes = new List<Dialogue.Node>();
            var node = new Dialogue.Node();
            node.ID = 0;
            node.text = taskConfirmedDialogue != null ? taskConfirmedDialogue : "Complete the task."; // TODO: Why is this (and the color(?)) sometimes null? Is the task node not loaded correctly?
            TaskManager.speakerID = entityIDforConfirmedResponse;
            node.textColor = useEntityColor && TaskManager.GetSpeaker() ? FactionManager.GetFactionColor(TaskManager.GetSpeaker().faction) : dialogueColor;
            node.nextNodes = new List<int>() { 1 };

            var node1 = new Dialogue.Node();
            node1.ID = 1;
            node1.action = Dialogue.DialogueAction.Exit;
            node1.buttonText = actionResponse != null ? actionResponse : "Alright."; // Players can only make one response, I haven't figured a way to make more without breaking it. -FoeFear
            dialogue.nodes.Add(node);
            dialogue.nodes.Add(node1);
            DialogueSystem.StartDialogue(dialogue, TaskManager.GetSpeaker());
        }

        public override int Traverse()
        {
            if (!forceTask)
            {
                DialogueSystem.ShowTaskPrompt(this, TaskManager.GetSpeaker());
                DialogueSystem.OnDialogueEnd += OnClick;
                return -1;
            }
            else
            {
                if (StartDialogueNode.missionCanvasNode && StartDialogueNode.missionCanvasNode.EntityID != null
                                                        && TaskManager.interactionOverrides.ContainsKey(StartDialogueNode.missionCanvasNode.EntityID))
                {
                    TaskManager.interactionOverrides[StartDialogueNode.missionCanvasNode.EntityID].Pop();
                }

                StartDialogueNode.missionCanvasNode = null;
                StartTask();
                return 0;
            }
        }

        private void SetTaskCheckpoint()
        {
            (Canvas.Traversal as Traverser).lastCheckpointName = taskName;
            TaskManager.Instance.AttemptAutoSave();

            if (!string.IsNullOrEmpty(entityIDforConfirmedResponse))
            {
                InteractAction action = new InteractAction();
                action.action = () => OnConfirmed();
                TaskManager.Instance.PushInteractionOverrides(entityIDforConfirmedResponse, action, Canvas.Traversal as Traverser);
            }
        }

        public void RegisterTask()
        {
            Task task = new Task()
            {
                taskID = taskID,
                objectived = objectiveList,
                creditReward = creditReward,
                dialogue = dialogueText,
                dialogueColor = dialogueColor,
                reputationReward = reputationReward,
                shardReward = shardReward
            };
            if (partReward)
            {
                task.partReward = new EntityBlueprint.PartInfo
                {
                    partID = partID,
                    abilityID = partAbilityID,
                    tier = partTier
                };
            }

            // TODO: Prevent this from breaking the game by not allowing this node in dialogue canvases
            var mission = PlayerCore.Instance.cursave.missions.Find((x) => x.name == (Canvas as QuestCanvas).missionName);
            if (mission != null)
            {
                mission.status = Mission.MissionStatus.Ongoing;
                if (MissionCondition.OnMissionStatusChange != null)
                {
                    MissionCondition.OnMissionStatusChange.Invoke(mission);
                }

                if (!mission.tasks.Exists((x) => x.dialogue == task.dialogue))
                {
                    mission.tasks.Add(task);
                }
            }
        }

        public void StartTask()
        {
            RegisterTask();

            SetTaskCheckpoint();
        }
    }
}
