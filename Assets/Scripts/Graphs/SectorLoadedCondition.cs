using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Sector Loaded Condition")]
    public class SectorLoadedCondition : Node, ICondition
    {
        public const string ID = "SectorLoadedCondition";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Sector Loaded Condition"; }
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
            GUILayout.Label("Sector name");
            sectorName = RTEditorGUI.TextField(sectorName);
        }

        public void Init(int index)
        {
            SectorManager.OnSectorLoad += SectorLoaded;

            state = ConditionState.Listening;
        }

        void SectorLoaded(string sector)
        {
            if (sector == sectorName)
            {
                state = ConditionState.Completed;
                output.connection(0).body.Calculate();
            }
        }

        public void DeInit()
        {
            SectorManager.OnSectorLoad -= SectorLoaded;

            state = ConditionState.Uninitialized;
        }
    }
}
