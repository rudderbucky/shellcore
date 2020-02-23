using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Cutscenes/FinishCutscene")]
    public class FinishCutsceneNode : Node
    {
        //Node things
        public const string ID = "FinishCutsceneNode";
        public override string GetName { get { return ID; } }

        public override string Title { get { return "Finish Cutscene"; } }
        public override Vector2 DefaultSize { get { return new Vector2(208, height); } }

        float height = 50f;

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputRight;

        [ConnectionKnob("Output Up", Direction.Out, "CutsceneComplete", NodeSide.Top, 100f)]
        public ConnectionKnob inputUp;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
        }

        public override int Traverse()
        {
            DialogueSystem.isInCutscene = false;
            PlayerCore.Instance.SetIsInteracting(false);
            DialogueSystem.Instance.DialogueViewTransitionOut();
            return 0;
        }
    }
}