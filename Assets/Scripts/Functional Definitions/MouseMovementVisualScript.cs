using UnityEngine;

public class MouseMovementVisualScript : MonoBehaviour
{
    [SerializeField]
    private Texture2D defaultCursor;

    [SerializeField]
    private Texture2D overTargetableCursor;

    [SerializeField]
    private LineRenderer deltaLineRenderer;

    public static MouseMovementVisualScript instance;

    [SerializeField]
    private Camera minimapCamera;

    public static bool overMinimap;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public static void Focus()
    {
        overMinimap = GetMousePosOnMinimap().x > 0 && GetMousePosOnMinimap().y > 0;
        instance.deltaLineRenderer.positionCount = 2;
        instance.deltaLineRenderer.SetPosition(0, PlayerCore.Instance.transform.position);
        if (Input.GetMouseButton(1))
        {
            instance.deltaLineRenderer.SetPosition(1,
                CameraScript.instance.GetWorldPositionOfMouse());
        }
        else if (Input.GetMouseButton(0) && overMinimap)
        {
            instance.deltaLineRenderer.SetPosition(1, instance.minimapCamera.ScreenToWorldPoint(GetMousePosOnMinimap()));
        }
        else if (PlayerCore.Instance.GetMinimapPoint().HasValue)
        {
            instance.deltaLineRenderer.SetPosition(1, PlayerCore.Instance.GetMinimapPoint().Value);
        }
        else
        {
            instance.deltaLineRenderer.positionCount = 0;
        }
    }

    // TODO: remove hardcoding
    public static Vector3 GetMousePosOnMinimap()
    {
        var minimapStart = new Vector2(Screen.width, Screen.height) - new Vector2(260, 260);
        return ((Vector2)Input.mousePosition - minimapStart) * 512 / 250;
    }
}
