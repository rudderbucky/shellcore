using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Tasks/StartTask")]
    public class StartTaskNode : Node
    {
        //Node things
        public const string ID = "StartTaskNode";
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Start Task"; } }
        public override Vector2 DefaultSize { get { return new Vector2(208, height); } }

        //Task related
        public string taskID = "";
        public string description = "";
        public int creditReward = 100;
        //public EntityBlueprint.PartInfo partReward;
        public bool partReward = false;
        public string partID = "";
        public int partAbilityID = 0;
        public int partTier = 1;
        public string taskGiverID = "";

        float height = 220f;

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
            height = 0f;
            GUILayout.Label("Task giver ID");
            taskGiverID = GUILayout.TextField(taskGiverID, GUILayout.Width(200f));
            GUILayout.Label("Description");
            description = GUILayout.TextArea(description, GUILayout.Width(200f));
            height += GUI.skin.textArea.CalcHeight(new GUIContent(description), 200f);
            GUILayout.Label("Credit reward");
            creditReward = RTEditorGUI.IntField(creditReward, GUILayout.Width(200f));
            partReward = RTEditorGUI.Toggle(partReward, "Part reward", GUILayout.Width(200f));
            if(partReward)
            {
                height += 300f;
                GUILayout.Label("Part ID:");
                partID = GUILayout.TextField(partID, GUILayout.Width(200f));
                if (ResourceManager.Instance && ResourceManager.GetAsset<Sprite>(partID) != null)
                {
                    GUILayout.Label(ResourceManager.GetAsset<Sprite>(partID).texture, GUILayout.Width(200f));
                    height += 200f;
                }
                partAbilityID = RTEditorGUI.IntField("Ability ID", partAbilityID, GUILayout.Width(200f));
                partTier = RTEditorGUI.IntField("Part tier", partTier, GUILayout.Width(200f));
            }
            else
            {
                height += 220f;
            }
        }

        public void OnClick(int index)
        {
            DialogueSystem.OnDialogueEnd -= OnClick;
            if (index != 0)
            {
                taskID = GetHashCode().ToString();
                Task task = new Task()
                {
                    taskID = taskID,
                    description = description,
                    creditReward = creditReward,
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
            DialogueSystem.ShowTaskPrompt(this);
            DialogueSystem.OnDialogueEnd += OnClick;
            return -1;
        }
    }
}