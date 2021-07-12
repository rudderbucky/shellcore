using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    static TargetManager Instance;

    //Dictionary<(int, int), Transform> targets = new Dictionary<(int, int), Transform>();

    List<(ITargetingSystem, Entity.EntityCategory)> targetSearchQueries = new List<(ITargetingSystem, Entity.EntityCategory)>();

    Dictionary<int, List<Entity>> groundTargets;
    Dictionary<int, List<Entity>> airTargets;
    Vector2[] positions;
    int[] factions;
    bool trUpdated = false;

    private void Awake()
    {
        Instance = this;
    }

    public static void Enqueue(ITargetingSystem targetingSystem, Entity.EntityCategory targetCategory = Entity.EntityCategory.All)
    {
        if (Instance)
        {
            Instance.enqueue(targetingSystem, targetCategory);
        }
    }

    void enqueue(ITargetingSystem targetingSystem, Entity.EntityCategory targetCategory)
    {
        if (targetSearchQueries.FindIndex((x) => x.Item1 == targetingSystem) == -1)
        {
            targetSearchQueries.Add((targetingSystem, targetCategory));
        }
    }

    private void Update()
    {
        if (!trUpdated)
        {
            UpdateTargets();
        }

        for (int i = 0; i < targetSearchQueries.Count; i++)
        {
            var ts = targetSearchQueries[i];
            if (ts.Item1.GetEntity())
            {
                GetTarget(ts.Item1, ts.Item2);
            }
            else
            {
                targetSearchQueries.Remove(ts);
                i--;
            }
        }

        targetSearchQueries.Clear();

        UpdateColliders();
    }

    private void LateUpdate()
    {
        trUpdated = false;
    }

    void UpdateColliders()
    {
        for (int i = 0; i < positions.Length; i++)
        {
            bool colliderNear = false;
            for (int j = 0; j < positions.Length; j++)
            {
                if (i == j)
                {
                    continue;
                }

                if ((positions[i] - positions[j]).sqrMagnitude < 200 && !FactionManager.IsAllied(factions[i], factions[j]))
                {
                    colliderNear = true;
                    break;
                }
            }

            AIData.entities[i].ToggleColliders(colliderNear);
        }
    }

    void UpdateTargets()
    {
        airTargets = new Dictionary<int, List<Entity>>();
        groundTargets = new Dictionary<int, List<Entity>>();
        positions = new Vector2[AIData.entities.Count];
        factions = new int[AIData.entities.Count];

        for (int i = 0; i < AIData.entities.Count; i++)
        {
            positions[i] = AIData.entities[i].transform.position;
            factions[i] = AIData.entities[i].faction;

            if (AIData.entities[i].GetIsDead() || AIData.entities[i].IsInvisible)
            {
                continue;
            }

            int faction = AIData.entities[i].faction;
            if ((AIData.entities[i].GetTerrain() & Entity.TerrainType.Air) != 0)
            {
                if (!airTargets.ContainsKey(faction))
                {
                    airTargets[faction] = new List<Entity>();
                }

                airTargets[faction].Add(AIData.entities[i]);
            }

            if ((AIData.entities[i].GetTerrain() & Entity.TerrainType.Ground) != 0)
            {
                if (!groundTargets.ContainsKey(faction))
                {
                    groundTargets[faction] = new List<Entity>();
                }

                groundTargets[faction].Add(AIData.entities[i]);
            }
        }

        trUpdated = true;
    }

    public static Transform GetTargetImmediate(ITargetingSystem ts, Entity.EntityCategory ec = Entity.EntityCategory.All)
    {
        return Instance.GetTarget(ts, ec);
    }

    public static List<Entity> GetTargetList(ITargetingSystem ts, Entity.EntityCategory ec)
    {
        return Instance.getTargetList(ts, ec);
    }

    public static Transform GetClosestFromList(List<Entity> targets, ITargetingSystem ts, Entity.EntityCategory ec)
    {
        return Instance.getClosestFromList(targets, ts, ec);
    }

    private List<Entity> getTargetList(ITargetingSystem ts, Entity.EntityCategory ec)
    {
        List<Entity> targets = new List<Entity>();
        for (int i = 0; i < FactionManager.FactionArrayLength; i++)
        {
            if (FactionManager.IsAllied(ts.GetEntity().faction, i) || !FactionManager.FactionExists(i))
            {
                continue;
            }

            if (ts.GetAbility() == null)
            {
                if (airTargets.ContainsKey(i))
                {
                    targets.AddRange(airTargets[i]);
                }

                if (groundTargets.ContainsKey(i))
                {
                    targets.AddRange(groundTargets[i]);
                }
            }
            else
            {
                if (ts.GetAbility().TerrainCheck(Entity.TerrainType.Air)
                    && airTargets.ContainsKey(i))
                {
                    targets.AddRange(airTargets[i]);
                }

                if (ts.GetAbility().TerrainCheck(Entity.TerrainType.Ground)
                    && groundTargets.ContainsKey(i))
                {
                    targets.AddRange(groundTargets[i]);
                }
            }
        }

        return targets;
    }

    private Transform getClosestFromList(List<Entity> targets, ITargetingSystem ts, Entity.EntityCategory ec)
    {
        Transform closest = null;
        float closestD = float.MaxValue;
        var pos = ts.GetAbility() ? ts.GetAbility().transform.position : ts.GetEntity().transform.position;

        for (int i = 0; i < targets.Count; i++) // go through all entities and check them for several factors
        {
            // check if the target's category matches
            if (ec == Entity.EntityCategory.All || targets[i].category == ec)
            {
                // check if it is the closest entity that passed the checks so far
                float sqrD = Vector3.SqrMagnitude(pos - targets[i].transform.position);
                if (closest == null || sqrD < closestD)
                {
                    if (targets[i] == ts.GetEntity())
                    {
                        continue;
                    }

                    var ability = ts.GetAbility();
                    if (ability != null && sqrD >= ability.GetRange() * ability.GetRange())
                    {
                        continue;
                    }

                    closestD = sqrD;
                    closest = targets[i].transform;
                }
            }
        }

        return closest;
    }

    Transform GetTarget(ITargetingSystem ts, Entity.EntityCategory ec)
    {
        if (!trUpdated)
        {
            UpdateTargets();
        }

        //Find the closest enemy


        List<Entity> targets = getTargetList(ts, ec);

        var closest = getClosestFromList(targets, ts, ec);
        // set to the closest compatible target
        ts.SetTarget(closest);
        return closest;
    }
}
