using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/FinishMission")]
    public class FinishMissionNode : Node
    {
        //Node things
        public const string ID = "FinishMissionNode";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Finish Mission"; }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(208, height); }
        }

        float height = 300f;

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob inputLeft;

        public string rewardsText;
        public string jingleID;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Sound Jingle ID");
            jingleID = GUILayout.TextField(jingleID);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Rewards text field");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            rewardsText = GUILayout.TextArea(rewardsText);
            GUILayout.EndHorizontal();
        }

        public override int Traverse()
        {
            var missionName = (Canvas as QuestCanvas).missionName;
            // TODO: prevent using this node in DialogueCanvases
            var mission = PlayerCore.Instance.cursave.missions.Find(
                (m) => m.name == missionName);
            mission.status = Mission.MissionStatus.Complete;
            if (MissionCondition.OnMissionStatusChange != null)
            {
                MissionCondition.OnMissionStatusChange.Invoke(mission);
            }

            DialogueSystem.ShowMissionComplete(mission, rewardsText);
            AudioManager.OverrideMusicTemporarily(jingleID);

            if (TaskManager.objectiveLocations.ContainsKey(missionName))
            {
                TaskManager.objectiveLocations[missionName].Clear();
            }

            (Canvas.Traversal as Traverser).lastCheckpointName = mission.name + "_complete";
            TaskManager.Instance.AttemptAutoSave();
            return -1;
        }
    }
}
