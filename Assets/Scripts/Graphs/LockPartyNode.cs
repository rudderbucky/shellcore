using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Lock Party Node")]
    public class LockPartyNode : Node
    {
        public const string ID = "LockParty";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Lock Party"; }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public bool unlockValue;

        public override void NodeGUI()
        {
            unlockValue = RTEditorGUI.Toggle(unlockValue, "Unlock Party");
        }

        public override int Traverse()
        {
            PartyManager.instance.SetOverrideLock(!unlockValue);
            return 0;
        }
    }
}
