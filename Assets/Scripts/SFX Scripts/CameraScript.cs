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

    public static float zLevel = 10;

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
            goalPos.z = -zLevel;
            transform.position = goalPos; // set position
        }
    }

    private void LateUpdate()
    {
     if(initialized)
        {
            if(Input.GetAxis("Mouse ScrollWheel") < 0f) 
            { 
                zLevel = Mathf.Min(10, zLevel + 0.5F);
            }
            else if(Input.GetAxis("Mouse ScrollWheel") > 0f) 
            {
                zLevel = Mathf.Max(5, zLevel - 0.5F);
            }    

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
        goalPos.z = -zLevel;
        transform.position = goalPos; // set position
    }

    public void Pan() {
        var vec = ((Vector2)target - (Vector2)transform.position).normalized;
        transform.position += (Vector3)vec * velocityFactor;
        var vec2 = ((Vector2)target - (Vector2)transform.position).normalized;
        if(vec2 != vec) transform.position = new Vector3(target.x, target.y, -zLevel);

        if(transform.position == target)
        {
            if(callback != null) callback.Invoke();
        }
    }
}
