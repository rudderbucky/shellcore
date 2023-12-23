using NodeEditorFramework.Utilities;
using UnityEngine;
using static CoreScriptsManager;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Win Siege Zone")]
    public class WinSiegeCondition : Node, ICondition
    {
        public const string ID = "WinSiegeCondition";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Win Siege Zone"; }
        }

        private ConditionState state;

        public ConditionState State
        {
            get { return state; }
            set { state = value; }
        }

        public string sectorName;

        [ConnectionKnob("Output", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob output;

        public override void NodeGUI()
        {
            output.DisplayLayout();
            GUILayout.Label("Sector Name:");
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
