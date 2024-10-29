using System.Collections;
using System.Collections.Generic;
using NodeEditorFramework.Standard;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

///<summary>
/// The box for selecting multiple entities from the overworld, this method handles overworld clicks for now
///</summary>
public class SelectionBoxScript : MonoBehaviour
{
    public Image image;
    private Vector2 startPoint;
    private Vector2 pivotVec;
    private Vector2 sizeVec;
    public ReticleScript reticleScript;
    public EventSystem eventSystem;
    public GameObject movementReticlePrefab;

    private PathData currentPathData;

    //private int nodeID = 1;
    private Vector2 lastPosition;

    public static bool simpleMouseMovement = true;

    private bool clicking = false;
    public Texture2D defaultCursor;
    public Texture2D overTargetableCursor;

    public static SelectionBoxScript instance;

    public static bool GetClicking()
    {
        return instance && instance.clicking;

    }

    void Awake()
    {
        instance = this;
        simpleMouseMovement = PlayerPrefs.GetString("SelectionBoxScript_simpleMouseMovement", "True") == "True";
    }

    private bool GetMouseOverTarget()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return CollisionManager.GetTargetAtPosition(pos) != null;
    }

    private void SetClicking(bool val)
    {
        clicking = image.enabled = val;
        if (val)
        {
            reticleScript.FindTarget();
            reticleScript.ClearSecondaryTargets();

            // Get reference point of selection box for drawing
            startPoint = Input.mousePosition;
            startPoint *= UIScalerScript.GetScale();
            image.rectTransform.anchoredPosition = startPoint;
            DrawBoxAndPivot();
        }
    }

    private void ClearTargetsIfInCutsceneOrInteracting()
    {
        if (PlayerCore.Instance.GetIsInteracting() || DialogueSystem.isInCutscene || PlayerViewScript.paused || PlayerViewScript.GetIsWindowActive())
        {
            if (!PlayerViewScript.paused)
            {
                reticleScript.ClearSecondaryTargets();
                reticleScript.SetTarget(null);
            }

            SetClicking(false);
            return;
        }
    }

    private void DrawBoxAndPivot()
    {
        // Draw box
        var d = UIScalerScript.GetScale();
        image.enabled = true;
        sizeVec = (Vector2)Input.mousePosition * d - startPoint;
        sizeVec.x = Mathf.Abs(sizeVec.x);
        sizeVec.y = Mathf.Abs(sizeVec.y);
        image.rectTransform.sizeDelta = sizeVec;

        // Change the pivot of the size delta when the mouse goes under/before the start point
        pivotVec = new Vector2();
        pivotVec.x = Input.mousePosition.x * d < startPoint.x ? 1 : 0;
        pivotVec.y = Input.mousePosition.y * d < startPoint.y ? 1 : 0;
        image.rectTransform.pivot = pivotVec;
    }

    private void GrabSelectionAndScanForEntities()
    {
        var d = UIScalerScript.GetScale();
        Vector2 boxStart =
            Camera.main.ScreenToWorldPoint(new Vector3(startPoint.x, startPoint.y, CameraScript.zLevel * d) / d);
        
        Vector2 boxExtents =
            Camera.main.ScreenToWorldPoint(
                new Vector3((1 - 2 * pivotVec.x) * sizeVec.x + startPoint.x,
                    (1 - 2 * pivotVec.y) * sizeVec.y + startPoint.y,
                    CameraScript.zLevel * d) / d);
        Rect finalBox = Rect.MinMaxRect(
            Mathf.Min(boxStart.x, boxExtents.x),
            Mathf.Min(boxStart.y, boxExtents.y),
            Mathf.Max(boxStart.x, boxExtents.x),
            Mathf.Max(boxStart.y, boxExtents.y)
        );

        // Now scan for entities
        foreach (var ent in AIData.entities)
        {
            if (ent != PlayerCore.Instance && ent.transform != PlayerCore.Instance.GetTargetingSystem().GetTarget()
                                           && finalBox.Contains(ent.transform.position))
            {
                reticleScript.AddSecondaryTarget(ent.transform);
            }
        }

        foreach (var part in AIData.strayParts)
        {
            if (part.transform != PlayerCore.Instance.GetTargetingSystem().GetTarget()
                                           && finalBox.Contains(part.transform.position))
            {
                reticleScript.AddSecondaryTarget(part.transform);
            }
        }

        foreach (var shards in AIData.shards)
        {
            if (shards.transform != PlayerCore.Instance.GetTargetingSystem().GetTarget()
                                           && finalBox.Contains(shards.transform.position))
            {
                reticleScript.AddSecondaryTarget(shards.transform);
            }
        }

        foreach (var fragments in AIData.rockFragments)
        {
            if (fragments.transform != PlayerCore.Instance.GetTargetingSystem().GetTarget()
                                           && finalBox.Contains(fragments.transform.position))
            {
                reticleScript.AddSecondaryTarget(fragments.transform);
            }
        }
    }

    private void CycleTarget()
    {
        var targSys = PlayerCore.Instance.GetTargetingSystem();

        // Tab cycles primary target
        if (Input.GetKeyDown(KeyCode.Tab) && targSys.GetSecondaryTargets().Count > 0)
        {
            if (targSys.GetTarget())
            {
                reticleScript.AddSecondaryTarget(targSys.GetTarget());
            }

            var newTarget = targSys.GetSecondaryTargets()[0];
            reticleScript.SetTarget(newTarget);
            reticleScript.RemoveSecondaryTarget(newTarget);
        }
    }

    void Update()
    {
        if (!PlayerCore.Instance || !PlayerCore.Instance.gameObject.activeSelf || !SystemLoader.AllLoaded) return;
        bool overTarget = GetMouseOverTarget();

        if (overTarget)
        {
            Cursor.SetCursor(overTargetableCursor, new Vector2(-0.16F, 0.16F), CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(defaultCursor, new Vector2(-0.16F, 0.16F), CursorMode.Auto);
        }

        ClearTargetsIfInCutsceneOrInteracting();

        CycleTarget();

        if (Input.GetMouseButtonDown(0) && !eventSystem.IsPointerOverGameObject())
        {
            SetClicking(true);
        }

        if (((Vector2)Input.mousePosition - startPoint).sqrMagnitude > 10) // Make sure the drag isn't interfering with clicks
        {
            if (Input.GetMouseButton(0) && clicking)
            {
                DrawBoxAndPivot();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (clicking)
                {
                    // End selection, push entities in box into secondary targets
                    SetClicking(false);
                    // Grab the rect of the selection box

                    GrabSelectionAndScanForEntities();
                }
            }
        }
        else if (Input.GetMouseButtonUp(0) && clicking)
        {
            SetClicking(false);
        }

        if (!Input.GetMouseButton(0) && clicking)
        {
            SetClicking(false);
        }
    }

    void BeginPathData(PathData.Node node, SpriteRenderer renderer)
    {
        foreach (var rend in reticleRenderersByNode.Values)
        {
            if (rend)
            {
                Destroy(rend.gameObject);
            }
        }

        reticleRenderersByNode.Clear();
        reticleRenderersByNode.Add(node, renderer);
        currentPathData = new PathData();
        currentPathData.waypoints = new List<PathData.Node>();
        currentPathData.waypoints.Add(node);
        lastPosition = PlayerCore.Instance.transform.position;
        StartCoroutine(pathPlayer(currentPathData));
    }

    void ContinuePathData(PathData.Node node, SpriteRenderer renderer)
    {
        reticleRenderersByNode.Add(node, renderer);
        currentPathData.waypoints.Add(node);
        PathData.Node lastNode = GetNode(currentPathData, node.ID - 1);
        lastNode.children.Add(node.ID);
        var lineRenderer = reticleRenderersByNode[lastNode].transform.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(
            new Vector3[]
            {
                lastNode.position,
                (node.position - lastNode.position).normalized + lastNode.position
            }
        );
    }

    private Dictionary<PathData.Node, SpriteRenderer> reticleRenderersByNode = new Dictionary<PathData.Node, SpriteRenderer>();

    IEnumerator pathPlayer(PathData data)
    {
        var player = PlayerCore.Instance;
        // TODO: Jank workaround, fix eventually
        PathData.Node current = data.waypoints[0];

        while (current != null)
        {
            if (PlayerCore.Instance.getDirectionalInput() != Vector2.zero || (currentPathData == null || !currentPathData.waypoints.Contains(current)))
            {
                if (currentPathData == null || (!currentPathData.waypoints.Contains(current)))
                {
                    yield break;
                }
                else
                {
                    foreach (var renderer in reticleRenderersByNode.Values)
                    {
                        if (renderer)
                        {
                            Destroy(renderer.gameObject);
                        }
                    }

                    reticleRenderersByNode.Clear();
                }

                break;
            }

            Vector2 delta = current.position - (Vector2)player.transform.position - player.GetComponent<Rigidbody2D>().velocity * Time.fixedDeltaTime;
            Vector2 originalDelta = current.position - lastPosition + player.GetComponent<Rigidbody2D>().velocity * Time.fixedDeltaTime;
            player.MoveCraft(delta.normalized);
            var lastNode = current.ID - 1 > 0 ? GetNode(currentPathData, current.ID - 1) : null;
            var lineRenderer = reticleRenderersByNode[current].GetComponent<LineRenderer>();

            reticleRenderersByNode[current].color = lineRenderer.startColor = lineRenderer.endColor
                = Color.Lerp(new Color32(100, 100, 100, 255), Color.green,
                    (1 - (delta.magnitude / originalDelta.magnitude)));

            if (delta.sqrMagnitude < PathAI.minDist)
            {
                Destroy(reticleRenderersByNode[current].gameObject, 5);
                reticleRenderersByNode.Remove(current);
                if (current.children.Count > 0)
                {
                    lastPosition = current.position;
                    int next = Random.Range(0, current.children.Count);
                    current = GetNode(data, current.children[next]);
                }
                else
                {
                    current = null;
                }
            }

            yield return null;
        }

        //nodeID = 1;
        currentPathData = null;
    }

    PathData.Node GetNode(PathData path, int ID)
    {
        for (int i = 0; i < path.waypoints.Count; i++)
        {
            if (path.waypoints[i].ID == ID)
            {
                return path.waypoints[i];
            }
        }

        return null;
    }
}
