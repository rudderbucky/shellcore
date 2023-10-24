using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Party : MonoBehaviour
{

    public static void ClearParty(bool deletePartyMembers)
    {
        PartyManager.instance.ClearParty(deletePartyMembers);
    }

    public static void RegisterPartyMember(string entityID)
    {
        if (!PlayerCore.Instance.cursave.unlockedPartyIDs.Contains(entityID))
        {
            PlayerCore.Instance.cursave.unlockedPartyIDs.Add(entityID);
        }
    }

    public static void AddPartyMember(string entityID)
    {
        PartyManager.instance.AssignBackend(entityID);
    }

    public static void RemovePartyMember(string entityID)
    {
        if (PartyManager.instance && PartyManager.instance.partyMembers.Exists(x => x.ID == entityID))
        {
            PartyManager.instance.Unassign(entityID);
        }
    }

    public static void SetPartyMemberEnabled(string entityID, bool enabled)
    {
        if (!enabled)
        {
            if (!PlayerCore.Instance.cursave.disabledPartyIDs.Contains(entityID))
            {
                PlayerCore.Instance.cursave.disabledPartyIDs.Add(entityID);
            }
        }
        else if (PlayerCore.Instance.cursave.disabledPartyIDs.Contains(entityID))
        {
            PlayerCore.Instance.cursave.disabledPartyIDs.Remove(entityID);
        }
        PartyManager.instance.UpdatePortraits();
    }
}
