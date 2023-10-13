using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene : MonoBehaviour
{

    public static void StartCutscene()
    {
        if (PlayerCore.Instance)
            PlayerCore.Instance.SetIsInteracting(true);
        DialogueSystem.Instance.FadeBarIn();
        DialogueSystem.isInCutscene = true;
    }

    public static void FinishCutscene()
    {
        if (PlayerCore.Instance)
            PlayerCore.Instance.SetIsInteracting(false);
        DialogueSystem.isInCutscene = false;
        DialogueSystem.Instance.DialogueViewTransitionOut();
    }
}
