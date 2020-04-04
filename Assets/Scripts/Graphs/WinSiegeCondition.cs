using System.Collections;
using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Win Siege Zone")]
    public class WinSiegeCondition : Node, ICondition
    {
        public const string ID = "WinSiegeCondition";
        public override string GetName { get { return ID; } }
        public override string Title { get { return "Win Siege Zone"; } }

        private ConditionState state;
        public ConditionState State { get { return state; } set { state = value; } }

        public delegate void SiegeZoneWonDelegate(string sectorName);
        public static SiegeZoneWonDelegate OnSiegeWin;

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
            OnSiegeWin += SiegeWin;

            state = ConditionState.Listening;
        }

        public void DeInit()
        {
            OnSiegeWin -= SiegeWin;
            state = ConditionState.Uninitialized;
        }

        void SiegeWin(string sector)
        {
            if (sector == sectorName)
            {
                state = ConditionState.Completed;
                output.connection(0).body.Calculate();
            }
        }
    }
}