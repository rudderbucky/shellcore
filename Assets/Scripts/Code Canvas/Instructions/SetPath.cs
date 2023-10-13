using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CodeCanvasSequence;
using static CodeTraverser;

public class SetPath : MonoBehaviour
{
    public static void Execute(string entityID, bool rotateWhileMoving, float customMass, string flagName, Sequence sequence, Context context)
    {
        flagName = flagName.Trim();
        Debug.Log("Entity ID: " + entityID + ", Flag name: " + flagName + ", Flag array count: " + AIData.flags.Count);

        Vector2 coords = new Vector2();
        for (int i = 0; i < AIData.flags.Count; i++)
        {
            if (AIData.flags[i].name == flagName)
            {
                coords = AIData.flags[i].transform.position;
                break;
            }
        }

        for (int i = 0; i < AIData.entities.Count; i++)
        {
            if (AIData.entities[i].ID == entityID && AIData.entities[i] is AirCraft airCraft)
            {
                if (AIData.entities[i] is PlayerCore player)
                {
                    AIData.entities[i].StartCoroutine(pathPlayer(player, coords, sequence, context));
                }
                else
                {
                    AIData.entities[i].isPathing = false; // override any previous paths given to it immediately
                    NodeEditorFramework.Standard.PathData pathData = new NodeEditorFramework.Standard.PathData();
                    pathData.waypoints = new List<NodeEditorFramework.Standard.PathData.Node>();
                    var waypoint = new NodeEditorFramework.Standard.PathData.Node();
                    waypoint.position = coords;
                    airCraft.GetAI().setPath(pathData, () => 
                    {
                        if (sequence.instructions != null)
                        {
                            CodeCanvasSequence.RunSequence(sequence, context);
                        }
                    });

                    if (customMass >= 0)
                    {
                        AIData.entities[i].weight = customMass;
                    }

                    airCraft.rotateWhileMoving = rotateWhileMoving;
                }
            }
        }
    }

    private static IEnumerator pathPlayer(PlayerCore player, Vector2 coords, Sequence sequence, Context context)
    {
        player.SetIsInteracting(true);
        NodeEditorFramework.Standard.PathData pathData = new NodeEditorFramework.Standard.PathData();
        pathData.waypoints = new List<NodeEditorFramework.Standard.PathData.Node>();
        var waypoint = new NodeEditorFramework.Standard.PathData.Node();
        waypoint.position = coords;

        Vector2 delta = coords - (Vector2)player.transform.position - player.GetComponent<Rigidbody2D>().velocity * Time.fixedDeltaTime;
        while (delta.sqrMagnitude > PathAI.minDist)
        {
            delta = coords - (Vector2)player.transform.position - player.GetComponent<Rigidbody2D>().velocity * Time.fixedDeltaTime;
            player.MoveCraft(delta.normalized);
            yield return null;
        }

        player.SetIsInteracting(false);

        if (sequence.instructions != null)
        {
            CodeCanvasSequence.RunSequence(sequence, context);
        }
    }
}
