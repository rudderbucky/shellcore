using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Actions/Set Faction Relations")]
    public class SetFactionRelations : Node
    {
        //Node things
        public const string ID = "SetFactionRelations";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Set Faction Relations"; }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(200, 150); }
        }

        public override bool ContinueCalculation
        {
            get { return true; }
        }

        //Data
        public int factionID;
        public int relationsSum;

        ConnectionKnobAttribute flowIn = new ConnectionKnobAttribute("Input ", Direction.In, "TaskFlow", ConnectionCount.Multi, NodeSide.Left, 20);
        ConnectionKnobAttribute flowOut = new ConnectionKnobAttribute("Output ", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right, 20);

        ConnectionKnobAttribute dialogueIn = new ConnectionKnobAttribute("Input ", Direction.In, "Dialogue", ConnectionCount.Multi, NodeSide.Left, 20);
        ConnectionKnobAttribute dialogueOut = new ConnectionKnobAttribute("Output ", Direction.Out, "Dialogue", ConnectionCount.Single, NodeSide.Right, 20);

        ConnectionKnob input;
        ConnectionKnob output;

        public override void NodeGUI()
        {
            if (input == null)
            {
                if (inputKnobs.Count > 0)
                {
                    input = inputKnobs[0];
                    output = outputKnobs[0];
                }
                else if (Canvas is DialogueCanvas)
                {
                    input = CreateConnectionKnob(dialogueIn);
                    output = CreateConnectionKnob(dialogueOut);
                }
                else
                {
                    input = CreateConnectionKnob(flowIn);
                    output = CreateConnectionKnob(flowOut);
                }
            }

            {
                GUILayout.BeginHorizontal();
                inputKnobs[0].DisplayLayout();
                outputKnobs[0].DisplayLayout();
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("Faction Number");
            factionID = RTEditorGUI.IntField(factionID);
            if (factionID < 0)
            {
                factionID = RTEditorGUI.IntField(0);
                Debug.LogWarning("This identification does not exist!");
            }
            GUILayout.Label("Relations sum:");
            relationsSum = RTEditorGUI.IntField(relationsSum);
        }

        public override int Traverse()
        {
            FactionManager.SetFactionRelations(factionID, relationsSum);
            return 0;
        }
    }
}
