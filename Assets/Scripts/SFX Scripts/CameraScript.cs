using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scripts for camera
/// </summary>
public class CameraScript : MonoBehaviour {

    public static CameraScript instance;
    public static UnityEngine.Events.UnityAction callback;
    private PlayerCore core; // the target for the camera to follow
    private bool initialized;
    public SFXHandler sFXHandler;

    public static bool panning;
    public static Vector3 target;
    public static float velocityFactor;

    public void Initialize(PlayerCore player)
    {
        core = player;
        if(!initialized) sFXHandler.Initialize();
        initialized = true;
        Start();
    }
    public void Start()
    {
        instance = this;
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
            if(panning) Pan();
            else if (core.IsMoving()) // lock camera
            {
                Focus();
            }
            ProximityInteractScript.Focus();
        }
    }

    public void Focus() {
        Vector3 goalPos = core.transform.position; // update vector
        goalPos.z = core.transform.position.z - 10; // maintain z axis difference
        transform.position = goalPos; // set position
    }

    public void Pan() {
        var vec = (target - transform.position).normalized;
        transform.position += vec * velocityFactor;
        var vec2 = (target - transform.position).normalized;
        if(vec2 != vec) transform.position = target;

        if(transform.position == target)
        {
            if(callback != null) callback.Invoke();
        }
    }
}
