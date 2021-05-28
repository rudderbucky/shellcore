using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Update Achievement Progress")]
    public class UpdateAchievementProgressNode : Node
    {
        //Node things
        public const string ID = "UpdateAchievementProgressNode";
        public override string GetName { get { return ID; } }

        public override string Title { get { return "Update Achievement Progress"; } }
        public override Vector2 DefaultSize { get { return new Vector2(208, height); } }

        float height = 300f;

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputRight;
        public int progressIncrease;
        public override void NodeGUI()
        {
            GUILayout.Label("Progress Increase:");
            progressIncrease = RTEditorGUI.IntField(progressIncrease, GUILayout.Width(200f));
        }

        public override int Traverse()
        {
            // TODO: prevent using this node in DialogueCanvases
            var achievement = PlayerCore.Instance.cursave.achievements.Find(
                (m) => m.name == (Canvas as QuestCanvas).missionName);
            achievement.progress += progressIncrease;
            if (achievement.progress > 100)
                achievement.completion = true;
            SectorManager.instance.player.alerter.showMessage("ACHIEVEMENT OBTAINED", "clip_victory");;             
            return -1;
        }
    }
}