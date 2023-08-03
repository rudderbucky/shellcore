using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldGrid : MonoBehaviour
{
    public int gridWidth; // grid width
    public int gridHeight; // grid height
    public int lineSpacing = 4;
    Transform mcamera;
    private GameObject lineHolder;
    private List<LineRenderer> linesUp;
    private List<LineRenderer> linesRight;
    public static OverworldGrid instance;
    bool initialized;
    private static bool active;
    void Awake()
    {
        instance = this;
        initialized = false;
        active = PlayerPrefs.GetString("OverworldGrid_active", "False") == "True";
    }

    public static void SetActive(bool act)
    {
        active = act;
        if (instance) instance.Initialize();
    }

    void Start()
    {
        Initialize();
    }

    // Start is called before the first frame update
    public void Initialize()
    {
        initialized = true;
        if (lineHolder)
        {
            for (int i = 0; i < lineHolder.transform.childCount; i++)
            {
                Destroy(lineHolder.transform.GetChild(i).gameObject);
            }
            Destroy(lineHolder);
        }
        if (!active) return;
        Vector3 dimensions = Camera.main.ScreenToWorldPoint(
                new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, CameraScript.GetMaxZoomLevel()));
            // grab camera dimensions
        mcamera =  Camera.main.transform;
        lineHolder = new GameObject("Line Holder");
        linesUp = new List<LineRenderer>();
        linesRight = new List<LineRenderer>();
        gridWidth = 1 + (int)Mathf.Ceil((dimensions.x - mcamera.position.x) * 2 / lineSpacing); // calculate height and width using camera dimensions
        gridHeight = 1 + (int)Mathf.Ceil((dimensions.y - mcamera.position.y) * 2 / lineSpacing);
        var lineStartPos = new Vector2 // get the tile start position (this project needs the tiles to center at 0,0)
        {
            x = mcamera.position.x - lineSpacing * (gridWidth - 1) / 2,
            y = mcamera.position.y - lineSpacing * (gridHeight - 1) / 2
        };

        for (int i = 0; i < gridWidth; i++)
        {
            linesUp.Add(CreateRenderer(new Vector2(i*4 + lineStartPos.x, lineStartPos.y), Vector3.up * Camera.main.pixelHeight));
        }
        for (int i = 0; i < gridHeight; i++)
        {
            linesRight.Add(CreateRenderer(new Vector2(lineStartPos.x, i*4 + lineStartPos.y), Vector3.right * Camera.main.pixelWidth));
        }
        
    }

    LineRenderer CreateRenderer(Vector2 pos, Vector3 dir)
    {
        LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>();
        lineRenderer.transform.SetParent(lineHolder.transform);
        lineRenderer.transform.position = pos;
        lineRenderer.startWidth = lineRenderer.endWidth = 0.05F;
        lineRenderer.SetPosition(0, lineRenderer.transform.position);
        lineRenderer.SetPosition(1, dir + lineRenderer.transform.position);
        lineRenderer.material = ResourceManager.GetAsset<Material>("white_material");
        lineRenderer.startColor = lineRenderer.endColor = new Color32(85, 100, 85, 50);
        return lineRenderer;
    }

    void Update()
    {
        if (!initialized || !active) return;
        foreach (var renderer in linesRight)
        {
            LineConstantDistance(renderer, 0);
            LineWrapper(renderer, 1);
        }

        foreach (var renderer in linesUp)
        {
            LineWrapper(renderer, 0);
            LineConstantDistance(renderer, 1);
        }
    }

    private void LineConstantDistance(LineRenderer line, int dimension)
    {
        if (!line) return;
            
        var lineStartPos = new Vector2 // get the tile start position (this project needs the tiles to center at 0,0)
        {
            x = mcamera.position.x - lineSpacing * (gridWidth - 1) / 2,
            y = mcamera.position.y - lineSpacing * (gridHeight - 1) / 2
        };
        var pos = line.transform.position;
        line.SetPosition(0, line.GetPosition(0) - pos);
        line.SetPosition(1, line.GetPosition(1) - pos);
        pos[dimension] = lineStartPos[dimension];
        line.transform.position = pos;

        line.SetPosition(0, line.GetPosition(0) + pos);
        line.SetPosition(1, line.GetPosition(1) + pos);
    }
    private void LineWrapper(LineRenderer line, int dimension)
    {
        if (line)
        {
            float limit = dimension == 0 ? gridWidth / 2 : gridHeight / 2;
            // the limit before the line should wrap

            if (Mathf.Abs(line.GetPosition(1)[dimension] - mcamera.position[dimension]) / lineSpacing > limit) // this means it is at an axis edge
            {
                limit = line.GetPosition(1)[dimension] - mcamera.position[dimension] > 0 ? -limit : limit; // right edge
                // (this may be slightly inefficient but I don't care it's cool)
                var oldpos = line.transform.position;
                // if limit remains positive left edge
                var displacement = line.transform.position; // grab the line position
                line.SetPosition(0, line.GetPosition(0) - displacement);
                line.SetPosition(1, line.GetPosition(1) - displacement);
                displacement[dimension] = displacement[dimension] + 2 * limit * lineSpacing; // update the x position to be at the other edge
                line.SetPosition(0, line.GetPosition(0) + displacement);
                line.SetPosition(1, line.GetPosition(1) + displacement);
                line.transform.position = displacement; // update the line position, similar process for the other checks as well
            }
        }
    }
}
