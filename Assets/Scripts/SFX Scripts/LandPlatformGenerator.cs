using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandPlatformGenerator : MonoBehaviour {


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
    public LandPlatform blueprint;
    private Dictionary<int, GameObject> tiles;
    private List<Rect> areas;
    private List<NavigationNode> nodes;
    private Vector2 center;

    private Dictionary<NavigationNode, int> areaIDByNode;
    private Dictionary<GameObject, int> areaIDByTile;
    public float tileSize { get; private set; }
    private Color color;
    private Vector2 offset;

    public static bool IsOnGround(Vector3 position)
    {
        var cols = Instance.blueprint.columns;
        var rows = Instance.blueprint.rows;
        Vector2 offset = new Vector2
        {
            x = Instance.center.x - Instance.tileSize * (cols - 1) / 2F,
            y = Instance.center.y + Instance.tileSize * (rows - 1) / 2F
        };
        Vector2 relativePos = ((Vector2)position - offset) / Instance.tileSize;
        relativePos.y = -relativePos.y;
        int index = Mathf.RoundToInt(relativePos.x) + Mathf.RoundToInt(relativePos.y) * cols;
        if (Instance.tiles.ContainsKey(index))
        {
            GameObject tile = Instance.tiles[index];
            if (tile.GetComponents<Collider2D>().Any(x => x.OverlapPoint(position)))
            {
                return true;
            }
        }
        return false;
    }

    public void SetColor(Color color) {
        this.color = color;
    }
    public void BuildTiles(LandPlatform platform, Vector2 center) {

        this.center = center;
        blueprint = platform;

        if (blueprint == null || blueprint.prefabs.Length <= 0)
            return;
        
        if(tiles != null) Unload();

        tileSize = ResourceManager.GetAsset<GameObject>(blueprint.prefabs[0]).GetComponent<SpriteRenderer>().bounds.size.x;

        var cols = blueprint.columns;
        var rows = blueprint.rows;
        offset = new Vector2 
        {
            x = center.x - tileSize * (cols-1)/2F,
            y = center.y + tileSize * (rows-1)/2F
        };
        tiles = new Dictionary<int, GameObject>();
        areas = new List<Rect>();

        for(int i = 0; i < blueprint.tilemap.Length; i++) {

            var pos = new Vector3
            {
                x = offset.x + tileSize * (i % cols),
                y = offset.y - tileSize * (i / cols),
                z = 0
            };

            switch(blueprint.tilemap[i]) {
                case -1:
                    break;
                default:
                    var tile = Instantiate(ResourceManager.GetAsset<GameObject>(blueprint.prefabs[blueprint.tilemap[i]]), pos, Quaternion.identity);
                    tile.transform.localEulerAngles = new Vector3(0,0,90 * blueprint.rotations[i]);
                    tile.GetComponent<SpriteRenderer>().color = color;
                    tiles.Add(i, tile);
                    tile.transform.parent = transform;
                    areas.Add(new Rect(pos.x, pos.y, tileSize, tileSize));
                    break;
            }
        }
    }

    public void Initialize()
    {
        Instance = this;
        nodes = new List<NavigationNode>();
        tiles = new Dictionary<int, GameObject>();
    }

    public void Unload()
    {
        foreach (var tile in tiles)
        {
            Destroy(tile.Value);
        }
        tiles.Clear();
        nodes.Clear();
    }

    [System.Serializable]
    class NavigationNode
    {
        public static bool operator ==(NavigationNode a, NavigationNode b)
        {
            if (ReferenceEquals(a, null))
                return ReferenceEquals(b, null);
            if (ReferenceEquals(b, null))
                return ReferenceEquals(a, null);
            return a.Equals(b);
        }
        public static bool operator !=(NavigationNode a, NavigationNode b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null && this == null)
                return true;

            if (!(obj is NavigationNode))
            {
                return false;
            }

            var node = (NavigationNode)obj;
            return pos.Equals(node.pos);
        }

        public override int GetHashCode()
        {
            return 991532785 + EqualityComparer<Vector2>.Default.GetHashCode(pos);
        }

        public Vector2 pos;
        [System.NonSerialized]
        public List<NavigationNode> neighbours;
        public List<float> distances;
        public NavigationNode(Vector2 position)
        {
            pos = position;
            neighbours = new List<NavigationNode>();
            distances = new List<float>();
        }
    }

    public void LoadNodes(LandPlatform.GroundNode[] nodeData)
    {
        if (nodeData == null)
            return;

        nodes = new List<NavigationNode>();

        // Load nodes
        for (int i = 0; i < nodeData.Length; i++)
        {
            var node = new NavigationNode(nodeData[i].pos);
            nodes.Add(node);
        }

        int debugCount = 0;

        // Connect nodes
        for (int i = 0; i < nodeData.Length; i++)
        {
            for (int j = i + 1; j < nodeData.Length; j++)
            {
                for (int k = 0; k < nodeData[i].neighbours.Length; k++)
                {
                    if (nodeData[i].neighbours[k] == nodeData[j].ID)
                    {
                        nodes[i].neighbours.Add(nodes[j]);
                        nodes[j].neighbours.Add(nodes[i]);
                        nodes[i].distances.Add(nodeData[i].distances[k]);
                        nodes[j].distances.Add(nodeData[i].distances[k]);
                        debugCount++;
                    }
                }
            }
        }

        // Generate dictionaries
        areaIDByNode = new Dictionary<NavigationNode, int>();
        areaIDByTile = new Dictionary<GameObject, int>();

        int currentAreaID = 0;
        foreach (NavigationNode node in nodes)
        {
            if (!areaIDByNode.ContainsKey(node))
            {
                RecursivelyDefineIDs(node, currentAreaID++);
            }
        }

        foreach (var pair in tiles)
        {
            GameObject tile = pair.Value;
            if (areaIDByTile.ContainsKey(tile)) continue;
            foreach (NavigationNode node in nodes)
            {
                if (isInLoS(tile.transform.position, node.pos))
                {
                    areaIDByTile.Add(tile, areaIDByNode[node]);
                    break;
                }
            }
        }
        List<int> ids = new List<int>();
        foreach (int ID in areaIDByTile.Values)
        {
            if (!ids.Contains(ID))
            {
                ids.Add(ID);
            }
        }
        Debug.Log("Done! Nodes: " + nodes.Count + " Connections: " + debugCount + " Landmasses: " + ids.Count);
    }

    public static LandPlatform.GroundNode[] BuildNodes(LandPlatform platform, Vector2 center)
    {
        Debug.Log("Generating land tiles...");
        Instance.BuildTiles(platform, center);

        Debug.Log("Building nodes...");

        float tileSize = Instance.tileSize;
        Vector2 offset = Instance.offset;
        float dToCenter = tileSize / 5f; // node distance to center on one axis
        Instance.nodes = new List<NavigationNode>();

        for (int i = 0; i < platform.rows; i++)
        {
            for (int j = 0; j < platform.columns; j++)
            {
                if (platform.tilemap[i * platform.columns + j] > -1)
                {
                    //create nodes
                    if (!Instance.isValidTile(i, j))
                        continue;

                    bool right = Instance.isValidTile(i, j + 1);
                    bool up = Instance.isValidTile(i - 1, j);
                    bool left = Instance.isValidTile(i, j - 1);
                    bool down = Instance.isValidTile(i + 1, j);

                    // check if 2-entry straight road
                    if ((!right && !left && up && down) || (right && left && !up && !down))
                        continue;

                    if (platform.prefabs[platform.tilemap[i * platform.columns + j]] == "New 2 Entry")
                    {
                        if (IsOnGround(new Vector2(j * tileSize, -i * tileSize) + offset))
                            Instance.nodes.Add(new NavigationNode(new Vector2(j * tileSize, -i * tileSize) + offset));
                        continue;
                    }

                    // Square node pattern
                    if (IsOnGround(new Vector2(j * tileSize + dToCenter, -i * tileSize + dToCenter) + offset))
                        Instance.nodes.Add(new NavigationNode(new Vector2(j * tileSize + dToCenter, -i * tileSize + dToCenter) + offset));
                    if (IsOnGround(new Vector2(j * tileSize - dToCenter, -i * tileSize + dToCenter) + offset))
                        Instance.nodes.Add(new NavigationNode(new Vector2(j * tileSize - dToCenter, -i * tileSize + dToCenter) + offset));
                    if (IsOnGround(new Vector2(j * tileSize - dToCenter, -i * tileSize - dToCenter) + offset))
                        Instance.nodes.Add(new NavigationNode(new Vector2(j * tileSize - dToCenter, -i * tileSize - dToCenter) + offset));
                    if (IsOnGround(new Vector2(j * tileSize + dToCenter, -i * tileSize - dToCenter) + offset))
                        Instance.nodes.Add(new NavigationNode(new Vector2(j * tileSize + dToCenter, -i * tileSize - dToCenter) + offset));

                    // Diamond node pattern
                    //if (IsOnGround(new Vector2(j * tileSize + dToCenter, -i * tileSize) + offset))
                    //    nodes.Add(new NavigationNode(new Vector2(j * tileSize + dToCenter, -i * tileSize) + offset));
                    //if (IsOnGround(new Vector2(j * tileSize, -i * tileSize + dToCenter) + offset))
                    //    nodes.Add(new NavigationNode(new Vector2(j * tileSize, -i * tileSize + dToCenter) + offset));
                    //if (IsOnGround(new Vector2(j * tileSize - dToCenter, -i * tileSize) + offset))
                    //    nodes.Add(new NavigationNode(new Vector2(j * tileSize - dToCenter, -i * tileSize) + offset));
                    //if (IsOnGround(new Vector2(j * tileSize, -i * tileSize - dToCenter) + offset))
                    //    nodes.Add(new NavigationNode(new Vector2(j * tileSize, -i * tileSize - dToCenter) + offset));
                }
            }
        }
        //connect nodes
        Debug.Log("Connecting nodes...");
        float maxDistanceSqr = (tileSize * 2f) * (tileSize * 2f);
        for (int i = 0; i < Instance.nodes.Count; i++)
        {
            for (int j = i + 1; j < Instance.nodes.Count; j++)
            {
                if (Instance.isInLoS(Instance.nodes[i].pos, Instance.nodes[j].pos, true))
                {
                    Instance.nodes[i].neighbours.Add(Instance.nodes[j]);
                    Instance.nodes[j].neighbours.Add(Instance.nodes[i]);
                    float d = (Instance.nodes[i].pos - Instance.nodes[j].pos).magnitude;
                    Instance.nodes[i].distances.Add(d);
                    Instance.nodes[j].distances.Add(d);
                }
            }
        }

        // Convert navigation nodes to storage format
        LandPlatform.GroundNode[] storageNodes = new LandPlatform.GroundNode[Instance.nodes.Count];
        for (int i = 0; i < Instance.nodes.Count; i++)
        {
            int[] neighbours = new int[Instance.nodes[i].neighbours.Count];
            float[] distances = new float[Instance.nodes[i].neighbours.Count];
            for (int j = 0; j < neighbours.Length; j++)
            {
                for (int k = 0; k < Instance.nodes.Count; k++)
                {
                    if (Instance.nodes[i].neighbours[j] == Instance.nodes[k])
                    {
                        neighbours[j] = k;
                        distances[j] = Instance.nodes[i].distances[j];
                        break;
                    }
                }
            }
            storageNodes[i] = new LandPlatform.GroundNode()
            {
                ID = i,
                pos = Instance.nodes[i].pos,
                neighbours = neighbours,
                distances = distances
            };
        }

        Instance.Unload();

        return storageNodes;
    }


    void RecursivelyDefineIDs(NavigationNode node, int ID) {
        if(!areaIDByNode.ContainsKey(node)) {
            areaIDByNode.Add(node, ID);
        } else if (areaIDByNode[node] != ID) {
            areaIDByNode[node] = ID;
        } else return;
        foreach(NavigationNode neighbor in node.neighbours) {
            RecursivelyDefineIDs(neighbor, ID);
        }
    }

    bool isValidTile(int x, int y)
    {
        bool limitCheck = x < blueprint.rows && y < blueprint.columns && x >= 0 && y >= 0;
        bool selfIsTile = limitCheck && blueprint.tilemap[x * blueprint.columns + y] > -1;
        bool selfIsWithinLengthLimit = selfIsTile && blueprint.tilemap[x * blueprint.columns + y] < blueprint.prefabs.Length;
        bool final = selfIsWithinLengthLimit && blueprint.prefabs[blueprint.tilemap[x * blueprint.columns + y]] != null;
        return final;
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (blueprint == null)
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

        if (areas != null)
        {
            for (int i = 0; i < areas.Count; i++)
            {
                if (new Rect(-0.5f * tileSize + offset.x, -0.5f * tileSize - offset.y, blueprint.columns * tileSize, blueprint.rows * tileSize).Contains(mPos))
                {
                    int x = -Mathf.FloorToInt((mPos.y - offset.y )/ tileSize + 0.5f);
                    int y = Mathf.FloorToInt((mPos.x - offset.x) / tileSize + 0.5f);
                    if (isValidTile(x, y))
                    {
                        Gizmos.color = new Color(0.1f, 0.8f, 1f, 0.01f);
                        Gizmos.DrawCube(new Vector3(y * tileSize, -x * tileSize, 0) + (Vector3)offset, new Vector3(tileSize, tileSize, 0));
                    }
                }
            }
        }

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Gizmos.color = new Color(0, 100, 255);
                Gizmos.DrawSphere(nodes[i].pos, 0.2f);
                UnityEditor.Handles.Label(nodes[i].pos + Vector2.right, nodes[i].pos.ToString());
                Gizmos.color = new Color(200, 0, 0, 100);
                for (int j = 0; j < nodes[i].neighbours.Count; j++)
                {
                    Gizmos.DrawLine(nodes[i].pos, nodes[i].neighbours[j].pos);
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
            if (reduceLongEdges)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    if (nodes[j].pos != p1 && nodes[j].pos != p2)
                    {
                        float d2 = (point - nodes[j].pos).sqrMagnitude;
                        if (d2 < 1f)
                        {
                            //Debug.Log("failed! A shorter route exists.");
                            return false;
                        }
                    }
                }
            }
            point += step;
        }

        //Debug.Log("passed" + p1 + " " + p2);
        return true;
    }

    private class PathfindNode
    {
        public NavigationNode node;
        public PathfindNode parent;
        public float d;

        public PathfindNode(NavigationNode node, PathfindNode parent, float distance)
        {
            this.node = node;
            this.parent = parent;
            d = distance;
        }
    }

    // TODO: multithread
    public static Vector2[] pathfind(Vector2 startPos, Vector2 targetPos)
    {
        if (Instance.blueprint == null)
            return null;

        if (Instance.nodes == null)
            return null;

        //find node closest to start and end positions
        NavigationNode start = getNearestNode(startPos, true);
        NavigationNode end = getNearestNode(targetPos, true);

        if (end == null)
        {
            Debug.Log("Getting another end point...");
            end = getNearestNode(targetPos, false);
            Debug.Log("End = " + end.pos);
        }
        if (start == null)
        {
            Debug.LogWarning("No start node found!");
            return null;
        }

        var openList = new List<PathfindNode>();
        var closedList = new List<PathfindNode>();
        openList.Capacity = Instance.nodes.Count;
        closedList.Capacity = Instance.nodes.Count;

        openList.Add(new PathfindNode(start, null, 0f));
        
        if(Instance.areaIDByNode.ContainsKey(start) && Instance.areaIDByNode.ContainsKey(end) 
        && Instance.areaIDByNode[start] != Instance.areaIDByNode[end])  // If start & end are on different platforms
        {
            GameObject[] tiles = Instance.GetTilesByID(Instance.areaIDByNode[start]); // Get tiles of the start platform
            GameObject closestTile = null; // Find the tile closest to the target position
            float distance = float.MaxValue;
            foreach(GameObject tile in tiles)
            {
                var dist = ((Vector2)tile.transform.position - targetPos).magnitude;
                if(dist < distance)
                {
                    closestTile = tile;
                    distance = dist;
                }
                else if(dist - distance < 0.01F) // If the distance is identical, choose the one closer to the start position
                {
                    var mag1 = (closestTile.transform.position - (Vector3)startPos).magnitude;
                    var mag2 = (tile.transform.position - (Vector3)startPos).magnitude;
                    if(mag1 - mag2 > 0.01F)
                    {
                        closestTile = tile;
                        distance = dist;
                    }
                }
            }
            if(closestTile != null) {
                Vector3 dist = targetPos;
                Vector3 tilePos = closestTile.transform.position;
                Vector3 offset = Vector3.zero;
                bool[] resetDist = new bool[2];
                if((int)(dist.x - tilePos.x) % (int)(Instance.tileSize / 2) == 0) {
                    offset.y = tilePos.y > dist.y  ? -Instance.tileSize / 3F : Instance.tileSize / 3F;
                    resetDist[0] = true;
                }
                if((int)(dist.y - tilePos.y) % (int)(Instance.tileSize / 2) == 0) {
                    offset.x = tilePos.x > dist.x  ? -Instance.tileSize / 3F : Instance.tileSize / 3F;
                    resetDist[1] = true;
                }
                for(int i = 0; i < 2; i++) {
                    if(resetDist[i]) dist[i] = 0;
                }
                targetPos = offset + tilePos + dist.normalized * Instance.tileSize / 3F;
                end = getNearestNode(tilePos, true);
            }
            else
            {
                Debug.LogWarning("No closest tile found!");
                return null;
            }
        }
        if (Mathf.Abs((startPos - targetPos).magnitude) < 0.01F) {
            return null;
        }
        if (Instance.isInLoS(startPos, targetPos)) {
            return new Vector2[] {targetPos};
        } else if (start == end) {
            return new Vector2[] {end.pos};
        }

        Debug.DrawLine(start.pos, start.pos + Vector2.up, Color.red, 3f);
        Debug.DrawLine(end.pos, end.pos + Vector2.right, Color.green, 3f);
        Debug.Log("Start pos : " + start.pos);
        Debug.Log("End pos : " + end.pos);

        while (openList.Count > 0)
        {
            if (openList.Count > Instance.nodes.Count * 2)
            {
                Debug.LogWarning("Pathfind error!");
                Debug.Log("Open: " + openList.Count);
                Debug.Log("Closed: " + closedList.Count);
                return null;
            }

            // Get next node with shortest total distance
            PathfindNode current = openList[0];
            float shortest = float.MaxValue;
            for (int i = 0; i < openList.Count; i++)
            {
                if(openList[i].d < shortest)
                {
                    shortest = openList[i].d;
                    current = openList[i];
                }
            }

            // Check if the goal has been reached
            if(current.node == end)
            {
                var path = new List<Vector2>();
                PathfindNode node = current;
                do
                {
                    path.Add(node.node.pos);
                    node = node.parent;
                }
                while (node != null && node.parent != null);

                // Try skipping the first node
                if(!Instance.isInLoS(path[path.Count - 1], startPos))
                    path.Add(node.node.pos);

                if(Instance.isInLoS(end.pos, targetPos)) 
                {
                    // if the second last path is in the line of sight you can skip the last one to the target
                    if(path.Count > 1 && Instance.isInLoS(path[1], targetPos)) {
                        path[0] = targetPos;
                    } else // otherwise add the target position as the last destination
                        path.Insert(0, targetPos);
                }
                Debug.Log("Pathfind success!");
                return path.ToArray();
            }

            for(int i = 0; i < current.node.neighbours.Count; i++)
            {
                NavigationNode neighbour = current.node.neighbours[i];
                bool closed = false;
                for(int j = 0; j < closedList.Count; j++)
                {
                    if(closedList[j].node == neighbour)
                    {
                        closed = true;
                        break;
                    }
                }
                for (int j = 0; j < openList.Count; j++)
                {
                    if (openList[j].node == neighbour)
                    {
                        if (openList[j].d > current.d + current.node.distances[i])
                        {
                            openList[j].d = current.d + current.node.distances[i];
                            openList[j].parent = current;
                        }
                        closed = true;
                        break;
                    }
                }
                if (!closed)
                {
                    openList.Add(new PathfindNode(neighbour, current, current.d + current.node.distances[i]));
                }
            }

            openList.Remove(current);
            closedList.Add(current);
        }
        Debug.Log("Pathfinding failed! OL empty.");
        return null;
    }

    public GameObject[] GetTilesByID(int ID) {
        List<GameObject> tilesToReturn = new List<GameObject>();
        foreach(GameObject tile in areaIDByTile.Keys) {
            if(areaIDByTile[tile] == ID) {
                tilesToReturn.Add(tile);
            }
        }
        return tilesToReturn.ToArray();
    }

    static NavigationNode getNearestNode(Vector2 pos, bool los = false)
    {
        NavigationNode nearest = null;
        float minD = float.MaxValue;
        for (int i = 0; i < Instance.nodes.Count; i++)
        {
            float d = (pos - Instance.nodes[i].pos).sqrMagnitude;
            if (d < minD)
            {
                if (los && !Instance.isInLoS(Instance.nodes[i].pos, pos))
                {
                    continue;
                }
                nearest = Instance.nodes[i];
                minD = d;
            }
        }
        return nearest;
    }
}