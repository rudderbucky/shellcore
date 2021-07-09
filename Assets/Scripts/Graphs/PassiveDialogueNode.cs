using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/Passive Dialogue Node")]
    public class PassiveDialogueNode : Node
    {
        public override string GetName
        {
            get { return "PassiveDialogueNode"; }
        }

        public override string Title
        {
            get { return "Passive Dialogue (Instant)"; }
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
        public ConnectionKnob input;

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right)]
        public ConnectionKnob output;

        ConnectionKnobAttribute OutStyle = new ConnectionKnobAttribute("Output", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right);
        public string text;
        public string id;
        public string soundType;
        public bool onlyShowIfInParty;

        public override void NodeGUI()
        {
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.Label("Text:");
            text = GUILayout.TextArea(text);
            GUILayout.Label("ID:");
            id = GUILayout.TextArea(id);
            GUILayout.Label("Sound Type:");
            soundType = GUILayout.TextArea(soundType);
            onlyShowIfInParty = GUILayout.Toggle(onlyShowIfInParty, "Only show if in party");
        }

        public override int Traverse()
        {
            if (!onlyShowIfInParty || (PartyManager.instance.partyMembers.Exists(sc => sc.ID == id)))
            {
                int soundIndex;
                bool success = int.TryParse(soundType, out soundIndex);
                if (!success)
                {
                    soundIndex = 1;
                }

                PassiveDialogueSystem.Instance.PushPassiveDialogue(id, text, soundIndex);
            }
            else
            {
                Debug.Log("Party member not found, not pushing dialogue");
            }

            return 0;
        }
    }
}
