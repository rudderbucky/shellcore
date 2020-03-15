using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Tasks/StartTask")]
    public class StartTaskNode : Node
    {
        //Node things
        public const string ID = "StartTaskNode";
        public override string GetName { get { return ID; } }

        public override string Title { get { return "Start Task"; } }
        public override Vector2 DefaultSize { get { return new Vector2(208, height); } }

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
        public string partID = "";
        public int partAbilityID = 0;
        public int partTier = 1;
        public int reputationReward = 0;
        bool init = false;
        Texture2D partTexture;
        float height = 220f;
        public bool forceTask = false;

        [ConnectionKnob("Input Left", Direction.In, "Dialogue", NodeSide.Left)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Accept", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputAccept;
        [ConnectionKnob("Output Decline", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputDecline;

        [ConnectionKnob("Input Up", Direction.In, "Complete", NodeSide.Top, 100f)]
        public ConnectionKnob inputUp;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            inputLeft.DisplayLayout();
            outputAccept.DisplayLayout();
            GUILayout.EndHorizontal();
            outputDecline.DisplayLayout();
            height = 110f;
            GUILayout.Label("Dialogue:");
            dialogueText = GUILayout.TextArea(dialogueText, GUILayout.Width(200f));
            height += GUI.skin.textArea.CalcHeight(new GUIContent(dialogueText), 200f);
            GUILayout.Label("Dialogue Color:");
            float r, g, b;
            GUILayout.BeginHorizontal();
            r = RTEditorGUI.FloatField(dialogueColor.r);
            g = RTEditorGUI.FloatField(dialogueColor.g);
            b = RTEditorGUI.FloatField(dialogueColor.b);
            GUILayout.EndHorizontal();
            dialogueColor = new Color(r, g, b);
            GUILayout.Label("Objective list:");
            objectiveList = GUILayout.TextArea(objectiveList, GUILayout.Width(200f));
            height += GUI.skin.textArea.CalcHeight(new GUIContent(objectiveList), 200f);
            GUILayout.Label("Credit reward:");
            creditReward = RTEditorGUI.IntField(creditReward, GUILayout.Width(200f));
            GUILayout.Label("Reputation reward:");
            reputationReward = RTEditorGUI.IntField(reputationReward, GUILayout.Width(200f));
            partReward = RTEditorGUI.Toggle(partReward, "Part reward", GUILayout.Width(200f));
            if(partReward)
            {
                height += 264f;
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
            }
            else
            {
                height += 160f;
            }
            forceTask = Utilities.RTEditorGUI.Toggle(forceTask, "Force Task Acceptance");
            height += GUI.skin.textArea.CalcHeight(new GUIContent(dialogueText), 50f);
        }

        public void OnClick(int index)
        {
            DialogueSystem.OnDialogueEnd -= OnClick;
            if (index != 0)
            {
                TaskManager.interactionOverrides.Remove(StartDialogueNode.dialogueStartNode.EntityName);
                StartTask();
                TaskManager.Instance.setNode(outputAccept);
            }
            else
            {
                if (outputDecline.connected())
                    TaskManager.Instance.setNode(outputDecline);
                else
                    TaskManager.Instance.setNode(StartDialogueNode.dialogueStartNode);
            }
        }

        public override int Traverse()
        {
            Debug.Log("Force Task: " + forceTask);
            if(!forceTask)
            {
                DialogueSystem.ShowTaskPrompt(this, TaskManager.GetSpeaker());
                DialogueSystem.OnDialogueEnd += OnClick;
                return -1;
            }
            else
            {
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
            TaskManager.Instance.AddTask(task);
        }
    }
}