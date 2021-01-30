using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandPlatformGenerator : MonoBehaviour {


    public static string[] prefabNames = new string[] {
                "New Junction",         // 0
                "New 1 Entry",          // 1
                "New 2 Entry",          // 2
                "New 0 Entry",          // 3
                "New 0 Entry Ghost",    // 4
                "New 3 Entry",          // 5
                "New 4 Entry",          // 6
                "New Junction Ghost",   // 7
                "New 1 Entry Ghost",    // 8
                "New 2 Entry Ghost",    // 9
                "New 3 Entry Ghost",    // 10
                "New 4 Entry Ghost",    // 11
            };

    // TODO: generate one mesh instead of multiple objects

    static LandPlatformGenerator instance;
    public static LandPlatformGenerator Instance
    {
        private set
        {
            instance = value;
        }
        get
        {
            if (instance == null || !instance)
            {
                instance = FindObjectOfType<LandPlatformGenerator>();
                instance.Initialize();
            }
            return instance;
        }
    }

    public GroundPlatform[] groundPlatforms;
    //private Dictionary<int, GameObject> tiles;
    private List<Rect> areas;
    //private List<NavigationNode> nodes;
    private Vector2 center;

    //private Dictionary<NavigationNode, int> areaIDByNode;
    // private Dictionary<GameObject, int> areaIDByTile; // TODO: Add areaIDByTile
    public float tileSize { get; set; }
    public Color color { get; private set; }
    public Vector2 Offset { get; set; }

    public static bool IsOnGround(Vector3 position)
    {
        if (Instance.groundPlatforms == null)
            return false;

        Vector2 relativePos = ((Vector2)position - instance.Offset) / Instance.tileSize;
        relativePos.y = -relativePos.y;

        for (int i = 0; i < Instance.groundPlatforms.Length; i++)
        {
            var plat = Instance.groundPlatforms[i];
            for (int j = 0; j < plat.tiles.Count; j++)
            {
                if (plat.tiles[j].pos == new Vector2Int(Mathf.RoundToInt(relativePos.x), Mathf.RoundToInt(relativePos.y)))
                {
                    if (plat.tiles[j].colliders.Any(x => x.OverlapPoint(position)))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        return false;

        //int index = Mathf.RoundToInt(relativePos.x) + Mathf.RoundToInt(relativePos.y) * cols;
        //if (Instance.tiles.ContainsKey(index))
        //{
        //    GameObject tile = Instance.tiles[index];
        //    if (tile.GetComponents<Collider2D>().Any(x => x.OverlapPoint(position)))
        //    {
        //        return true;
        //    }
        //}
        //return false;
    }

    public void SetColor(Color color) {
        this.color = color;
    }

    public void BuildTiles(LandPlatform platform, Vector2 center) {

        this.center = center;
        var blueprint = platform;

        tileSize = ResourceManager.GetAsset<GameObject>(blueprint.prefabs[0]).GetComponent<SpriteRenderer>().bounds.size.x;

        var cols = blueprint.columns;
        var rows = blueprint.rows;
        Offset = new Vector2 
        {
            x = center.x - tileSize * (cols-1)/2F,
            y = center.y + tileSize * (rows-1)/2F
        };
        // TODO: read new data from file, for each platform

        areas = new List<Rect>();

        var tiles = new List<GroundPlatform.Tile>();

        if (blueprint != null && blueprint.prefabs.Length > 0)
        {
            // Create tile objects
            for (int i = 0; i < blueprint.tilemap.Length; i++)
            {

                var pos = new Vector3
                {
                    x = Offset.x + tileSize * (i % cols),
                    y = Offset.y - tileSize * (i / cols),
                    z = 0
                };

                if (blueprint.tilemap[i] > -1)
                {
                    var obj = Instantiate(ResourceManager.GetAsset<GameObject>(blueprint.prefabs[blueprint.tilemap[i]]), pos, Quaternion.identity);
                    obj.transform.localEulerAngles = new Vector3(0, 0, 90 * blueprint.rotations[i]);
                    obj.GetComponent<SpriteRenderer>().color = color;
                    obj.transform.parent = transform;

                    tiles.Add(
                        new GroundPlatform.Tile()
                        {
                            pos = new Vector2Int(i % cols, i / cols),
                            type = (byte)blueprint.tilemap[i],
                            rotation = (byte)blueprint.rotations[i],
                            directions = new Dictionary<Vector2Int, byte>(),
                            colliders = obj.GetComponentsInChildren<Collider2D>()
                        });
                }
            }
            groundPlatforms = DivideToPlatforms(tiles);
        }
    }

    public static GroundPlatform[] DivideToPlatforms(List<GroundPlatform.Tile> tiles)
    {
        // Divide to platforms
        int[] platNums = new int[tiles.Count];
        for (int i = 0; i < tiles.Count; i++)
        {
            platNums[i] = -1;
        }
        int platIndex = 0;
        for (int i = 0; i < tiles.Count; i++)
        {
            var openList = new List<GroundPlatform.Tile>();

            if (platNums[i] == -1)
            {
                openList.Add(tiles[i]);
                platNums[i] = platIndex;

                while (openList.Count > 0)
                {
                    // Get connected neighbors

                    var current = openList[0];
                    int ends = GroundPlatform.GetPlatformEnds(current);

                    //Debug.Log("Tile: " + new Vector2Int(current.pos.x, current.pos.y) + " type: " + current.type + " rot: " + current.rotation + " direction flags: " + (ends & 8) + " " + (ends & 4) + " " + (ends & 2) + " " + (ends & 1));
                    if ((ends & 1) == 1)
                    {
                        int neighborIndex = tiles.FindIndex(t => t.pos == new Vector2Int(current.pos.x + 1, current.pos.y));
                        if (neighborIndex > -1)
                        {
                            if (platNums[neighborIndex] == -1)
                            {
                                openList.Add(tiles[neighborIndex]);
                                platNums[neighborIndex] = platIndex;
                            }
                            else if (platNums[neighborIndex] != platIndex)
                            {
                                Debug.LogWarning("Platform index collision 1!");
                            }
                        }
                        else
                        {
                            Debug.Log("Couldn't find a 1 tile");
                        }
                    }
                    if ((ends & 2) == 2)
                    {
                        int neighborIndex = tiles.FindIndex(t => t.pos == new Vector2Int(current.pos.x, current.pos.y - 1));
                        if (neighborIndex > -1)
                        {
                            if (platNums[neighborIndex] == -1)
                            {
                                openList.Add(tiles[neighborIndex]);
                                platNums[neighborIndex] = platIndex;
                            }
                            else if (platNums[neighborIndex] != platIndex)
                            {
                                Debug.LogWarning("Platform index collision 2!");
                            }
                        }
                        else
                        {
                            Debug.Log("Couldn't find a 2 tile");
                        }
                    }
                    if ((ends & 4) == 4)
                    {
                        int neighborIndex = tiles.FindIndex(t => t.pos == new Vector2Int(current.pos.x - 1, current.pos.y));
                        if (neighborIndex > -1)
                        {
                            if (platNums[neighborIndex] == -1)
                            {
                                openList.Add(tiles[neighborIndex]);
                                platNums[neighborIndex] = platIndex;
                            }
                            else if (platNums[neighborIndex] != platIndex)
                            {
                                Debug.LogWarning("Platform index collision 4!");
                            }
                        }
                        else
                        {
                            Debug.Log("Couldn't find a 4 tile");
                        }
                    }
                    if ((ends & 8) == 8)
                    {
                        int neighborIndex = tiles.FindIndex(t => t.pos == new Vector2Int(current.pos.x, current.pos.y + 1));
                        if (neighborIndex > -1)
                        {
                            if (platNums[neighborIndex] == -1)
                            {
                                openList.Add(tiles[neighborIndex]);
                                platNums[neighborIndex] = platIndex;
                            }
                            else if (platNums[neighborIndex] != platIndex)
                            {
                                Debug.LogWarning("Platform index collision 8!");
                            }
                        }
                        else
                        {
                            Debug.Log("Couldn't find an 8 tile");
                        }
                    }

                    openList.RemoveAt(0);
                }
                platIndex++;
            }
        }

        var platforms = new List<GroundPlatform>();

        for (int i = 0; i < platIndex; i++)
        {
            var platTiles = new List<GroundPlatform.Tile>();
            for (int j = 0; j < platNums.Length; j++)
            {
                if (platNums[j] == i)
                    platTiles.Add(tiles[j]);
            }
            platforms.Add(new GroundPlatform(platTiles.ToArray()));
        }

        return platforms.ToArray();
    }

    public void Initialize()
    {
        Instance = this;
        //nodes = new List<NavigationNode>();
        groundPlatforms = null;
    }

    public void Unload()
    {
        if (groundPlatforms != null)
            foreach (var plat in groundPlatforms)
            {
                plat.Clear();
            }
        groundPlatforms = null;

        //nodes.Clear();
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundPlatforms == null)
            return;

        var v3 = Input.mousePosition;
        v3.z = 10.0f;
        v3 = Camera.main.ScreenToWorldPoint(v3);
        Vector2 mPos = v3;

        if (IsOnGround(mPos))
            Gizmos.color = Color.white;
        else
            Gizmos.color = Color.red;
        Gizmos.DrawSphere(mPos, 0.2f);

        Vector2 relativePos = ((Vector2)mPos - instance.Offset) / Instance.tileSize;
        relativePos.y = -relativePos.y;


        //if (areas != null)
        //{
        //    for (int i = 0; i < areas.Count; i++)
        //    {
        //        if (new Rect(-0.5f * tileSize + Offset.x, -0.5f * tileSize - Offset.y, blueprint.columns * tileSize, blueprint.rows * tileSize).Contains(mPos))
        //        {
        //            int x = -Mathf.FloorToInt((mPos.y - Offset.y )/ tileSize + 0.5f);
        //            int y = Mathf.FloorToInt((mPos.x - Offset.x) / tileSize + 0.5f);
        //            if (isValidTile(x, y))
        //            {
        //                Gizmos.color = new Color(0.1f, 0.8f, 1f, 0.01f);
        //                Gizmos.DrawCube(new Vector3(y * tileSize, -x * tileSize, 0) + (Vector3)Offset, new Vector3(tileSize, tileSize, 0));
        //            }
        //        }
        //    }
        //}
    }
    #endif
    bool isInLoS(Vector2 p1, Vector2 p2, bool reduceLongEdges = false)
    {
        // TODO: try using sub-divisions instead of a ray?
        // Other optimization?

        float d = (p2 - p1).magnitude;

        Vector2 step = (p2 - p1) / (d * 10f);
        Vector2 point = p1;
        float stepLength = step.magnitude;
        Vector2 normal = new Vector2(step.y, -step.x).normalized * 0.5f; //half-width of a tank = 0.5f
        for (float i = 0; i < d; i += stepLength)
        {
            if (!IsOnGround(new Vector2(point.x + normal.x, point.y + normal.y)) || !IsOnGround(new Vector2(point.x - normal.x, point.y - normal.y)))
            {
                //Debug.Log("failed" + p1 + " " + p2);
                return false;
            }
            //if (reduceLongEdges)
            //{
            //    for (int j = 0; j < nodes.Count; j++)
            //    {
            //        if (nodes[j].pos != p1 && nodes[j].pos != p2)
            //        {
            //            float d2 = (point - nodes[j].pos).sqrMagnitude;
            //            if (d2 < 1f)
            //            {
            //                //Debug.Log("failed! A shorter route exists.");
            //                return false;
            //            }
            //        }
            //    }
            //}
            point += step;
        }

        //Debug.Log("passed" + p1 + " " + p2);
        return true;
    }

    public static Vector2[] pathfind(Vector2 startPos, Vector2 targetPos, float distance = 0f)
    {
        // Get platform
        var plat = Instance.GetPlatformInPosition(startPos);

        GroundPlatform.Tile? end = instance.GetNearestTile(plat, targetPos);
        GroundPlatform.Tile? start = instance.GetNearestTile(plat, startPos);

        float d = (startPos - targetPos).sqrMagnitude;
        float sqr = distance * distance;
        List<Vector2> path = new List<Vector2>();
        if (end.Value.pos == start.Value.pos && end.HasValue)
        {
            if (d > sqr)
            {
                path.Add(TileToWorldPos(end.Value.pos));
            }
            else
            {
                return null;
            }
        }

        int iteration = 0;
        GroundPlatform.Tile current = start.Value;
        
        while (current.pos != end.Value.pos && d > sqr)
        {
            byte dir = 0;
            if (current.directions.ContainsKey(end.Value.pos))
            {
                dir = current.directions[end.Value.pos];
                GroundPlatform.Tile? next = null;
                switch (dir)
                {
                    case 0:
                        next = plat.GetTile(current.pos + Vector2Int.right);
                        break;
                    case 1:
                        next = plat.GetTile(current.pos + Vector2Int.down);
                        break;
                    case 2:
                        next = plat.GetTile(current.pos + Vector2Int.left);
                        break;
                    case 3:
                        next = plat.GetTile(current.pos + Vector2Int.up);
                        break;
                    default:
                        break;
                }

                if (next.HasValue)
                {
                    current = next.Value;
                    path.Add(TileToWorldPos(current.pos));
                }
                else
                {
                    Debug.LogError("Pathfinding failed because of corrupted direction data at " + current.pos + " with direction " + dir);
                    return null;
                }
            }
            else
            {
                Debug.LogError("Pathfinding failed because of incomplete ground platform generation.");
                return null;
            }

            d = (targetPos - TileToWorldPos(current.pos)).sqrMagnitude;

            iteration++;
            if (iteration > 10000)
            {
                string s = "";
                for (int i = 0; i < path.Count; i++)
                {
                    s += path[i].ToString() + '\n';
                }

                Debug.Log(s);

                Debug.LogError("Infinite loop in pathfinding!");
                return null;
            }
        }

        // Get closer from the tile center if needed
        //d = (current.pos - targetPos).magnitude;
        //if (d > distance && path.Count > 0)
        //{
        //    path[path.Count - 1] = current.pos + (current.pos - targetPos).normalized * (d - distance);
        //}

        path.Reverse();

        string pathString = "";
        for (int i = 0; i < path.Count; i++)
        {
            pathString += path[i].ToString();
        }

        if (path.Count > 1)
        {
            d = (startPos - path[path.Count - 1]).magnitude;
            if ((start.Value.type == 0 ||
                start.Value.type == 7 ||
                start.Value.type == 5 ||
                start.Value.type == 10 )
                && d > instance.tileSize) // Don't skip turns
                path.Add(TileToWorldPos(start.Value.pos));
        }


        // Debug.Log("Path: " + pathString);

        return path.ToArray();
    }

    public static Vector2 TileToWorldPos(Vector2Int pos)
    {
        Vector2 sectorPos = new Vector2(pos.x, -pos.y) * instance.tileSize + instance.Offset;
        return sectorPos;
    }

    GroundPlatform.Tile? GetNearestTile(GroundPlatform platform, Vector2 pos, float maxDist = float.MaxValue)
    {
        Vector2 relativePos = (pos - instance.Offset) / Instance.tileSize;
        relativePos.y = -relativePos.y;

        float minDist = maxDist * maxDist;
        GroundPlatform.Tile? tile = null; // When would this return null? Max distance parameter?

        for (int i = 0; i < platform.tiles.Count; i++)
        {
            float d = (relativePos - platform.tiles[i].pos).sqrMagnitude;
            if (d < minDist)
            {
                minDist = d;
                tile = platform.tiles[i];
            }
        }
        return tile;
    }

    protected GroundPlatform GetPlatformInPosition(Vector2 pos)
    {
        Vector2 relativePos = (pos - Offset) / tileSize;
        relativePos.y = -relativePos.y;

        for (int i = 0; i < groundPlatforms.Length; i++)
        {
            var plat = groundPlatforms[i];
            var tilePos = new Vector2Int(Mathf.RoundToInt(relativePos.x), Mathf.RoundToInt(relativePos.y));
            if (!plat.tiles.Exists(t => t.pos == tilePos))
                continue;
            return plat;
        }
        return null;
    }

    /*
    public GameObject[] GetTilesByID(int ID) {
        List<GameObject> tilesToReturn = new List<GameObject>();
        foreach(GameObject tile in areaIDByTile.Keys) {
            if(areaIDByTile[tile] == ID) {
                tilesToReturn.Add(tile);
            }
        }
        return tilesToReturn.ToArray();
    }
    */
}