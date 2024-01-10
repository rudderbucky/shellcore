using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CoreScriptsManager;
using static CoreScriptsSequence;

public class Mobility : MonoBehaviour
{

    public static void SetPath(string entityID, bool rotateWhileMoving, float customMass, string flagName, string targetEntityID, Sequence sequence, Context context)
    {
        Vector2 coords = new Vector2();

        if (flagName != null)
        {
            flagName = flagName.Trim();
            Debug.Log("<Set Path> Entity ID: " + entityID + ", Flag name: " + flagName + ", Flag array count: " + AIData.flags.Count);

            for (int i = 0; i < AIData.flags.Count; i++)
            {
                if (AIData.flags[i].name == flagName)
                {
                    coords = AIData.flags[i].transform.position;
                    break;
                }
            }
        }

        if (coords == Vector2.zero)
        {
            for (int i = 0; i < AIData.entities.Count; i++)
            {
                if (AIData.entities[i].ID == targetEntityID)
                {
                    coords = AIData.entities[i].transform.position;
                    break;
                }
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
                    pathData.waypoints.Add(waypoint);
                    waypoint.children = new List<int>();
                    airCraft.GetAI().setPath(pathData, () => 
                    {
                        if (sequence.instructions != null)
                        {
                            CoreScriptsSequence.RunSequence(sequence, context);
                        }
                    });

                    if (customMass >= 0)
                    {
                        AIData.entities[i].weight = customMass;
                        if (AIData.entities[i] is Craft craft) craft.CalculatePhysicsConstants();
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
            CoreScriptsSequence.RunSequence(sequence, context);
        }
    }


    public static void Rotate(string entityID, string targetID, string angle, Sequence sequence, Context context)
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
                    entity.transform.rotation = Quaternion.identity;
                    entity.transform.RotateAround(entity.transform.position, Vector3.forward, Vector3.SignedAngle(Vector3.up, targetVector, Vector3.forward));
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
        float lastDelta = 0;
        while (delta > 0.0001F)
        {
            player.RotateCraft(targetVector);
            delta = Mathf.Abs(Vector2.Dot(player.transform.up, normalizedTarget) - 1f);
            // give up if the rotate didn't do anything
            if (lastDelta == delta) break;
            lastDelta = delta;
            yield return null;
        }

        player.SetIsInteracting(false);

        if (sequence.instructions != null)
        {
            CoreScriptsSequence.RunSequence(sequence, context);
        }
    }

    public static void ForceTractor(string entityID, string targetEntityID)
    {
        Entity target = null;
        Entity entity = null;
        Debug.Log("Entity ID: " + entityID);
        Debug.Log("Target ID: " + targetEntityID);

        foreach (var ent in AIData.entities)
        {
            if (ent.ID == entityID)
            {
                entity = ent;
            }

            if (ent.ID == targetEntityID)
            {
                target = ent;
            }
        }

        if (!entity.GetComponent<TractorBeam>())
        {
            var beam = entity.gameObject.AddComponent<TractorBeam>();
            beam.owner = entity;
            beam.BuildTractor();
        }
        if (entity && entity.GetComponent<TractorBeam>() && target)
        {
            entity.GetComponentInChildren<TractorBeam>().ForceTarget(target.transform);
        }
        else if (entity && entity.GetComponent<TractorBeam>())
        {
            entity.GetComponentInChildren<TractorBeam>().ForceTarget(null);
        }
        else
        {
            Debug.LogError(entity + " " + entity.GetComponentInChildren<TractorBeam>());
        }
    }

    public static void WarpPlayer(string sectorName, string entityID, string flagName)
    {
        if (!string.IsNullOrEmpty(flagName)) entityID = flagName;
        Flag.FindEntityAndWarpPlayer(sectorName, entityID, !string.IsNullOrEmpty(flagName));
    }

    public static void Follow(string entityID, string targetEntityID, bool stopFollowing, bool disallowAggression)
    {
        if (!stopFollowing)
        {
            Entity target = SectorManager.instance.GetEntity(targetEntityID);
            if (target != null)
            {
                for (int i = 0; i < AIData.entities.Count; i++)
                {
                    if (AIData.entities[i].ID == entityID && AIData.entities[i] is AirCraft airCraft)
                    {
                        airCraft.GetAI().follow(target.transform);


                        if (disallowAggression)
                        {
                            airCraft.GetAI().aggression = AirCraftAI.AIAggression.KeepMoving;
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("Follow target not found!");
            }
        }
        else
        {
            for (int i = 0; i < AIData.entities.Count; i++)
            {
                if (AIData.entities[i].ID == entityID && AIData.entities[i] is AirCraft airCraft)
                {
                    airCraft.GetAI().follow(null);
                }
            }
        }
    }
}
