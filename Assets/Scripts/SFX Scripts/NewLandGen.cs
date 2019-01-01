using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewLandGen : MonoBehaviour {


    // TODO: create nodes, create paths, create platform generation based on blueprint
    public LandPlatform blueprint;
    private List<GameObject> tiles;
    private List<Rect> areas;
    private List<NavigationNode> nodes;
    private float tileSize;

    public bool CheckOnGround(Vector3 position)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].GetComponent<SpriteRenderer>().bounds.Contains(position))
            {
                return true;
            }
        }
        return false;
    }

    public void BuildTiles() {
        if (!blueprint || blueprint.prefabs.Length <= 0)
            return;
        
        if(tiles != null) Unload();

        tileSize = blueprint.prefabs[0].GetComponent<SpriteRenderer>().bounds.size.x;

        var rows = blueprint.rows;
        var cols = blueprint.columns;
        var offset = new Vector2 
        {
            x = -tileSize * (cols-1)/2,
            y = +tileSize * (rows-1)/2
        };

        tiles = new List<GameObject>();

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
                    var tile = Instantiate(blueprint.prefabs[blueprint.tilemap[i]], pos, Quaternion.identity);
                    tiles.Add(tile);
                    tile.transform.parent = transform;
                    break;
            }
        }
        BuildNodes();
    }

    void Start() {
        BuildTiles();
    }
    public void Unload()
    {
        for(int i = 0; i < tiles.Count; i++)
        {
            Destroy(tiles[i]);
        }
        tiles.Clear();
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
    void BuildNodes()
    {
        Debug.Log("Building nodes...");

        float dToCenter = tileSize / 2f; // node distance to center on one axis
        nodes = new List<NavigationNode>();
        var offset = new Vector2 
        {
            x = -tileSize * (blueprint.columns-1)/2,
            y = +tileSize * (blueprint.rows-1)/2
        };
        for (int i = 0; i < blueprint.rows; i++)
        {
            for (int j = 0; j < blueprint.columns; j++)
            {
                if (blueprint.tilemap[i * blueprint.columns + j] > -1)
                {
                    //create nodes
                    if (!isValidTile(i, j))
                        continue;

                    bool right = isValidTile(i, j + 1);
                    bool up = isValidTile(i - 1, j);
                    bool left = isValidTile(i, j - 1);
                    bool down = isValidTile(i + 1, j);

                    //Debug.Log(right + "" + up + left + down + (i * blueprint.columns + j));
                    if ((!right && !up) || (right && up && !isValidTile(i + 1, j + 1))) //check if the tile is a corner
                        nodes.Add(new NavigationNode(new Vector2(j * tileSize + dToCenter, -i * tileSize + dToCenter) + offset));
                    if ((!left && !up) || (left && up && !isValidTile(i - 1, j + 1)))
                        nodes.Add(new NavigationNode(new Vector2(j * tileSize - dToCenter, -i * tileSize + dToCenter) + offset));
                    if ((!left && !down) || (left && down && !isValidTile(i - 1, j - 1)))
                        nodes.Add(new NavigationNode(new Vector2(j * tileSize - dToCenter, -i * tileSize - dToCenter) + offset));
                    if ((!right && !down) || (right && down && !isValidTile(i + 1, j - 1)))
                        nodes.Add(new NavigationNode(new Vector2(j * tileSize + dToCenter, -i * tileSize - dToCenter) + offset));
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
        bool limitCheck = x < blueprint.rows && y < blueprint.columns && x >= 0 && y >= 0;
        bool selfIsTile = limitCheck && blueprint.tilemap[x * blueprint.columns + y] > -1;
        bool selfIsWithinLengthLimit = selfIsTile && blueprint.tilemap[x * blueprint.columns + y] < blueprint.prefabs.Length;
        bool final = selfIsWithinLengthLimit && blueprint.prefabs[blueprint.tilemap[x * blueprint.columns + y]] != null;
        return final;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = new Color(0, 100, 150);
        if(nodes == null) return;
        foreach(NavigationNode node in nodes) {
            Gizmos.DrawCube(node.pos, new Vector3(0.3F, 0.3F));
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
}