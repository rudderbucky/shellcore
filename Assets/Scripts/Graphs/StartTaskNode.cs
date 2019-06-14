using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "TaskSystem/StartTask")]
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

        [ConnectionKnob("Input Left", Direction.In, "Task", NodeSide.Left, 20)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Right", Direction.Out, "Task", NodeSide.Right, 20)]
        public ConnectionKnob outputRight;

        [ConnectionKnob("Input Up", Direction.In, "Complete", NodeSide.Top, 100f)]
        public ConnectionKnob inputUp;

        public override void NodeGUI()
        {
            GUILayout.Label("Task ID:");
            taskID = GUILayout.TextField(taskID, GUILayout.Width(200f));
            GUILayout.Label("Task giver ID");
            taskGiverID = GUILayout.TextField(taskGiverID, GUILayout.Width(200f));
            GUILayout.Label("Description");
            description = GUILayout.TextArea(description, GUILayout.Width(200f));
            GUILayout.Label("Credit reward");
            creditReward = RTEditorGUI.IntField(creditReward, GUILayout.Width(200f));
            partReward = RTEditorGUI.Toggle(partReward, "Part reward", GUILayout.Width(200f));
            if(partReward)
            {
                height = 300f;
                GUILayout.Label("Part ID:");
                partID = GUILayout.TextField(partID);
                if (ResourceManager.Instance && ResourceManager.GetAsset<Sprite>(partID) != null)
                {
                    GUILayout.Label(ResourceManager.GetAsset<Sprite>(partID).texture, GUILayout.Width(200f));
                    height = 500f;
                }
                partAbilityID = RTEditorGUI.IntField("Ability ID", partAbilityID, GUILayout.Width(200f));
                partTier = RTEditorGUI.IntField("Part tier", partTier, GUILayout.Width(200f));
            }
            else
            {
                height = 220f;
            }

        }

        public override int Traverse()
        {
            Task task = new Task()
            {
                description = description,
                creditReward = creditReward,
            };
            if(partReward)
            {
                task.partReward = new EntityBlueprint.PartInfo
                {
                    partID = partID,
                    abilityID = partAbilityID,
                    tier = partTier
                };
            }
            TaskManager.Instance.AddTask(task);
            return 0;
        }
    }
}