using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class LandPlatformGenerator : MonoBehaviour
{


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
        private set { instance = value; }
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

    public float tileSize { get; set; }
    public Color color { get; private set; }
    public Vector2 Offset { get; set; }
    public Vector2Int Size { get; set; }

    private Queue<Entity> searchQueue = new Queue<Entity>();
    private short[,] directionMap;
    private Vector2 center;

    // private Dictionary<GameObject, int> areaIDByTile; // TODO: Add platIDByTile

    public static bool IsOnGround(Vector3 position)
    {
        if (Instance.groundPlatforms == null)
        {
            return false;
        }

        if (Instance.tileSize == 0)
        {
            Debug.LogError("Tile size = 0");
        }

        if (Instance.Offset == Vector2.zero)
        {
            Debug.LogError("Offset = 0");
        }

        Vector2 relativePos = ((Vector2)position - instance.Offset) / Instance.tileSize;
        relativePos.y = -relativePos.y;

        for (int i = 0; i < Instance.groundPlatforms.Length; i++)
        {
            var plat = Instance.groundPlatforms[i];

            GroundPlatform.Tile? tile = plat.GetTile(new Vector2Int(Mathf.RoundToInt(relativePos.x), Mathf.RoundToInt(relativePos.y)));
            if (tile.HasValue && tile.Value.colliders.Any(x => x.OverlapPoint(position)))
            {
                return true;
            }
        }

        return false;
    }

    public void SetColor(Color color)
    {
        this.color = color;
        if (groundPlatforms != null)
        {
            for (int i = 0; i < groundPlatforms.Length; i++)
            {
                for (int j = 0; j < groundPlatforms[i].tiles.Count; j++)
                {
                    var obj = groundPlatforms[i].tiles?[j].colliders?[0];
                    if (obj)
                    {
                        obj.GetComponent<SpriteRenderer>().color = color;
                    }
                }
            }
        }
    }

    public void LoadSector(Sector sector)
    {
        searchQueue.Clear();
        for (int i = 0; i < AIData.entities.Count; i++)
        {
            searchQueue.Enqueue(AIData.entities[i]);
        }

        Vector2 center = new Vector2(sector.bounds.x + sector.bounds.w / 2, sector.bounds.y - sector.bounds.h / 2);

        if (sector.platform) // Old data
        {
            BuildTiles(sector.platform, center);
        }
        else if (sector.platformData.Length > 0)
        {
            GameObject[] prefabs = new GameObject[prefabNames.Length];
            for (int i = 0; i < prefabNames.Length; i++)
            {
                prefabs[i] = ResourceManager.GetAsset<GameObject>(prefabNames[i]);
            }

            tileSize = prefabs[0].GetComponent<SpriteRenderer>().bounds.size.x;

            var cols = sector.bounds.w / (int)tileSize;
            var rows = sector.bounds.h / (int)tileSize;

            Offset = new Vector2
            {
                x = center.x - tileSize * (cols - 1) / 2F,
                y = center.y + tileSize * (rows - 1) / 2F
            };

            Size = new Vector2Int
            {
                x = cols,
                y = rows
            };

            directionMap = new short[cols, rows];
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    directionMap[i, j] = -1;
                }
            }

            // Assemble platforms from tile prefabs
            sector.platforms = new GroundPlatform[sector.platformData.Length];
            for (int i = 0; i < sector.platformData.Length; i++)
            {
                var plat = new GroundPlatform(sector.platformData[i], prefabs, this);
                sector.platforms[i] = plat;
            }

            groundPlatforms = sector.platforms;

            // Create the direction map
            for (int i = 0; i < groundPlatforms.Length; i++)
            {
                for (int j = 0; j < groundPlatforms[i].tiles.Count; j++)
                {
                    GroundPlatform.Tile t = groundPlatforms[i].tiles[j];
                    if (t.pos.x >= cols || t.pos.y >= rows || t.pos.x < 0 || t.pos.y < 0)
                    {
                        Debug.LogWarning($"Invalid tile position: { t.pos } Bounds: {cols}, {rows}");
                        continue;
                    }
                    directionMap[t.pos.x, t.pos.y] = GroundPlatform.GetPlatformOpenings(t);
                }
            }


            // Direction map debug
            string str = "";
            for (int j = 0; j < rows; j++)
            {
                for (int i = 0; i < cols; i++)
                {
                    str += directionMap[i, j].ToString().PadLeft(2, '0') + ' ';
                }
                str += '\n';
            }
        }
        if (directionMap != null) Debug.Log("Direction map: " + directionMap.Length);
    }

    private void Update()
    {
        if (searchQueue.Count > 0 && groundPlatforms != null)
        {
            // Search closest tiles
            Entity ent = searchQueue.Dequeue();

            for (int i = 0; i < groundPlatforms.Length; i++)
            {
                // Is this algorithm optimal?
                if (ent && !ent.GetIsDead())
                    groundPlatforms[i].GetClosestTile(ent);
            }
        }
    }

    public void BuildTiles(LandPlatform platform, Vector2 center)
    {
        this.center = center;
        var blueprint = platform;

        tileSize = ResourceManager.GetAsset<GameObject>(blueprint.prefabs[0]).GetComponent<SpriteRenderer>().bounds.size.x;

        var cols = blueprint.columns;
        var rows = blueprint.rows;
        Offset = new Vector2
        {
            x = center.x - tileSize * (cols - 1) / 2F,
            y = center.y + tileSize * (rows - 1) / 2F
        };
        // TODO: read new data from file, for each platform

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
                    short ends = GroundPlatform.GetPlatformOpenings(current);

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
                {
                    platTiles.Add(tiles[j]);
                }
            }

            platforms.Add(new GroundPlatform(platTiles.ToArray()));
        }

        return platforms.ToArray();
    }

    public void Initialize()
    {
        Instance = this;
        groundPlatforms = null;
        Entity.OnEntitySpawn += EnqueueEntity;
    }

    public void Unload()
    {
        if (groundPlatforms != null)
        {
            foreach (var plat in groundPlatforms)
            {
                plat.Clear();
            }
        }

        groundPlatforms = null;
        searchQueue.Clear();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundPlatforms == null)
        {
            return;
        }

        var v3 = Input.mousePosition;
        v3.z = 10.0f;
        v3 = Camera.main.ScreenToWorldPoint(v3);
        Vector2 mPos = v3;

        if (IsOnGround(mPos))
        {
            Gizmos.color = Color.white;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawSphere(mPos, 0.2f);

        Vector2 relativePos = (mPos - instance.Offset) / Instance.tileSize;
        relativePos.y = -relativePos.y;

        Vector2Int tilePos = new Vector2Int(Mathf.RoundToInt(relativePos.x), Mathf.RoundToInt(relativePos.y));

        Gizmos.DrawCube(TileToWorldPos(tilePos), Vector3.one * tileSize);

        // Initialize direction vectors
        Vector2Int[] unitVectors = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1)
        };

        Gizmos.color = Color.white;

        for (int i = 0; i < Size.x; i++)
        {
            for (int j = 0; j < Size.y; j++)
            {
                if (directionMap[i, j] != -1)
                {
                    if ((directionMap[i, j] & (1 << i)) == (1 << i))
                    {
                        Gizmos.DrawCube(TileToWorldPos(new Vector2Int(i, j)) + (Vector2)unitVectors[i] * (tileSize / 3f), Vector3.one * tileSize / 8f);
                    }
                }
            }
        }
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

    class Node
    {
        public Vector2Int pos;
        public short directions;
        public Node parent;
    }

    public static Vector2[] pathfind(Vector2 startPos, Entity[] targets, Vector2[] positions, float maxDistance = 0f)
    {
        float sqrDist = maxDistance * maxDistance;

        // Get correct platform and the starting tile
        var plat = Instance.GetPlatformInPosition(startPos);
        Vector2Int startTilePos = WorldToTilePos(startPos);

        // Get end tiles
        List<Vector2Int> endTiles = new List<Vector2Int>();
        if(targets != null){
            for (int i = 0; i < targets.Length; i++)
            {
                var t = plat.GetClosestTile(targets[i]);
                if ((TileToWorldPos(t.pos) - (Vector2)targets[i].transform.position).sqrMagnitude < sqrDist)
                {
                    endTiles.Add(t.pos);
                }
            }
        }
        else{
            for (int i = 0; i < positions.Length; i++)
            {
                var t = instance.GetNearestTile(plat, positions[i]).Value;
                if ((TileToWorldPos(t.pos) - positions[i]).sqrMagnitude < sqrDist)
                {
                    endTiles.Add(t.pos);
                }
            }
        }

        if (endTiles.Count == 0)
        {
            return null;
        }

        if (endTiles.Contains(startTilePos))
        {
            return new Vector2[] { TileToWorldPos(startTilePos) };
        }

        // Initialize node lists
        // TODO: Queue instead of List?
        List<Node> openList = new List<Node>
        {
            new Node
            {
                pos = startTilePos,
                directions = instance.GetDirections(startTilePos),
                parent = null
            }
        };
        List<Vector2Int> closedList = new List<Vector2Int>();

        // Initialize direction vectors
        Vector2Int[] unitVectors = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1)
        };

        // Start flood fill
        while (openList.Count > 0)
        {
            Node current = openList[0];

            // Check for end tiles
            for (int i = 0; i < endTiles.Count; i++)
            {
                if (current.pos == endTiles[i])
                {
                    // Path found!
                    List<Vector2> path = new List<Vector2>();
                    while (current.parent != null)
                    {
                        path.Add(TileToWorldPos(current.pos));
                        current = current.parent;

                        if (path.Count > 10000)
                        {
                            Debug.LogError("Infinite loop at path construction.");
                            return null;
                        }
                    }

                    // Add parentless start position
                    if ((current.directions == 3 ||
                        current.directions == 6 ||
                        current.directions == 9 ||
                        current.directions == 12) &&
                        path.Count > 1)
                    {
                        Vector2 last = (TileToWorldPos(current.pos));
                        Vector2 smoothed = (path[path.Count - 1] * 0.35f) + (last * 0.65f);
                        path.Add(smoothed);
                    }
                    else
                    {
                        path.Add(TileToWorldPos(current.pos));
                    }

                    if (path.Count > 1)
                    {
                        if (instance.isInLoS(startPos, path[path.Count - 2]))
                        {
                            path.RemoveAt(path.Count - 1);
                        }
                    }


                    List<Vector2> smooth = new List<Vector2> { path[0] };

                    // Path smoothing
                    if (path.Count > 2)
                    {
                        for (int j = 1; j < path.Count - 1; j++)
                        {
                            if (path[j - 1].x != path[j + 1].x && path[j - 1].y != path[j + 1].y)
                            {
                                Vector2 prev = (path[j - 1] * 0.35f) + (path[j] * 0.65f);
                                Vector2 next = (path[j + 1] * 0.35f) + (path[j] * 0.65f);

                                smooth.Add(prev);
                                smooth.Add(next);
                            }
                            else
                            {
                                smooth.Add(path[j]);
                            }
                        }
                    }
                    if (path.Count > 1)
                    {
                        smooth.Add(path[path.Count - 1]);
                    }

                    // Path from end to start. Tanks start fron the last node index.
                    return smooth.ToArray();
                }
            }

            // Add new nodes
            for (int i = 0; i < 4; i++)
            {
                if ((current.directions & (1 << i)) == (1 << i))
                {
                    Vector2Int nextPos = current.pos + unitVectors[i];
                    short dirs = instance.GetDirections(nextPos);
                    if (dirs == -1)
                    {
                        // Road ends!
                        continue;
                    }
                    if (!closedList.Contains(nextPos))
                    {
                        bool found = false;
                        for (int j = 0; j < openList.Count; j++)
                        {
                            if (openList[j].pos == nextPos)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            openList.Add(
                                new Node
                                {
                                    pos = nextPos,
                                    directions = dirs,
                                    parent = current
                                });
                        }
                    }
                }
            }

            openList.RemoveAt(0);
        }

        // It's possible for there to be no path to any target, so it's okay to return nothing
        Debug.Log($"No viable path found to any of the [{endTiles.Count}] destinations.");
        return null;
    }

    /// <summary>
    /// Get which directions are available from this tile position
    /// </summary>
    short GetDirections(Vector2Int pos)
    {
        return GetDirections(pos.x, pos.y);
    }

    short GetDirections(int x, int y)
    {
        if (x >= Size.x || y >= Size.y || x < 0 || y < 0)
        {
            return -1;
        }
        return directionMap[x, y];
    }

    internal static Vector2[] SeekAndPathfind(Vector2 startPos, Entity[] entities, float maxRange = 100f)
    {
        // Get platform
        var plat = Instance.GetPlatformInPosition(startPos);

        // Get Entities' closest tiles from platform
        // Flood fill

        return null;
    }

    public static Vector2 TileToWorldPos(Vector2Int pos)
    {
        Vector2 sectorPos = new Vector2(pos.x, -pos.y) * instance.tileSize + instance.Offset;
        return sectorPos;
    }

    public static Vector2Int WorldToTilePos(Vector2 pos)
    {
        Vector2 relativePos = (pos - instance.Offset) / Instance.tileSize;
        relativePos.y = -relativePos.y;

        Vector2Int intPos = new Vector2Int(Mathf.RoundToInt(relativePos.x), Mathf.RoundToInt(relativePos.y));
        return intPos;
    }

    // TODO: move this to Ground platform?
    public GroundPlatform.Tile? GetNearestTile(GroundPlatform platform, Vector2 pos, float maxDist = 100000f)
    {
        Vector2 relativePos = (pos - instance.Offset) / Instance.tileSize;
        relativePos.y = -relativePos.y;

        Vector2Int intPos = new Vector2Int(Mathf.RoundToInt(relativePos.x), Mathf.RoundToInt(relativePos.y));
        GroundPlatform.Tile? tileUnderPos = platform.GetTile(intPos);
        if (tileUnderPos.HasValue)
        {
            //Debug.Log($"Tile of {platform.offset} platform under start pos returned at {relativePos}");
            return tileUnderPos;
        }

        float scaledDistance = maxDist / Instance.tileSize;

        float minDist = scaledDistance * scaledDistance;
        GroundPlatform.Tile? tile = null; // Returns null if all tiles are outside max range

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
            {
                continue;
            }

            return plat;
        }

        return null;
    }

    public static void EnqueueEntity(Entity ent)
    {
        if (instance != null)
        {
            if (!Instance.searchQueue.Contains(ent))
                Instance.searchQueue.Enqueue(ent);
        }
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
