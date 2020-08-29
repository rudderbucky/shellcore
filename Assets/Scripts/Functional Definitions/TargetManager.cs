using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
    static TargetManager Instance;

    //Dictionary<(int, int), Transform> targets = new Dictionary<(int, int), Transform>();

    List<ITargetingSystem> targetSearchQueries = new List<ITargetingSystem>();

    Dictionary<int, List<Entity>> groundTargets;
    Dictionary<int, List<Entity>> airTargets;
    Vector2[] positions;
    int[] factions;
    bool trUpdated = false;

    private void Awake()
    {
        Instance = this;
    }

    public static void Enqueue(ITargetingSystem targetingSystem)
    {
        Instance.enqueue(targetingSystem);
    }
    
    void enqueue(ITargetingSystem targetingSystem)
    {
        if (!targetSearchQueries.Contains(targetingSystem))
            targetSearchQueries.Add(targetingSystem);
    }

    private void Update()
    {
        if (!trUpdated)
            UpdateTargets();

        for (int i = 0; i < targetSearchQueries.Count; i++)
        {
            var ts = targetSearchQueries[i];
            if (ts.GetEntity())
            {
                GetTarget(ts);
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
                    continue;

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

            if (AIData.entities[i].GetIsDead()
                || AIData.entities[i].invisible)
            {
                continue;
            }
            int faction = AIData.entities[i].faction;
            if ((AIData.entities[i].GetTerrain() & Entity.TerrainType.Air) != 0)
            {
                if (!airTargets.ContainsKey(faction))
                    airTargets[faction] = new List<Entity>();
                airTargets[faction].Add(AIData.entities[i]);
            }
            if ((AIData.entities[i].GetTerrain() & Entity.TerrainType.Ground) != 0)
            {
                if (!groundTargets.ContainsKey(faction))
                    groundTargets[faction] = new List<Entity>();
                groundTargets[faction].Add(AIData.entities[i]);
            }
        }
        trUpdated = true;
    }

    public static Transform GetTargetImmediate(ITargetingSystem ts)
    {
        return Instance.GetTarget(ts);
    }

    Transform GetTarget(ITargetingSystem ts)
    {
        if (!trUpdated)
            UpdateTargets();
        
        //Find the closest enemy
        Transform closest = null;
        float closestD = float.MaxValue;
        var pos = ts.GetEntity().transform.position;

        List<Entity> targets = new List<Entity>();
        for (int i = 0; i < FactionManager.FactionCount; i++)
        {
            if (FactionManager.IsAllied(ts.GetEntity().faction, i))
                continue;

            if (ts.GetAbility() == null)
            {
                if (airTargets.ContainsKey(i))
                    targets.AddRange(airTargets[i]);
                if (groundTargets.ContainsKey(i))
                    targets.AddRange(groundTargets[i]);
            }
            else
            {
                if ((ts.GetAbility().terrain & Entity.TerrainType.Air) != 0
                    && airTargets.ContainsKey(i))
                {
                    targets.AddRange(airTargets[i]);
                }
                if ((ts.GetAbility().terrain & Entity.TerrainType.Ground) != 0
                    && groundTargets.ContainsKey(i))
                {
                    targets.AddRange(groundTargets[i]);
                }
            }
        }
        for (int i = 0; i < targets.Count; i++) // go through all entities and check them for several factors
        {
            // check if it is the closest entity that passed the checks so far
            float sqrD = Vector3.SqrMagnitude(pos - targets[i].transform.position);
            if (closest == null || sqrD < closestD)
            {
                if (targets[i] == ts.GetEntity())
                    continue;
                var ability = ts.GetAbility();
                if (ability != null && sqrD >= ability.GetRange() * ability.GetRange())
                    continue;

                closestD = sqrD;
                closest = targets[i].transform;
            }
        }
        // set to the closest compatible target
        ts.SetTarget(closest);
        return closest;
    }
}
