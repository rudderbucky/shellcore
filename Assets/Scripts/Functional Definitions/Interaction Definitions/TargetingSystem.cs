using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used for the targeting of crafts for weapon abilities
/// </summary>
public class TargetingSystem {

    private Transform target; // the transform of the target
    public Transform parent; // parent object

    /// <summary>
    /// Constructor that sets the target to null and takes a transform from which distances are calculate from
    /// </summary>
    public TargetingSystem(Transform parent) {
        // initialize instance fields
        target = null;
        this.parent = parent;
    }

    /// <summary>
    /// Set the target of the targeting system
    /// </summary>
    /// <param name="target">The target to set to</param>
    public void SetTarget(Transform target) {
        this.target = target; // set target
    }

    /// <summary>
    /// Get the target of the targeting system
    /// </summary>
    /// <param name="findNew">Whether or not the targeting system should find a new target</param>
    /// <returns>The target of the targeting system</returns>
    public Transform GetTarget(bool findNew = false) {
        if(findNew)
        {
            //Find the closest enemy
            //TODO: optimize
            Transform closest = null;
            float closestD = float.MaxValue;

            for (int i = 0; i < AIData.entities.Count; i++)
            {
                if(!AIData.entities[i]) continue;
                if (AIData.entities[i].transform == parent)
                    continue;
                if (parent.GetComponent<Entity>().faction == AIData.entities[i].faction)
                    continue;
                if (AIData.entities[i].GetIsDead())
                {
                    continue;
                }

                float sqrD = Vector3.SqrMagnitude(parent.position - AIData.entities[i].transform.position);
                if (closest == null || sqrD < closestD)
                {
                    closestD = sqrD;
                    closest = AIData.entities[i].transform;
                }
            }
            target = closest;
        }

        return target; // get target
    }

    private List<Entity> secondaryTargets = new List<Entity>();

    public bool AddSecondaryTarget(Entity ent)
    {
        if(!secondaryTargets.Contains(ent))
        {
            secondaryTargets.Add(ent);
            return true;
        }
        return false;
    }

    public void RemoveSecondaryTarget(Entity ent)
    {
        if(secondaryTargets.Contains(ent))
            secondaryTargets.Remove(ent);
    }

    public List<Entity> GetSecondaryTargets()
    {
        return secondaryTargets;
    }

    public void ClearSecondaryTargets()
    {
        secondaryTargets.Clear();
    }
}
