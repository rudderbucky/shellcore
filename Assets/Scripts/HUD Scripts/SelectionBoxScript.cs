using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NodeEditorFramework.Standard;

///<summary>
/// The box for selecting multiple entities from the overworld
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
    private int nodeID = 1;
    private Vector2 lastPosition;

    private bool dronesChecked = false;
    public static bool simpleMouseMovement = true;

    void Awake()
    {
        simpleMouseMovement = PlayerPrefs.GetString("SelectionBoxScript_simpleMouseMovement", "True") == "True";
    }
    void Update()
    {
        if(PlayerCore.Instance.GetIsInteracting() || DialogueSystem.isInCutscene || PlayerViewScript.paused) 
        {
            image.enabled = false;
            return;
        }

        if(Input.GetMouseButtonDown(0))
        {
            if(!eventSystem.IsPointerOverGameObject())
            {
                dronesChecked = reticleScript.FindTarget();
            }
            reticleScript.ClearSecondaryTargets();

            // Get reference point of selection box for drawing
            startPoint = Input.mousePosition;
            image.rectTransform.anchoredPosition = startPoint;
        }

        if(((Vector2)Input.mousePosition - startPoint).sqrMagnitude > 10) // make sure the drag isn't interfering with clicks
        {
            if(Input.GetMouseButton(0))
            {
                // Draw box
                image.enabled = true;
                sizeVec = (Vector2)Input.mousePosition - startPoint;
                sizeVec.x = Mathf.Abs(sizeVec.x);
                sizeVec.y = Mathf.Abs(sizeVec.y);
                image.rectTransform.sizeDelta = sizeVec;
                
                // Change the pivot of the size delta when the mouse goes under/before the start point
                pivotVec = new Vector2();
                pivotVec.x = Input.mousePosition.x < startPoint.x ? 1 : 0;
                pivotVec.y = Input.mousePosition.y < startPoint.y ? 1 : 0;
                image.rectTransform.pivot = pivotVec;
            } 
            else if(Input.GetMouseButtonUp(0))
            {
                // Grab the rect of the selection box
                Vector2 boxStart = 
                    Camera.main.ScreenToWorldPoint(new Vector3(startPoint.x, startPoint.y, 10));
                Vector2 boxExtents = 
                    Camera.main.ScreenToWorldPoint(
                        new Vector3((1 - 2 * pivotVec.x) * sizeVec.x + startPoint.x, 
                        (1 - 2 * pivotVec.y) * sizeVec.y + startPoint.y, 
                        10));
                Rect finalBox = Rect.MinMaxRect(
                    Mathf.Min(boxStart.x, boxExtents.x),
                    Mathf.Min(boxStart.y, boxExtents.y),
                    Mathf.Max(boxStart.x, boxExtents.x),
                    Mathf.Max(boxStart.y, boxExtents.y)
                );

                // Now scan for entities
                foreach(var ent in AIData.entities)
                {
                    if(ent != PlayerCore.Instance && ent.transform != PlayerCore.Instance.GetTargetingSystem().GetTarget() 
                    && finalBox.Contains(ent.transform.position) && PlayerCore.Instance.GetTargetingSystem().GetSecondaryTargets().Count < 8) 
                        // only 8 secondary targets allowed
                        reticleScript.AddSecondaryTarget(ent);
                }
                image.enabled = false;
            }
        }
        else if(!dronesChecked && Input.GetMouseButtonUp(0) 
            && !PlayerCore.Instance.GetIsDead() && 
                !eventSystem.IsPointerOverGameObject() && !PlayerCore.Instance.GetTargetingSystem().GetTarget()
                && (Input.GetKey(KeyCode.LeftShift) || simpleMouseMovement))
        {
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0,0,10));
            var renderer = Instantiate(movementReticlePrefab, mouseWorldPos, Quaternion.identity).GetComponent<SpriteRenderer>();
            renderer.color = new Color32((byte)100, (byte)100, (byte)100, (byte)255);

            var node = new PathData.Node();
            node.children = new List<int>();
            node.position = mouseWorldPos;
            node.ID = nodeID++;

            if(currentPathData == null
                || (!Input.GetKey(KeyCode.LeftShift) && !eventSystem.IsPointerOverGameObject() && simpleMouseMovement))
            {
                foreach(var rend in reticleRenderersByNode.Values) 
                    if(rend) Destroy(rend.gameObject);
                reticleRenderersByNode.Clear();
                reticleRenderersByNode.Add(node, renderer);
                currentPathData = new PathData();
                currentPathData.waypoints = new List<PathData.Node>();
                currentPathData.waypoints.Add(node);
                lastPosition = PlayerCore.Instance.transform.position;
                StartCoroutine(pathPlayer(currentPathData));
            }
            else
            {
                reticleRenderersByNode.Add(node, renderer);
                currentPathData.waypoints.Add(node);
                PathData.Node lastNode = GetNode(currentPathData, node.ID - 1);
                lastNode.children.Add(node.ID);
                var lineRenderer = reticleRenderersByNode[lastNode].transform.GetComponent<LineRenderer>();
                lineRenderer.positionCount = 2;
                lineRenderer.SetPositions(
                    new Vector3[] {
                        lastNode.position,
                        ((Vector2)mouseWorldPos - lastNode.position).normalized + lastNode.position
                    }
                );
            }
        } else if(Input.GetMouseButtonUp(0)) dronesChecked = false;
    }

    private Dictionary<PathData.Node, SpriteRenderer> reticleRenderersByNode = new Dictionary<PathData.Node, SpriteRenderer>();
    IEnumerator pathPlayer(PathData data)
    {
        var player = PlayerCore.Instance;
        // TODO: Jank workaround, fix eventually
        PathData.Node current = data.waypoints[0];

        while (current != null)
        {
            if(PlayerCore.getDirectionalInput() != Vector2.zero || (!currentPathData.waypoints.Contains(current))){
                if((!currentPathData.waypoints.Contains(current)))
                    yield break;
                else
                {
                    foreach(var renderer in reticleRenderersByNode.Values) 
                    if(renderer) Destroy(renderer.gameObject);
                    reticleRenderersByNode.Clear();
                }
                break;
            } 

            Vector2 delta = current.position - (Vector2)player.transform.position;
            Vector2 originalDelta = current.position - lastPosition;
            player.MoveCraft(delta.normalized);
            var lastNode = current.ID - 1 > 0 ? GetNode(currentPathData, current.ID - 1) : null;
            var lineRenderer = reticleRenderersByNode[current].GetComponent<LineRenderer>();

            reticleRenderersByNode[current].color = lineRenderer.startColor = lineRenderer.endColor
                = Color.Lerp(new Color32((byte)100, (byte)100, (byte)100, (byte)255), Color.green,
            (1 - (delta.magnitude / originalDelta.magnitude)));

            if (delta.sqrMagnitude < 0.1f)
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
                    current = null;
            }
            yield return null;
        }
        nodeID = 1;
        currentPathData = null;
    }

    PathData.Node GetNode(PathData path, int ID)
    {
        for (int i = 0; i < path.waypoints.Count; i++)
        {
            if (path.waypoints[i].ID == ID)
                return path.waypoints[i];
        }
        return null;
    }
}
