using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scripts for camera
/// </summary>
public class CameraScript : MonoBehaviour {

    // TODO: Arrow indicating target position from TargetingSystem if not on camera
    private PlayerCore core; // the target for the camera to follow
    private bool initialized;
    public SFXHandler sFXHandler;
    public void Initialize(PlayerCore player)
    {
        core = player;
        if(!initialized) sFXHandler.Initialize();
        initialized = true;
        Start();
    }
    public void Start()
    {
        if (core)
        {
            Vector3 goalPos = core.transform.position; // update vector
            goalPos.z = core.transform.position.z - 10; // maintain z axis difference
            transform.position = goalPos; // set position
        }
    }

    private void LateUpdate()
    {
     if(initialized)
        {
            if (core.IsMoving()) // lock camera
            {
                Focus();
            }

        }
    }

    public void Focus() {
        Vector3 goalPos = core.transform.position; // update vector
        goalPos.z = core.transform.position.z - 10; // maintain z axis difference
        transform.position = goalPos; // set position
    }
}
