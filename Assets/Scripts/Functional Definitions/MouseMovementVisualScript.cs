using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Standard;

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

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void Focus()
    {
        overMinimap = GetMousePosOnMinimap().x > 0 && GetMousePosOnMinimap().y > 0;
        if(Input.GetMouseButton(1))
        {
            instance.deltaLineRenderer.positionCount = 2;
            instance.deltaLineRenderer.SetPosition(0, PlayerCore.Instance.transform.position);
            var pos = instance.minimapCamera.ScreenToWorldPoint(Input.mousePosition + new Vector3(0,0,30));
            //instance.deltaLineRenderer.SetPosition(1, CameraScript.instance.GetWorldPositionOfMouse());
            instance.deltaLineRenderer.SetPosition(1, overMinimap ? instance.minimapCamera.ScreenToWorldPoint(GetMousePosOnMinimap()) :
                CameraScript.instance.GetWorldPositionOfMouse());
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
        return ((Vector2)Input.mousePosition - minimapStart) * 512/250;
    }
}
