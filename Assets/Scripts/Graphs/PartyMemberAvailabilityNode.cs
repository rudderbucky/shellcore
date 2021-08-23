using NodeEditorFramework.Utilities;
using UnityEngine;
namespace NodeEditorFramework.Standard
{

    // Node prevents assignment of party members. Saves which members are disabled
    [Node(false, "Flow/Adjust Party Member Availability")]
    public class PartyMemberAvailabilityNode : Node
    {
        public static string LimitedSector;
        //public static SectorLimiterNode StartPoint;

        //Node things
        public const string ID = "PartyMemberAvailability";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Adjust Party Member"; }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        public string entityID;
        public bool disableMember;

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left, 20)]
        public ConnectionKnob input;

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right, 20)]
        public ConnectionKnob output;

        public override void NodeGUI()
        {
            GUILayout.Label("Party Member:");
            entityID = GUILayout.TextField(entityID, GUILayout.Width(200f));
            disableMember = RTEditorGUI.Toggle(disableMember, "Disable as party member");
        }

        public override int Traverse()
        {
            if (disableMember && PlayerCore.Instance && PlayerCore.Instance.cursave != null)
            {
                if (disableMember)
                {
                    if (!PlayerCore.Instance.cursave.disabledPartyIDs.Contains(entityID))
                    {
                        PlayerCore.Instance.cursave.disabledPartyIDs.Add(entityID);
                    }
                    if (PartyManager.instance && PartyManager.instance.partyMembers.Exists(x => x.ID == entityID))
                    {
                        PartyManager.instance.Unassign(entityID);
                    }
                }
                
            }
            else if (PlayerCore.Instance.cursave.disabledPartyIDs.Contains(entityID) && PlayerCore.Instance && PlayerCore.Instance.cursave != null)
                {
                    PlayerCore.Instance.cursave.disabledPartyIDs.Remove(entityID);
                    PartyManager.instance.UpdatePortraits();
                } 
            return 0;
        }
    }
}