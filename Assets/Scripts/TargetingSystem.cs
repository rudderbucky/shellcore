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

    /// <summary>
    /// No argument constructor that sets the target to null
    /// </summary>
    public TargetingSystem() {
        // initialize instance fields
        target = null;
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
    /// <returns>The target of the targeting system</returns>
    public Transform GetTarget() {
        return target; // get target
    }
}
