using NodeEditorFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Start Achievement", typeof(QuestCanvas))]
    public class StartAchievementNode : Node
    {
        //Node things
        public const string ID = "StartAchievementNode";
        public override string GetName { get { return ID; } }

        public override string Title { get { return "Start Achievement"; } }
        public override Vector2 DefaultSize { get { return new Vector2(250, height); } }

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputRight;

        public string achievementName;
        public string description;
        public Color textColor = Color.white;
        float height = 400f;
        public override void NodeGUI()
        {
           
            GUILayout.BeginHorizontal();
            GUILayout.Label("Achievement Name:");
            achievementName = GUILayout.TextArea(achievementName, GUILayout.Width(100f));
            (Canvas as QuestCanvas).missionName = achievementName;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Description:");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            description = GUILayout.TextArea(description, GUILayout.Width(100f));
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
            if(PlayerCore.Instance.cursave.achievements.TrueForAll((x) => {return x.name != achievementName;}))
            {

                var achievement = new Achievement();
                achievement.name = achievementName;
                achievement.completion = false;
                achievement.description = description;
                achievement.textColor = textColor;
                achievement.progress = 0;
                PlayerCore.Instance.cursave.achievements.Add(achievement);
            }
            else
            {
                var achievement = PlayerCore.Instance.cursave.achievements.Find((x) => {return x.name == achievementName;});
                achievement.description = description;
                achievement.textColor = textColor;
            }

            // TODO: Prevent this from breaking the game by not allowing this node in dialogue canvases
            (Canvas as QuestCanvas).missionName = achievementName;
        }
    }
}