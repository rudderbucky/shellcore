using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "AI/Clear Dialogue")]
    public class ClearDialogueNode : Node
    {
        public override string GetName
        {
            get { return "ClearDialogue"; }
        }

        public override string Title
        {
            get { return "Clear Pending Dialogue"; }
        }

        public override Vector2 MinSize
        {
            get { return new Vector2(200f, 100f); }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        public override bool AllowRecursion
        {
            get { return true; }
        }

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputRight;
        public string EntityID;

        public NodeEditorGUI.NodeEditorState state;

        public override void NodeGUI()
        {
            GUILayout.Label("Entity ID");
            EntityID = GUILayout.TextField(EntityID);
        }

        public override int Traverse()
        {
            IDialogueOverrideHandler handler = null;
            if (state != NodeEditorGUI.NodeEditorState.Dialogue)
            {
                handler = TaskManager.Instance;
            }
            else
            {
                handler = DialogueSystem.Instance;
            }

            if (handler.GetInteractionOverrides().ContainsKey(EntityID))
            {
                handler.GetInteractionOverrides()[EntityID].Clear();
            }
            return 0;
        }
    }
}
