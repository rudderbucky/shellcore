using UnityEngine;
using NodeEditorFramework.Utilities;
using System.Collections.Generic;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Tasks/StartTask", typeof(QuestCanvas))]
    public class StartTaskNode : Node
    {
        //Node things
        public const string ID = "StartTaskNode";
        public override string GetName { get { return ID; } }

        public override bool AutoLayout { get { return true; } }

        public override string Title { get { return "Start Task"; } }
        public override Vector2 MinSize { get { return new Vector2(208, 50); } }
    
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
        public int shardReward;
        public string taskName = "";
        public string acceptResponse;
        public string declineResponse;
        public string taskConfirmedDialogue;
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
            if(!(useEntityColor = GUILayout.Toggle(useEntityColor, "Use entity color")))
            {
                GUILayout.Label("Text Color:");
                float r, g, b;
                GUILayout.BeginHorizontal();
                r = RTEditorGUI.FloatField(dialogueColor.r);
                g = RTEditorGUI.FloatField(dialogueColor.g);
                b = RTEditorGUI.FloatField(dialogueColor.b);
                GUILayout.EndHorizontal();
                dialogueColor = new Color(r, g, b);
            }
            GUILayout.BeginHorizontal();
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
            GUILayout.EndHorizontal();
            GUILayout.Label("Objective list:");
            objectiveList = GUILayout.TextArea(objectiveList, GUILayout.Width(200f));
            GUILayout.Label("Credit reward:");
            creditReward = RTEditorGUI.IntField(creditReward, GUILayout.Width(208f));
            GUILayout.Label("Reputation reward:");

            reputationReward = RTEditorGUI.IntField(reputationReward, GUILayout.Width(208f));
            GUILayout.Label("Shard reward:");
            shardReward = RTEditorGUI.IntField(shardReward, GUILayout.Width(208f));

            partReward = RTEditorGUI.Toggle(partReward, "Part reward", GUILayout.Width(200f));
            if(partReward)
            {
                height += 320f;
                GUILayout.Label("Part ID:");
                partID = GUILayout.TextField(partID, GUILayout.Width(200f));
                if (ResourceManager.Instance != null && partID != null && (GUI.changed || !init))
                {
                    init = true;
                    PartBlueprint partBlueprint = ResourceManager.GetAsset<PartBlueprint>(partID);
                    if(partBlueprint != null)
                    {
                        partTexture = ResourceManager.GetAsset<Sprite>(partBlueprint.spriteID).texture;
                    }
                    else
                    {
                        partTexture = null;
                    }
                }
                if(partTexture != null)
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
                string abilityName = AbilityUtilities.GetAbilityNameByID(partAbilityID, null);
                if (abilityName != "Name unset")
                {
                    GUILayout.Label("Ability: " + abilityName);
                    height += 24f;
                }
                partTier = RTEditorGUI.IntField("Part tier", partTier, GUILayout.Width(200f));
                GUILayout.Label("Part Secondary Data:");
                partSecondaryData = GUILayout.TextField(partSecondaryData, GUILayout.Width(200f));
            }
            else
            {
                height += 160f;
            }
            forceTask = Utilities.RTEditorGUI.Toggle(forceTask, "Force Task Acceptance");
            height += GUI.skin.textArea.CalcHeight(new GUIContent(dialogueText), 50f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Entity ID for Confirmed Dialogue");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            entityIDforConfirmedResponse = GUILayout.TextField(entityIDforConfirmedResponse, GUILayout.Width(200f));
            GUILayout.EndHorizontal();

            GUILayout.Label("Task Confirmed Dialogue:");
            taskConfirmedDialogue = GUILayout.TextArea(taskConfirmedDialogue, GUILayout.Width(200f));
            height += GUI.skin.textArea.CalcHeight(new GUIContent(taskConfirmedDialogue), 200f);
        }

        public void OnClick(int index)
        {
            DialogueSystem.OnDialogueEnd -= OnClick;
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
                    TaskManager.Instance.setNode(outputDecline);
                else
                    TaskManager.Instance.setNode(StartDialogueNode.missionCanvasNode);
            }
        }

        public void OnConfirmed()
        {
            Dialogue dialogue = ScriptableObject.CreateInstance<Dialogue>();
            dialogue.nodes = new List<Dialogue.Node>();
            var node = new Dialogue.Node();
            node.ID = 0;
            node.text = taskConfirmedDialogue != null ? taskConfirmedDialogue : "Complete the task."; // TODO: Why is this (and the color(?)) sometimes null? Is the task node not loaded correctly?
            node.textColor = dialogueColor;
            node.nextNodes = new List<int>() {1};

            var node1 = new Dialogue.Node();
            node1.ID = 1;
            node1.action = Dialogue.DialogueAction.Exit;
            node1.buttonText = "Alright."; // TODO: allow customizing in World Creator?
            dialogue.nodes.Add(node);
            dialogue.nodes.Add(node1);
            TaskManager.speakerID = entityIDforConfirmedResponse;
            DialogueSystem.StartDialogue(dialogue, TaskManager.GetSpeaker());
        }

        public override int Traverse()
        {

            if(!forceTask)
            {
                DialogueSystem.ShowTaskPrompt(this, TaskManager.GetSpeaker());
                DialogueSystem.OnDialogueEnd += OnClick;
                return -1;
            }
            else
            {
                if(StartDialogueNode.missionCanvasNode && StartDialogueNode.missionCanvasNode.EntityID != null
                    && TaskManager.interactionOverrides.ContainsKey(StartDialogueNode.missionCanvasNode.EntityID))
                {
                    TaskManager.interactionOverrides[StartDialogueNode.missionCanvasNode.EntityID].Pop();
                }
                StartDialogueNode.missionCanvasNode = null;
                StartTask();
                return 0;
            }
        }

        public void StartTask()
        {
            Task task = new Task()
            {
                taskID = taskID,
                objectived = objectiveList,
                creditReward = creditReward,
                dialogue = dialogueText,
                dialogueColor = dialogueColor,
                reputationReward = reputationReward,
                shardReward = shardReward,
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
            if(mission != null)
            {
                mission.status = Mission.MissionStatus.Ongoing;
                if (MissionCondition.OnMissionStatusChange != null)
                    MissionCondition.OnMissionStatusChange.Invoke(mission);
                if(!mission.tasks.Exists((x) => x.dialogue == task.dialogue)) mission.tasks.Add(task);
            }

            (Canvas.Traversal as Traverser).lastCheckpointName = taskName;
            TaskManager.Instance.AttemptAutoSave();



            if(entityIDforConfirmedResponse != null && entityIDforConfirmedResponse != "")
            {
                if(TaskManager.interactionOverrides.ContainsKey(entityIDforConfirmedResponse))
                {
                    TaskManager.interactionOverrides[entityIDforConfirmedResponse].Push(() => OnConfirmed());
                }
                else 
                {
                    var stack = new Stack<UnityEngine.Events.UnityAction>();
                    stack.Push(() => OnConfirmed());
                    TaskManager.interactionOverrides.Add(entityIDforConfirmedResponse, stack);
                }
            }
        }
    }
}