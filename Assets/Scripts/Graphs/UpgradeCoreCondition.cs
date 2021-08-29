using UnityEngine;
using UnityEngine.Events;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/CoreUpgrade")]
    public class UpgradeCoreCondition : Node, ICondition
    {
        public static UnityEvent OnCoreUpgrade = new UnityEvent();
        public const string ID = "UpgradeCoreCondition";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Upgrade Core Condition"; }
        }

        public ConditionState state; // Property can't be serialized -> field

        public ConditionState State
        {
            get { return state; }
            set { state = value; }
        }

        [ConnectionKnob("Output", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob output;

        public string ShellID = "";

        public override void NodeGUI()
        {
            output.DisplayLayout();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Shell ID: ");
            ShellID = Utilities.RTEditorGUI.TextField(ShellID);
            GUILayout.EndHorizontal();
        }

        public void Init(int index)
        {
            OnCoreUpgrade.AddListener(CheckShell);
            State = ConditionState.Listening;
            CheckShell();
        }

        public void DeInit()
        {
            OnCoreUpgrade.RemoveListener(CheckShell);
            State = ConditionState.Uninitialized;
        }

        public void CheckShell()
        {
            if (PlayerCore.Instance.blueprint.coreShellSpriteID == ShellID)
            {
                State = ConditionState.Completed;
                connectionKnobs[0].connection(0).body.Calculate();
            }
        }
    }
}
