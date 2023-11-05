using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CoreScriptsManager;
using static CoreScriptsSequence;

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

    public static void StartCameraPan(Vector3 coordinates, bool useCoordinates, 
        string flagName, float velocityFactor, Sequence sequence, Context context)
    {
        Vector3 coords = coordinates;
        if (!useCoordinates)
        {
            for (int i = 0; i < AIData.flags.Count; i++)
            {
                if (AIData.flags[i].name == flagName)
                {
                    coords = AIData.flags[i].transform.position;
                    break;
                }
            }
        }

        CameraScript.velocityFactor = velocityFactor;
        coords.z = -CameraScript.zLevel;
        CameraScript.panning = true;
        CameraScript.target = coords;
        if (sequence.instructions != null)
        {
            CameraScript.callback = () => CoreScriptsSequence.RunSequence(sequence, context);
        }

    }

    public static void EndCameraPan()
    {
        CameraScript.panning = false;
        CameraScript.instance.Focus(PlayerCore.Instance.transform.position);
        foreach (var rect in RectangleEffectScript.instances)
        {
            if (rect)
            {
                rect.Start();
            }
        }
    }

    public static void FadeIntoBlack()
    {
        DialogueSystem.Instance.FadeInScreenBlack();
    }

    public static void FadeOutOfBlack()
    {
        DialogueSystem.Instance.FadeOutScreenBlack();
    }
}
