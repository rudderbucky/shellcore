using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CoreScriptsSequence;
using static CoreScriptsManager;

public class Rotate : MonoBehaviour
{
    public static void Execute(string entityID, string targetID, string angle, Sequence sequence, Context context)
    {
        Debug.Log("Rotate - Entity ID: " + entityID + ", Target ID: " + targetID);
        Transform target = null;
        Entity entity = null;

        for (int i = 0; i < AIData.entities.Count; i++)
        {
            if (!AIData.entities[i]) continue;
            if (AIData.entities[i].ID == entityID)
            {
                entity = AIData.entities[i];
            }

            if (AIData.entities[i].ID == targetID)
            {
                target = AIData.entities[i].transform;
            }
        }
        if (!target)
        {
            foreach (var flag in AIData.flags)
            {
                if (flag.name != targetID) continue;
                target = flag.transform;
            }
        }

        if (angle == null)
        {
            if (!(target && entity))
            {
                Debug.LogWarning($"Could not find target/entity! {target} {entity}");
                return;
            }

            Vector2 targetVector = target.position - entity.transform.position;
            //calculate difference of angles and compare them to find the correct turning direction
            if (!(entity is PlayerCore))
            {
                if (entity is AirCraft airCraft)
                {
                    airCraft.GetAI().RotateTo(targetVector, () =>
                    {
                        if (sequence.instructions != null)
                        {
                            CoreScriptsSequence.RunSequence(sequence, context);
                        }
                    });
                }
                else
                {
                    entity.transform.RotateAround(entity.transform.position, Vector3.forward, Vector3.SignedAngle(Vector3.up, -targetVector, Vector3.forward));
                    return;
                }
            }
            else
            {
                entity.StartCoroutine(rotatePlayer(targetVector, sequence, context));
            }
        }
        else
        {
            entity.transform.rotation = Quaternion.Euler(new Vector3(0, 0, float.Parse(angle)));
            return;
        }
    }

    private static IEnumerator rotatePlayer(Vector2 targetVector, Sequence sequence, Context context)
    {
        var player = PlayerCore.Instance;
        player.SetIsInteracting(true);

        Vector2 normalizedTarget = targetVector.normalized;
        float delta = Mathf.Abs(Vector2.Dot(player.transform.up, normalizedTarget) - 1f);
        while (delta > 0.0001F)
        {
            player.RotateCraft(targetVector);
            delta = Mathf.Abs(Vector2.Dot(player.transform.up, normalizedTarget) - 1f);
            yield return null;
        }

        player.SetIsInteracting(false);

        if (sequence.instructions != null)
        {
            CoreScriptsSequence.RunSequence(sequence, context);
        }
    }
}
