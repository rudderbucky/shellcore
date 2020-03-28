using NodeEditorFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Start Mission")]
    public class StartMissionNode : Node
    {
        //Node things
        public const string ID = "StartMissionNode";
        public override string GetName { get { return ID; } }

        public override string Title { get { return "Start Mission"; } }
        public override Vector2 DefaultSize { get { return new Vector2(250, height); } }

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputRight;

        public string missionName;
        public string rank;
        public string entryPoint;
        public List<string> prerequisites = new List<string>();
        public string prerequisitesUnsatisifedText;
        public Color textColor;
        float height = 400f;
        public override void NodeGUI()
        {
           
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mission Name:");
            missionName = GUILayout.TextArea(missionName, GUILayout.Width(100f));
            (Canvas as QuestCanvas).missionName = missionName;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mission Rank:");
            rank = GUILayout.TextArea(rank, GUILayout.Width(100f));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Entry Point Hint:");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            entryPoint = GUILayout.TextArea(entryPoint, GUILayout.Width(100f));
            GUILayout.EndHorizontal();

            for (int i = 0; i < prerequisites.Count; i++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                { // Remove current label
                    prerequisites.RemoveAt(i);
                    i--;
                    GUILayout.EndHorizontal();
                    height -= 50;
                    continue;
                }
                GUILayout.Label("Prerequisite " + i);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                prerequisites[i] = GUILayout.TextArea(prerequisites[i], GUILayout.Width(100f));
                GUILayout.EndHorizontal();
            }
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false), GUILayout.MinWidth(100f)))
            {
                prerequisites.Add("");
                height += 50;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Dialogue to show if prerequisites unsatisfied:");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            prerequisitesUnsatisifedText = GUILayout.TextArea(prerequisitesUnsatisifedText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.EndHorizontal();
            GUILayout.Label("Dialogue Text Color:");
            float r, g, b;
            GUILayout.BeginHorizontal();
            r = RTEditorGUI.FloatField(textColor.r);
            g = RTEditorGUI.FloatField(textColor.g);
            b = RTEditorGUI.FloatField(textColor.b);
            GUILayout.EndHorizontal();
            textColor = new Color(r, g, b);
        }

        public override int Traverse()
        {
            TryAddMission();
            return 0;
        }

        public void TryAddMission()
        {
            if(PlayerCore.Instance.cursave.missions.TrueForAll((x) => {return x.name != missionName;}))
            {
                var mission = new Mission();
                mission.name = missionName;
                mission.rank = rank;
                mission.prerequisites = prerequisites;
                mission.status = Mission.MissionStatus.Inactive;
                mission.tasks = new List<Task>();
                mission.entryPoint = entryPoint;
                mission.prerequisitesUnsatisifedText = prerequisitesUnsatisifedText;
                mission.textColor = textColor;
                PlayerCore.Instance.cursave.missions.Add(mission);
            }
            else
            {
                var mission = PlayerCore.Instance.cursave.missions.Find((x) => {return x.name == missionName;});
                mission.rank = rank;
                mission.prerequisites = prerequisites;
                mission.entryPoint = entryPoint;
                mission.prerequisitesUnsatisifedText = prerequisitesUnsatisifedText;
                mission.textColor = textColor;
            }

            // TODO: Prevent this from breaking the game by not allowing this node in dialogue canvases
            (Canvas as QuestCanvas).missionName = missionName;
        }
    }
}