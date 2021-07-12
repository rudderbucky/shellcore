using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Battle Zone Condition")]
    public class WinBattleCondition : Node, ICondition
    {
        public const string ID = "WinBattleCondition";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Battle Zone Condition"; }
        }

        private ConditionState state;

        public ConditionState State
        {
            get { return state; }
            set { state = value; }
        }

        public delegate void BattlezoneWonDelegate(string sectorName);

        public static BattlezoneWonDelegate OnBattleWin;
        public static BattlezoneWonDelegate OnBattleLose;

        public override bool AutoLayout
        {
            get { return true; }
        }

        public override Vector2 MinSize
        {
            get { return new Vector2(200, 50); }
        }

        public string sectorName;

        [ConnectionKnob("Output", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob output;

        public bool loseMode = false;

        public override void NodeGUI()
        {
            output.DisplayLayout();
            GUILayout.Label("Sector name");
            sectorName = RTEditorGUI.TextField(sectorName);
            loseMode = RTEditorGUI.Toggle(loseMode, "Check for loss instead of win?");
        }

        public void Init(int index)
        {
            if (!loseMode)
            {
                OnBattleWin += BattleEnd;
            }
            else
            {
                OnBattleLose += BattleEnd;
            }

            state = ConditionState.Listening;
        }

        public void DeInit()
        {
            if (!loseMode)
            {
                OnBattleWin -= BattleEnd;
            }
            else
            {
                OnBattleLose -= BattleEnd;
            }

            state = ConditionState.Uninitialized;
        }

        void BattleEnd(string sector)
        {
            if (sector == sectorName)
            {
                state = ConditionState.Completed;
                output.connection(0).body.Calculate();
            }
        }
    }
}
