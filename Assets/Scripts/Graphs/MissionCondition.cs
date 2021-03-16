using System.Collections;
using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Mission")]
    public class MissionCondition : Node, ICondition
    {
        public const string ID = "MissionCondition";
        public override string GetName { get { return ID; } }
        public override string Title { get { return "Mission Condition"; } }
        public override Vector2 DefaultSize { get { return new Vector2(200, 200); } }

        private ConditionState state;
        public ConditionState State { get { return state; } set { state = value; } }

        readonly string[] missionStatusTexts =
        {
            "Inactive",
            "Ongoing",
            "Complete"
        };

        public delegate void MissionStatusDelegate(Mission mission);
        public static MissionStatusDelegate OnMissionStatusChange;

        public string missionName;
        public int missionStatus;

        [ConnectionKnob("Output", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob output;

        public override void NodeGUI()
        {
            output.DisplayLayout();
            GUILayout.Label("Mission name");
            missionName = RTEditorGUI.TextField(missionName);

            missionStatus = GUILayout.SelectionGrid(missionStatus, missionStatusTexts, 1);
        }

        public void Init(int index)
        {
            Debug.Log("Init...");
            for (int i = 0; i < PlayerCore.Instance.cursave.missions.Count; i++)
            {
                Debug.Log("Mission: " + PlayerCore.Instance.cursave.missions[i].name + ", status: " + PlayerCore.Instance.cursave.missions[i].status);
                if (PlayerCore.Instance.cursave.missions[i].name == missionName &&
                    PlayerCore.Instance.cursave.missions[i].status == (Mission.MissionStatus)missionStatus)
                {
                    state = ConditionState.Completed;
                    output.connection(0).body.Calculate();
                    return;
                }
            }

            OnMissionStatusChange += MissionStatus;

            state = ConditionState.Listening;
        }

        public void DeInit()
        {
            OnMissionStatusChange -= MissionStatus;
            state = ConditionState.Uninitialized;
        }

        void MissionStatus(Mission mission)
        {
            if (mission.name == missionName)
            {
                if ((Mission.MissionStatus)missionStatus == mission.status)
                {
                    state = ConditionState.Completed;
                    output.connection(0).body.Calculate();
                }
            }
        }
    }
}