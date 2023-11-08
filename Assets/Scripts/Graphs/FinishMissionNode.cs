using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Tasks/Finish Mission", typeof(QuestCanvas))]
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

        public override Vector2 MinSize
        {
            get { return new Vector2(208, 300f); }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob inputLeft;

        public string rewardsText;
        public string jingleID;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Sound Jingle ID:");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            jingleID = GUILayout.TextField(jingleID);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Reward Text Field:");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            rewardsText = GUILayout.TextArea(rewardsText, GUILayout.ExpandHeight(false));
            GUILayout.EndHorizontal();
        }

        public override int Traverse()
        {
            if (!PlayerCore.Instance || PlayerCore.Instance.cursave == null || PlayerCore.Instance.cursave.missions == null)
            {
                return -1;
            }

            if (!TaskManager.objectiveLocations.ContainsKey((Canvas as QuestCanvas).missionName))
            {
                Debug.LogWarning($"Task Manager does not contain an objective list for mission {(Canvas as QuestCanvas).missionName}");
            }
            // TODO: prevent using this node in DialogueCanvases
            var mission = PlayerCore.Instance.cursave.missions.Find(
                (m) => m.name == (Canvas as QuestCanvas).missionName);
            mission.status = Mission.MissionStatus.Complete;
            if (MissionCondition.OnMissionStatusChange != null)
            {
                MissionCondition.OnMissionStatusChange.Invoke(mission);
            }

            if (CoreScriptsManager.OnVariableUpdate != null)
            {
                CoreScriptsManager.OnVariableUpdate.Invoke("MissionStatus(");
            }
            // try loading all missions for which this mission is a prerequisite
            PlayerCore.Instance.cursave.missions.FindAll(m => m.prerequisites != null && m.prerequisites.Contains(mission.name))
                .ForEach(m => TaskManager.Instance.startNewQuest(m.name));

            DialogueSystem.ShowMissionComplete(mission, rewardsText);
            AudioManager.OverrideMusicTemporarily(jingleID);

            if (TaskManager.objectiveLocations.ContainsKey((Canvas as QuestCanvas).missionName))
            {
                TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Clear();
            }

            (Canvas.Traversal as Traverser).lastCheckpointName = mission.name + "_complete";
            TaskManager.Instance.AttemptAutoSave();
            return -1;
        }
    }
}
