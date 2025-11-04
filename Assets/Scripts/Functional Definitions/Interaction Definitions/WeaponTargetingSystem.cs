using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The targeting system of each weapon ability
/// </summary>
public class WeaponTargetingSystem : ITargetingSystem
{
    public WeaponAbility ability; // owner ability of the targeting system
    public Transform target; // target of the targeting system

    /// <summary>
    /// Get the target of the targeting system
    /// </summary>
    /// <param name="findNew">Whether or not the targeting system should find a new target</param>
    /// <returns>The target of the targeting system</returns>
    public Transform GetTarget()
    {
        Transform tmp = ability?.Core?.GetTargetingSystem()?.GetTarget(); // get the core's target if it has one

        if (tmp != null && tmp && IsValidTarget(tmp))
        {
            target = tmp;
            return target; // if the manual target is compatible it overrides everything
        }

        if (!IsValidTarget(target))
        {
            TargetManager.Enqueue(this, ability.category);
            return null;
        }

        return target; // return the target
    }

    // checks for: if it is the same faction as the ability entity, 
    // if it's dead, if it is weapon-compatible, if it is invisible
    bool IsValidTarget(Transform t)
    {
        if (t == null || !t || !ability || !ability.Core)
        {
            return false;
        }

        IDamageable damageable = t.GetComponent<IDamageable>();
        return (damageable != null
                && !damageable.GetIsDead()
                && damageable.GetTransform() != ability.Core.GetTransform()
                && ability.Core != damageable as Entity
                && !FactionManager.IsAllied(damageable.GetFaction(), ability.Core.faction)
                && ability.CheckCategoryCompatibility(damageable)
                && (t.position - ability.transform.position).magnitude <= ability.GetRange()
                && !damageable.GetInvisible());
    }

    // Position override for chain beam
    bool IsValidTarget(Transform t, Vector2 lookupPos)
    {
        if (t == null || !t || !ability || !ability.Core)
        {
            return false;
        }

        IDamageable damageable = t.GetComponent<IDamageable>();
        return (damageable != null
                && !damageable.GetIsDead()
                && damageable.GetTransform() != ability.Core.GetTransform()
                && ability.Core != damageable as Entity
                && !FactionManager.IsAllied(damageable.GetFaction(), ability.Core.faction)
                && ability.CheckCategoryCompatibility(damageable)
                && ((Vector2)t.position - lookupPos).magnitude <= ability.GetRange()
                && !damageable.GetInvisible());
    }

    public Entity GetEntity()
    {
        return (ability && ability.Core) ? ability.Core : null;
    }

    public WeaponAbility GetAbility()
    {
        return ability;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    public virtual Transform[] GetClosestTargets(int num, Vector3 pos, bool dronesAreFree = false)
    {
        // Just get the N closest entities, the complexity is just O(N) instead of sorting which would be O(NlogN)

        Entity[] potentialTargets = TargetManager.GetTargetArray(this, ability.category, out var count);
        List<Transform> targets = new();
        List<Transform> drones = new();
        List<float> closestD = new();

        for (int i = 0; i < count; i++) // go through all entities and check them for several factors
        {
            Entity target = potentialTargets[i];

            Transform tr = target.transform;

            // Check if the target can be shot at
            if (!IsValidTarget(tr, pos))
            {
                continue;
            }

            // check if it is the closest entity that passed the checks so far
            float sqrD = Vector3.SqrMagnitude(pos - tr.position);

            if (dronesAreFree && target is Drone)
            {
                drones.Add(tr);
                continue;
            }

            if (targets.Count == 0)
            {
                targets.Add(tr);
                closestD.Add(sqrD);
                continue;
            }

            bool added = false;
            for (int j = 0; j < targets.Count; j++)
            {
                if (sqrD >= closestD[j])
                {
                    continue;
                }

                targets.Insert(j, tr);
                closestD.Insert(j, sqrD);

                if (targets.Count > num)
                {
                    targets.RemoveAt(targets.Count - 1);
                    closestD.RemoveAt(targets.Count - 1);
                }
                added = true;
                break;
            }

            if (!added && targets.Count < num)
            {
                targets.Add(tr);
                closestD.Add(sqrD);
            }
        }
        targets.AddRange(drones);
        return targets.ToArray();
    }

    public Transform[] GetClosestTargets(int num, bool dronesAreFree = false)
    {
        return GetClosestTargets(num, GetEntity().transform.position, dronesAreFree);
    }
}
