using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Cutscenes/Start Cutscene")]
    public class StartCutsceneNode : Node
    {
        //Node things
        public const string ID = "StartCutsceneNode";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Start Cutscene"; }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(208, height); }
        }

        float height = 50f;

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob inputLeft;

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputRight;

        [ConnectionKnob("Input Up", Direction.In, "CutsceneComplete", NodeSide.Top, 100f)]
        public ConnectionKnob inputUp;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
        }

        public override int Traverse()
        {
            PlayerCore.Instance.SetIsInteracting(true);
            DialogueSystem.Instance.FadeBarIn();
            DialogueSystem.isInCutscene = true;
            return 0;
        }
    }
}
