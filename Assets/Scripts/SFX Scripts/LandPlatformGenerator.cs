using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandPlatformGenerator : MonoBehaviour
{
    public static LandPlatformGenerator instance { private set; get; }

    public LandPlatform blueprint;

    private List<GameObject> tiles;
    private List<Rect> areas;
    private List<NavigationNode> nodes;

    private float tileSize;

    public static bool CheckOnGround(Vector3 position)
    {
        for (int i = 0; i < instance.tiles.Count; i++)
        {
            if (instance.tiles[i].GetComponent<SpriteRenderer>().bounds.Contains(position))
            {
                return true;
            }
        }
        return false;
    }

    public static Vector3 getDirection(Vector3 position)
    {
        return Vector3.zero;
    }

    struct NavigationNode
    {
        public static bool operator ==(NavigationNode x, NavigationNode y)
        {
            return x.pos == y.pos;
        }
        public static bool operator !=(NavigationNode x, NavigationNode y)
        {
            return x.pos != y.pos;
        }

        public override bool Equals(object obj)
        {
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
        public List<NavigationNode> neighbours;
        public List<float> distances;

        public NavigationNode(Vector2 position)
        {
            pos = position;
            neighbours = new List<NavigationNode>();
            distances = new List<float>();
        }
    }

    private void Awake()
    {
        instance = this;
    }

    public void Init(LandPlatform platform)
    {
        if(tiles != null)
            unload();

        tiles = new List<GameObject>();
        areas = new List<Rect>();

        blueprint = platform;

        if (blueprint == null)
            return;

        if (blueprint.prefabs.Length > 0)
        {
            tileSize = 1f;
            for(int i = 0; i < blueprint.prefabs.Length; i++)
                if(blueprint.prefabs[i] != null)
                {
                    tileSize = blueprint.prefabs[0].GetComponent<SpriteRenderer>().sprite.bounds.size.x;
                    break;
                }
            for (int i = 0; i < blueprint.rows; i++)
            {
                for (int j = 0; j < blueprint.columns; j++)
                {
                    if (blueprint.tilemap[i + j * blueprint.rows] > -1 && blueprint.tilemap[i + j * blueprint.rows] < blueprint.prefabs.Length && blueprint.prefabs[blueprint.tilemap[i + j * blueprint.rows]] != null)
                    {
                        GameObject tile = Instantiate(blueprint.prefabs[blueprint.tilemap[i + j * blueprint.rows]], new Vector3(j * tileSize, i * tileSize, 0), Quaternion.identity);
                        tiles.Add(tile);
                        tile.transform.SetParent(transform);

                        areas.Add(new Rect(j * tileSize, i * tileSize, tileSize, tileSize));
                    }
                }
            }
        }

        buildNodes();
    }

    public void unload()
    {
        for(int i = 0; i < tiles.Count; i++)
        {
            Destroy(tiles[i]);
        }
        tiles.Clear();
    }

    void buildNodes()
    {
        Debug.Log("Building nodes...");

        float dToCenter = tileSize / 3f; // node distance to center on one axis
        nodes = new List<NavigationNode>();
        for (int i = 0; i < blueprint.rows; i++)
        {
            for (int j = 0; j < blueprint.columns; j++)
            {
                if (blueprint.tilemap[i + j * blueprint.rows] > -1)
                {
                    //create nodes
                    if (!isValidTile(i, j))
                        continue;

                    bool right = isValidTile(i + 1, j);
                    bool up = isValidTile(i, j + 1);
                    bool left = isValidTile(i - 1, j);
                    bool down = isValidTile(i, j - 1);

                    if ((!right && !up) || (right && up && !isValidTile(i + 1, j + 1))) //check if the tile is a corner
                        nodes.Add(new NavigationNode(new Vector2(j * tileSize + dToCenter, i * tileSize + dToCenter)));
                    if ((!left && !up) || (left && up && !isValidTile(i - 1, j + 1)))
                        nodes.Add(new NavigationNode(new Vector2(j * tileSize + dToCenter, i * tileSize - dToCenter)));
                    if ((!left && !down) || (left && down && !isValidTile(i - 1, j - 1)))
                        nodes.Add(new NavigationNode(new Vector2(j * tileSize - dToCenter, i * tileSize - dToCenter)));
                    if ((!right && !down) || (right && down && !isValidTile(i + 1, j - 1)))
                        nodes.Add(new NavigationNode(new Vector2(j * tileSize - dToCenter, i * tileSize + dToCenter)));
                }
            }
        }

        int debugCount = 0;

        //connect nodes
        Debug.Log("Connecting nodes...");
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                if (isInLoS(nodes[i].pos, nodes[j].pos))
                {
                    nodes[i].neighbours.Add(nodes[j]);
                    nodes[j].neighbours.Add(nodes[i]);
                    float d = (nodes[i].pos - nodes[j].pos).magnitude;
                    nodes[i].distances.Add(d);
                    nodes[j].distances.Add(d);
                    debugCount++;
                }
            }
        }

        Debug.Log("Done! Nodes: " + nodes.Count + " Connections: " + debugCount);
    }

    bool isValidTile(int x, int y)
    {
        return x < blueprint.rows && y < blueprint.columns && x >= 0 && y >= 0 && blueprint.tilemap[x + y * blueprint.rows] > -1 && blueprint.tilemap[x + y * blueprint.rows] < blueprint.prefabs.Length && blueprint.prefabs[blueprint.tilemap[x + y * blueprint.rows]] != null;
    }

    bool isInLoS(Vector2 p1, Vector2 p2)
    {
        Vector2 p12 = p1 / tileSize + Vector2.one * 0.5f;
        Vector2 p22 = p2 / tileSize + Vector2.one * 0.5f;

        float d = (p22 - p12).magnitude;

        Vector2 step = (p22 - p12) / (d * 10f);
        Vector2 point = p12;
        float stepLength = step.magnitude;
        //TODO: get normals, use them
        for (float i = 0; i < d; i += stepLength)
        {
            if (!isValidTile(Mathf.FloorToInt(point.y), Mathf.FloorToInt(point.x)))
            {
                return false;
            }
            point += step;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (blueprint == null)
            return;

        var v3 = Input.mousePosition;
        v3.z = 10.0f;
        v3 = Camera.main.ScreenToWorldPoint(v3);
        Vector2 mPos = v3;

        if (areas != null)
        {
            for (int i = 0; i < areas.Count; i++)
            {
                if (new Rect(-0.5f * tileSize, -0.5f * tileSize, blueprint.columns * tileSize, blueprint.rows * tileSize).Contains(mPos))
                {
                    int x = Mathf.FloorToInt(mPos.y / tileSize + 0.5f);
                    int y = Mathf.FloorToInt(mPos.x / tileSize + 0.5f);
                    if (isValidTile(x, y))
                    {
                        Gizmos.color = new Color(0, 100, 150);
                        Gizmos.DrawCube(new Vector3(y * tileSize, x * tileSize, 0), new Vector3(tileSize, tileSize, tileSize));
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
                Gizmos.color = new Color(200, 0, 0, 100);
                for (int j = 0; j < nodes[i].neighbours.Count; j++)
                {
                    Gizmos.DrawLine(nodes[i].pos, nodes[i].neighbours[j].pos);
                }
            }
        }
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

    public static Vector2[] pathfind(Vector2 startPos, Vector2 targetPos)
    {
        if (instance.blueprint == null)
            return null;

        if (instance.nodes == null)
            return null;

        //find node closest to start and end positions
        NavigationNode start = getNearestNode(startPos, true);
        NavigationNode end = getNearestNode(targetPos);

        if (start == end)
            return null;

        var openList = new List<PathfindNode>();
        var closedList = new List<PathfindNode>();

        //TODO: try adding all nodes in LoS to open list
        openList.Add(new PathfindNode(start, null, 0f));

        while (openList.Count > 0)
        {
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
                    Debug.Log(node.node.pos);
                    path.Add(node.node.pos);
                    node = node.parent;
                }
                while (node.parent != null);

                // Try skipping the first node
                if(!instance.isInLoS(path[path.Count - 1], startPos))
                    path.Add(node.node.pos);

                return path.ToArray();
            }

            for(int i = 0; i < current.node.neighbours.Count; i++)
            {
                bool closed = false;
                for(int j = 0; j < closedList.Count; j++)
                {
                    if(closedList[j].node == current.node.neighbours[i])
                    {
                        closed = true;
                        break;
                    }
                }
                if(!closed)
                {
                    openList.Add(new PathfindNode(current.node.neighbours[i], current, current.d + current.node.distances[i]));
                }
            }

            openList.Remove(current);
            closedList.Add(current);
        }
        return null;
    }

    static NavigationNode getNearestNode(Vector2 pos, bool los = false)
    {
        Vector2 pos2 = pos;// + Vector2.one * 0.5f * instance.tileSize;
        NavigationNode nearest = new NavigationNode(Vector2.zero);
        float minD = float.MaxValue;
        for (int i = 0; i < instance.nodes.Count; i++)
        {
            if(los && !instance.isInLoS(instance.nodes[i].pos, pos2))
            {
                continue;
            }
            float d = (pos2 - instance.nodes[i].pos).sqrMagnitude;
            if (d < minD)
            {
                nearest = instance.nodes[i];
                minD = d;
            }
        }
        return nearest;
    }
}
