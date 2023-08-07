using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Scripts for camera
/// </summary>
public class CameraScript : MonoBehaviour
{
    public static CameraScript instance;
    public static UnityEngine.Events.UnityAction callback;
    private PlayerCore core; // the target for the camera to follow
    private bool initialized;

    public static bool panning;
    public static Vector3 target;
    public static float velocityFactor;

    public static float zLevel = 10;
    public EventSystem eventSystem;
    public Camera minimapCamera;

    public void Initialize(PlayerCore player)
    {
        if (zLevel > GetMaxZoomLevel())
        {
            zLevel = GetMaxZoomLevel();
        }

        core = player;

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
            foreach (var rect in RectangleEffectScript.instances)
            {
                if (rect)
                {
                    rect.Start();
                }
            }
            if (OverworldGrid.instance) OverworldGrid.instance.Initialize();
        }
    }

    // returns max zoom level based on player core tier
    public static float GetMaxZoomLevel()
    {
        if (!PlayerCore.Instance)
        {
            return 10;
        }

        return 10 + 2.5F * (1 +
                            CoreUpgraderScript.GetCoreTier(PlayerCore.Instance.blueprint.coreShellSpriteID));
    }

    private void LateUpdate()
    {
        if (initialized)
        {
            if (eventSystem && !eventSystem.IsPointerOverGameObject())
            {
                if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    zLevel = Mathf.Min(GetMaxZoomLevel(), zLevel + 0.5F);
                    if (target != null) target.z = -zLevel;
                    Focus(transform.position);
                }
                else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                {
                    zLevel = Mathf.Max(5, zLevel - 0.5F);
                    if (target != null) target.z = -zLevel;
                    Focus(transform.position);
                }
            }

            if (panning)
            {
                Pan();
            }
            else if (core.IsMoving()) // lock camera
            {
                Focus(core.transform.position);
            }

            ProximityInteractScript.Focus();
            MouseMovementVisualScript.Focus();
            if (ReticleScript.instance)
            {
                ReticleScript.instance.Focus();
            }
        }
    }

    public void Focus(Vector3 pos)
    {
        Vector3 goalPos = pos; // update vector
        goalPos.z = -zLevel;
        transform.position = goalPos; // set position
    }

    public void Pan()
    {
        var vec = ((Vector2)target - (Vector2)transform.position).normalized;
        transform.position += (Vector3)vec * velocityFactor;
        var vec2 = ((Vector2)target - (Vector2)transform.position).normalized;
        if (Vector2.Distance(vec2,vec) > 0.5F)
        {
            transform.position = new Vector3(target.x, target.y, -zLevel);
        }

        if (transform.position == target)
        {
            if (callback != null)
            {
                callback.Invoke();
            }
        }
    }

    public Vector3 GetWorldPositionOfMouse()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, CameraScript.zLevel));
    }
}
