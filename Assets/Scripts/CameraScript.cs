using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scripts for camera
/// </summary>
public class CameraScript : MonoBehaviour {

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
            //GetComponent<Rigidbody2D>().velocity = core.GetComponent<Rigidbody2D>().velocity; // set velocity
            // camera's drag and mass should always be the same as the target's, otherwise the velocity's will desync
            // possible fix to this: keep the momentums equal instead of velocity, but idk
        }
    }
}
