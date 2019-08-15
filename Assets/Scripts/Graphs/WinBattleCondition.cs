using System.Collections;
using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Win Battle Zone")]
    public class WinBattleCondition : Node, ICondition
    {
        public const string ID = "WinBattleCondition";
        public override string GetName { get { return ID; } }
        public override string Title { get { return "Win Battle Zone"; } }

        private ConditionState state;
        public ConditionState State { get { return state; } set { state = value; } }

        public delegate void BattlezoneWonDelegate(string sectorName);
        public static BattlezoneWonDelegate OnBattleWin;

        public string sectorName;

        [ConnectionKnob("Output", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob output;

        public override void NodeGUI()
        {
            output.DisplayLayout();
            GUILayout.Label("Sector name");
            sectorName = RTEditorGUI.TextField(sectorName);
        }

        public void Init(int index)
        {
            OnBattleWin += BattleWin;

            state = ConditionState.Listening;
        }

        public void DeInit()
        {
            OnBattleWin -= BattleWin;
            state = ConditionState.Uninitialized;
        }

        void BattleWin(string sector)
        {
            if (sector == sectorName)
            {
                state = ConditionState.Completed;
                output.connection(0).body.Calculate();
            }
        }
    }
}