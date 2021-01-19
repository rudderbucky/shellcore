using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/PartyManagement")]
    public class PartyManagementNode : Node
    {
        public static string LimitedSector;
        //public static SectorLimiterNode StartPoint;

        //Node things
        public const string ID = "PartyManagementNode";
        public override string GetName { get { return ID; } }

        public override string Title { get { return "Party Manager"; } }
        public override Vector2 DefaultSize { get { return new Vector2(208, height); } }

        public bool clearParty;
        public string entityID;
        float height = 40f;

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left, 20)]
        public ConnectionKnob input;

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right, 20)]
        public ConnectionKnob output;

        public override void NodeGUI()
        {
            height = 40f;
            clearParty = RTEditorGUI.Toggle(clearParty, "Clear Party");
            if(!clearParty)
            {
                height = 84f;
                GUILayout.Label("Add ShellCore by ID:");
                entityID = GUILayout.TextField(entityID, GUILayout.Width(200f));
            }
            else
            {
                entityID = "";
            }
        }

        public override int Traverse()
        {
            if(clearParty) PartyManager.instance.partyMembers.Clear();
            else 
            {
                if(!PlayerCore.Instance.cursave.unlockedPartyIDs.Contains(entityID))
                    PlayerCore.Instance.cursave.unlockedPartyIDs.Add(entityID);
                PartyManager.instance.AssignBackend(entityID);
            }
            return 0;
        }
    }
}