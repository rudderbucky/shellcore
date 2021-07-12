using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Start Mission", typeof(QuestCanvas))]
    public class StartMissionNode : Node
    {
        //Node things
        public const string ID = "StartMissionNode";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Start Mission"; }
        }

        public override Vector2 MinSize
        {
            get { return new Vector2(258, 250); }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputRight;

        public string missionName;
        public string rank;
        public string entryPoint;
        public List<string> prerequisites = new List<string>();
        public string prerequisitesUnsatisifedText;
        public Color textColor = Color.white;
        public bool overrideCheckpoint;
        public string overrideCheckpointName;
        public int episode = 0;

        public override void NodeGUI()
        {
            GUILayout.Label("Mission Name:");
            missionName = GUILayout.TextField(missionName, GUILayout.Width(250));
            (Canvas as QuestCanvas).missionName = missionName;

            GUILayout.Label("Mission Rank:");
            rank = GUILayout.TextField(rank, GUILayout.Width(250));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Entry Point Hint:");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            entryPoint = GUILayout.TextArea(entryPoint, GUILayout.Width(250));
            GUILayout.EndHorizontal();

            for (int i = 0; i < prerequisites.Count; i++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                {
                    // Remove current label
                    prerequisites.RemoveAt(i);
                    i--;
                    GUILayout.EndHorizontal();
                    continue;
                }

                GUILayout.Label("Prerequisite " + i);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                prerequisites[i] = GUILayout.TextArea(prerequisites[i], GUILayout.Width(250));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add prerequisite missions", GUILayout.MinWidth(250)))
            {
                prerequisites.Add("");
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            episode = RTEditorGUI.IntField("Episode:", episode);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Dialogue to show if prerequisites unsatisfied:");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            prerequisitesUnsatisifedText = GUILayout.TextArea(prerequisitesUnsatisifedText, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.Label("Dialogue Text Color:");
            float r, g, b;
            GUILayout.BeginHorizontal();
            r = RTEditorGUI.FloatField(textColor.r);
            g = RTEditorGUI.FloatField(textColor.g);
            b = RTEditorGUI.FloatField(textColor.b);
            GUILayout.EndHorizontal();
            textColor = new Color(r, g, b);

            if (overrideCheckpoint = GUILayout.Toggle(overrideCheckpoint, "Override Checkpoint", GUILayout.MinWidth(250)))
            {
                GUILayout.Label("Checkpoint name: ");
                overrideCheckpointName = GUILayout.TextField(overrideCheckpointName);
            }
        }

        public override int Traverse()
        {
            TryAddMission();
            return 0;
        }

        public void TryAddMission()
        {
            if (PlayerCore.Instance.cursave.missions.TrueForAll((x) => { return x.name != missionName; }))
            {
                var mission = new Mission()
                {
                    name = missionName,
                    rank = rank,
                    status = Mission.MissionStatus.Inactive,
                    tasks = new List<Task>(),
                    prerequisites = prerequisites,
                    entryPoint = entryPoint,
                    textColor = textColor,
                    episode = episode
                };
                PlayerCore.Instance.cursave.missions.Add(mission);
            }
            else
            {
                var mission = PlayerCore.Instance.cursave.missions.Find((x) => { return x.name == missionName; });
                mission.rank = rank;
                mission.prerequisites = prerequisites;
                mission.entryPoint = entryPoint;
                mission.textColor = textColor;
                mission.episode = episode;
            }

            // TODO: Prevent this from breaking the game by not allowing this node in dialogue canvases
            (Canvas as QuestCanvas).missionName = missionName;
        }
    }
}
