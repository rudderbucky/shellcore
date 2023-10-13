using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveDialogue : MonoBehaviour
{
    // TODO: Allow passive dialogue to inherit entity color
    public static void Execute(string id, string text, string soundType, bool onlyShowIfInParty)
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
    }
}
