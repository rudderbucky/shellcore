using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Update Achievement Progress", typeof(AchievementCanvas))]
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
        public float progressIncrease;
        public override void NodeGUI()
        {
            GUILayout.Label("Progress Increase:");
            progressIncrease = RTEditorGUI.FloatField(progressIncrease, GUILayout.Width(200f));
        }

        public override int Traverse()
        {
            // TODO: prevent using this node in DialogueCanvases
            var achievement = PlayerCore.Instance.cursave.achievements.Find(
                (m) => m.name == (Canvas as AchievementCanvas).missionName);
            achievement.progress += progressIncrease;
            if (achievement.progress > 100 && !achievement.completion)
                achievement.completion = true;
            SectorManager.instance.player.alerter.showMessage("ACHIEVEMENT OBTAINED", "clip_victory");;             
            return -1;
        }
    }
}