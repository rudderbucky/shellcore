using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystem {
    // TODO: Change transform to Vector3, and check if a core is dead by raycasting, 
    // and constantly update the position of the targeting system
    private Transform target;

    public TargetingSystem() {
        target = null;
    }

    public void SetTarget(Transform target) {
        this.target = target;
    }

    public Transform GetTarget() {
        return target;
    }
}
