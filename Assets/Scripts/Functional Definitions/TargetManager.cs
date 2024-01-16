using System;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    static TargetManager Instance;

    //Dictionary<(int, int), Transform> targets = new Dictionary<(int, int), Transform>();

    List<(ITargetingSystem, Entity.EntityCategory)> targetSearchQueries = new List<(ITargetingSystem, Entity.EntityCategory)>();

    //Dictionary<int, List<Entity>> groundTargets = new();
    //Dictionary<int, List<Entity>> airTargets = new();
    bool trUpdated = false;

    Entity[][] groundTargets;
    Entity[][] airTargets;
    Entity[][] allTargets;
    public int[] groundCount;
    public int[] airCount;
    public int[] allCount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        groundTargets = new Entity[FactionManager.FactionArrayLength][];
        airTargets = new Entity[FactionManager.FactionArrayLength][];
        allTargets = new Entity[FactionManager.FactionArrayLength][];

        groundCount = new int[FactionManager.FactionArrayLength];
        airCount = new int[FactionManager.FactionArrayLength];
        allCount = new int[FactionManager.FactionArrayLength];

        for (int i = 0; i < FactionManager.FactionArrayLength; i++)
        {
            groundTargets[i] = new Entity[1024];
            airTargets[i] = new Entity[1024];
            allTargets[i] = new Entity[1024];
        }
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
    }

    private void LateUpdate()
    {
        trUpdated = false;
    }

    void UpdateTargets()
    {
        Array.Fill(groundCount, 0);
        Array.Fill(airCount, 0);
        Array.Fill(allCount, 0);

        bool countSameFactionTargets = false;
        foreach (var ent in AIData.entities)
        {
            if (ent.faction.overrideFaction != 0)
            {
                countSameFactionTargets = true;
                break;
            }
        }

        for (int i = 0; i < AIData.entities.Count; i++)
        {
            Entity ent = AIData.entities[i];
            if (ent.GetIsDead()
                || ent.IsInvisible)
            {
                continue;
            }
            int faction = AIData.entities[i].faction.factionID;

            for (int j = 0; j < FactionManager.FactionArrayLength; j++)
            {
                if (FactionManager.IsAllied(j, faction) && !countSameFactionTargets)
                    continue;
                if (!FactionManager.FactionExists(j))
                    continue;

                if (j >= allTargets.Length) continue;
                allTargets[j][allCount[j]++] = ent;

                if ((ent.GetTerrain() & Entity.TerrainType.Air) != 0)
                {
                    airTargets[j][airCount[j]++] = ent;
                }

                if ((AIData.entities[i].GetTerrain() & Entity.TerrainType.Ground) != 0)
                {
                    groundTargets[j][groundCount[j]++] = ent;
                }

                CheckArraySizes(j);
            }
        }

        trUpdated = true;
    }

    void CheckArraySizes(int faction)
    {
        if (allTargets[faction].Length == allCount[faction] - 1)
        {
            Entity[] newArray = new Entity[allTargets[faction].Length * 2];
            Array.Copy(allTargets[faction], newArray, allTargets[faction].Length);
            allTargets[faction] = newArray;
        }
        if (airTargets[faction].Length == airCount[faction] - 1)
        {
            Entity[] newArray = new Entity[airTargets[faction].Length * 2];
            Array.Copy(airTargets[faction], newArray, airTargets[faction].Length);
            airTargets[faction] = newArray;
        }
        if (groundTargets[faction].Length == groundCount[faction] - 1)
        {
            Entity[] newArray = new Entity[groundTargets[faction].Length * 2];
            Array.Copy(groundTargets[faction], newArray, groundTargets[faction].Length);
            groundTargets[faction] = newArray;
        }
    }

    public static Transform GetTargetImmediate(ITargetingSystem ts, Entity.EntityCategory ec = Entity.EntityCategory.All)
    {
        return Instance.GetTarget(ts, ec);
    }

    public static Entity[] GetTargetArray(ITargetingSystem ts, Entity.EntityCategory ec, out int count)
    {
        return Instance.getTargetList(ts, ec, out count);
    }

    public static Transform GetClosestFromList(Entity[] targets, Vector3 pos, ITargetingSystem ts, Entity.EntityCategory ec, int targetCount)
    {
        return Instance.getClosestFromList(targets, pos, ts, ts.GetAbility(), ec, targetCount);
    }

    public static Transform GetClosestFromList(Entity[] targets, ITargetingSystem ts, Entity.EntityCategory ec, int targetCount)
    {
        return Instance.getClosestFromList(targets, ts, ec, targetCount);
    }

    private Entity[] getTargetList(ITargetingSystem ts, Entity.EntityCategory ec, out int count)
    {
        if (ts == null || ts.Equals(null)) 
        {
            count = 0;
            return new Entity[0];
        }
        int faction = ts.GetEntity().faction.factionID;

        if (ts.GetAbility() == null)
        {
            count = allCount[faction];
            return allTargets[faction];
        }
        else
        {
            bool air = ts.GetAbility().TerrainCheck(Entity.TerrainType.Air);
            bool ground = ts.GetAbility().TerrainCheck(Entity.TerrainType.Ground);

            if (air && ground)
            {
                count = allCount[faction];
                return allTargets[faction];
            }
            if (air)
            {
                count = airCount[faction];
                return airTargets[faction];
            }
            if (ground)
            {
                count = groundCount[faction];
                return groundTargets[faction];
            }
        }
        Debug.LogWarning("No target array!");
        count = 0;
        return new Entity[] { };
    }

    private Transform  getClosestFromList(Entity[] targets, ITargetingSystem ts, Entity.EntityCategory ec, int targetCount)
    {
        var pos = ts.GetAbility() ? ts.GetAbility().transform.position : ts.GetEntity().transform.position;
        return getClosestFromList(targets, pos, ts, ts.GetAbility(), ec, targetCount);
    }

    public int GetTargetCount(ITargetingSystem ts, int faction)
    {
        bool ground = ts.GetAbility().TerrainCheck(Entity.TerrainType.Ground);
        bool air = ts.GetAbility().TerrainCheck(Entity.TerrainType.Air);

        if (ground && air)
            return allCount[faction];
        if (ground)
            return groundCount[faction];
        if (air)
            return airCount[faction];
        return 0;
    }

    private Transform getClosestFromList(Entity[] targets, Vector3 pos, ITargetingSystem ts, WeaponAbility tsAbility, Entity.EntityCategory ec, int targetCount)
    {
        Transform closest = null;
        float closestD = float.MaxValue;
        Entity tsEntity = ts.GetEntity();

        //int targetCount = GetTargetCount(ts, ts.GetEntity().faction);

        for (int i = 0; i < targetCount; i++) // go through all entities and check them for several factors
        {
            if (FactionManager.IsAllied(targets[i].faction, tsEntity.faction)) continue;
            // check if the target's category matches
            if (ec == Entity.EntityCategory.All || targets[i].Category == ec)
            {
                // check if it is the closest entity that passed the checks so far
                float sqrD = Vector3.SqrMagnitude(pos - targets[i].transform.position);
                if (closest == null || sqrD < closestD)
                {
                    if (targets[i] == tsEntity)
                    {
                        continue;
                    }

                    var ability = tsAbility;
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
        Entity[] targets = getTargetList(ts, ec, out var count);

        var closest = getClosestFromList(targets, ts, ec, count);
        // set to the closest compatible target
        ts.SetTarget(closest);
        return closest;
    }
}
