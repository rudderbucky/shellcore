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

    private bool dronesChecked = false;
    public static bool simpleMouseMovement = true;

    private bool clicking = false;
    public Texture2D defaultCursor;
    public Texture2D overTargetableCursor;

    void Awake()
    {
        simpleMouseMovement = PlayerPrefs.GetString("SelectionBoxScript_simpleMouseMovement", "True") == "True";
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // create a ray
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity); // get an array of all hits
        bool overTarget = false;

        foreach (var hit in hits)
        {
            var hitTransforn = hit.transform;
            var ent = hitTransforn.GetComponent<Entity>();
            if ((hitTransforn.GetComponent<ITargetable>() != null && hitTransforn != PlayerCore.Instance.transform)
                || (hitTransforn.GetComponent<Draggable>() && (!ent || ent.faction == PlayerCore.Instance.faction)))
            {
                Cursor.SetCursor(overTargetableCursor, new Vector2(-0.16F, 0.16F), CursorMode.Auto);
                overTarget = true;
                break;
            }
        }

        if (!overTarget)
        {
            Cursor.SetCursor(defaultCursor, new Vector2(-0.16F, 0.16F), CursorMode.Auto);
        }

        // Clear targets if in cutscene/interacting
        if (PlayerCore.Instance.GetIsInteracting() || DialogueSystem.isInCutscene || PlayerViewScript.paused || PlayerViewScript.GetIsWindowActive())
        {
            // Debug.Log(PlayerCore.Instance.GetIsInteracting() + " " + DialogueSystem.isInCutscene + " " + PlayerViewScript.paused + " " + PlayerViewScript.GetIsWindowActive());
            if (!PlayerViewScript.paused)
            {
                reticleScript.ClearSecondaryTargets();
                reticleScript.SetTarget(null);
            }

            image.enabled = false;
            clicking = false;
            return;
        }

        var targSys = PlayerCore.Instance.GetTargetingSystem();

        // Tab cycles primary target
        if (Input.GetKeyDown(KeyCode.Tab) && targSys.GetSecondaryTargets().Count > 0)
        {
            if (targSys.GetTarget() && targSys.GetTarget().GetComponent<Entity>())
            {
                reticleScript.AddSecondaryTarget(targSys.GetTarget().GetComponent<Entity>());
            }

            var newTarget = targSys.GetSecondaryTargets()[0];
            reticleScript.SetTarget(newTarget.transform);
            reticleScript.RemoveSecondaryTarget(newTarget);
        }

        // Right click clears targets
        /*
        if(Input.GetMouseButtonDown(1))
        {
            if(Input.GetKey(KeyCode.LeftShift))
            {
                PlayerCore.Instance.GetTargetingSystem().SetTarget(null);
            }
            else
            {
                if(PlayerCore.Instance.GetTargetingSystem().GetSecondaryTargets().Count > 0)
                    reticleScript.ClearSecondaryTargets();
                else PlayerCore.Instance.GetTargetingSystem().SetTarget(null);
            }
        }
        */

        if (Input.GetMouseButtonDown(0) && !eventSystem.IsPointerOverGameObject())
        {
            dronesChecked = reticleScript.FindTarget();
            reticleScript.ClearSecondaryTargets();

            // Get reference point of selection box for drawing
            startPoint = Input.mousePosition;
            image.rectTransform.anchoredPosition = startPoint;

            clicking = true;
        }

        if (((Vector2)Input.mousePosition - startPoint).sqrMagnitude > 10) // Make sure the drag isn't interfering with clicks
        {
            if (Input.GetMouseButton(0) && clicking)
            {
                // Draw box
                image.enabled = true;
                sizeVec = (Vector2)Input.mousePosition - startPoint;
                sizeVec.x = Mathf.Abs(sizeVec.x);
                sizeVec.y = Mathf.Abs(sizeVec.y);
                image.rectTransform.sizeDelta = sizeVec;

                // Change the pivot of the size delta when the mouse goes under/before the start point
                pivotVec = new Vector2()
                {
                    x = Input.mousePosition.x < startPoint.x ? 1 : 0,
                    y = Input.mousePosition.y < startPoint.y ? 1 : 0
                };
                image.rectTransform.pivot = pivotVec;
            }
            else if (Input.GetMouseButtonUp(0) && clicking)
            {
                // End selection, push entities in box into secondary targets
                clicking = false;
                // Grab the rect of the selection box
                Vector2 boxStart =
                    Camera.main.ScreenToWorldPoint(new Vector3(startPoint.x, startPoint.y, CameraScript.zLevel));
                Vector2 boxExtents =
                    Camera.main.ScreenToWorldPoint(
                        new Vector3(
                            (1 - 2 * pivotVec.x) * sizeVec.x + startPoint.x,
                            (1 - 2 * pivotVec.y) * sizeVec.y + startPoint.y,
                            CameraScript.zLevel
                        )
                    );
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
                        reticleScript.AddSecondaryTarget(ent);
                    }
                }

                image.enabled = false;
            }
        }

        /*
        // Drones being commanded has a higher priority than mouse movement, so it should prevent the player themselves from moving.
        // This controls mouse movement, first it checks if the click was meant for mouse movement
        else if(!dronesChecked && Input.GetMouseButtonUp(0) 
            && !PlayerCore.Instance.GetIsDead() && !PlayerCore.Instance.GetTargetingSystem().GetTarget()
                && (Input.GetKey(KeyCode.LeftShift) || simpleMouseMovement) && !DialogueSystem.isInCutscene)
        {
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0,0,CameraScript.zLevel));
            var renderer = Instantiate(movementReticlePrefab, mouseWorldPos, Quaternion.identity).GetComponent<SpriteRenderer>();
            renderer.color = new Color32((byte)100, (byte)100, (byte)100, (byte)255);

            var node = new PathData.Node();
            node.children = new List<int>();
            node.position = mouseWorldPos;
            node.ID = nodeID++;

            if(currentPathData == null
                || (!Input.GetKey(KeyCode.LeftShift) && simpleMouseMovement))
            {
                BeginPathData(node, renderer);
            }
            else
            {
                ContinuePathData(node, renderer);
            }
        } else if(Input.GetMouseButtonUp(0)) dronesChecked = false;
        */
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
        currentPathData = new PathData()
        {
            waypoints = new List<PathData.Node>()
        };
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

            Vector2 delta = current.position - (Vector2)player.transform.position - (Vector2)player.GetComponent<Rigidbody2D>().velocity * Time.fixedDeltaTime;
            Vector2 originalDelta = current.position - lastPosition + (Vector2)player.GetComponent<Rigidbody2D>().velocity * Time.fixedDeltaTime;
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
