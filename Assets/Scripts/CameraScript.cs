using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scripts for camera
/// </summary>
public class CameraScript : MonoBehaviour {

    // TODO: Arrow indicating target position from TargetingSystem if not on camera

    public PlayerCore core; // the target for the camera to follow
    public void Start()
    {
        Vector3 goalPos = core.transform.position; // update vector
        goalPos.z = core.transform.position.z - 10; // maintain z axis difference
        transform.position = goalPos; // set position
    }

    private void Update()
    {
        
        if (core.IsMoving()) // lock camera
        {
            Vector3 goalPos = core.transform.position; // update vector
            goalPos.z = core.transform.position.z - 10; // maintain z axis difference
            transform.position = goalPos; // set position
        }
    }
}
