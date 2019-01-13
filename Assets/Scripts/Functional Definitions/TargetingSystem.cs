using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used for the targeting of crafts for weapon abilities
/// </summary>
public class TargetingSystem {
    // TODO: Change transform to Vector3, and check if a core is dead by raycasting, 
    // and constantly update the position of the targeting system

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

            for (int i = 0; i < AirCraftAI.entities.Count; i++)
            {
                if (AirCraftAI.entities[i].transform == parent)
                    continue;
                if (parent.GetComponent<Entity>().faction == AirCraftAI.entities[i].faction)
                    continue;
                if (AirCraftAI.entities[i].GetIsDead())
                {
                    continue;
                }

                float sqrD = Vector3.SqrMagnitude(parent.position - AirCraftAI.entities[i].transform.position);
                if (closest == null || sqrD < closestD)
                {
                    closestD = sqrD;
                    closest = AirCraftAI.entities[i].transform;
                }
            }
            target = closest;
        }

        return target; // get target
    }
}
