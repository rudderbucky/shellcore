using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public class GroundPlatform
{
    public struct Tile
    {
        public Vector2Int pos;
        public byte type;
        public byte rotation;
        public Dictionary<Vector2Int, byte> directions; // 0 = right, 1 = up, 2 = left, 3 = down
        public Dictionary<Vector2Int, ushort> distances;
        public Collider2D[] colliders;

        public override bool Equals(object obj)
        {
            if (obj is Tile)
            {
                return ((Tile)obj).pos == pos;
            }
            else
            {
                return false;
            }
        }

        // Probably unnecessary, but VS wanted me to add it...
        public override int GetHashCode()
        {
            var hashCode = -1929533107;
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector2Int>.Default.GetHashCode(pos);
            hashCode = hashCode * -1521134295 + type.GetHashCode();
            hashCode = hashCode * -1521134295 + rotation.GetHashCode();
            return hashCode;
        }

        public static bool operator== (Tile a, Tile b)
        {
            return a.pos == b.pos;
        }

        public static bool operator !=(Tile a, Tile b)
        {
            return a.pos != b.pos;
        }
    }

    public List<Tile> tiles;

    Tile[] tileGrid;

    public Vector2Int offset { get; private set; }
    Vector2Int size;

    const string encoding = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"; // 62 chars
    const ushort versionNumber = 1;

    public Tile? GetTile(Vector2Int pos)
    {
        if (tileGrid != null)
        {
            if (pos.x < offset.x || pos.y < offset.y || pos.x - offset.x >= size.x || pos.y - offset.y >= size.y)
                return null;

            // TODO: use profiler to check if this is an effective optimization
            //Debug.Log(pos.x + ", " + pos.y + " -> " + (pos.x - offset.x + (pos.y - offset.y) * size.x) + " / " + tileGrid.Length);
            return tileGrid[pos.x - offset.x + (pos.y - offset.y) * size.x];
        }
        else
        {
            Debug.Log("Using the old GetTile");
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].pos == pos)
                {
                    return tiles[i];
                }
            }
        }
        return null;
    }

    public string Encode()
    {
        string s = "+";
        s += numToString(1); // Version number
        s += numToString((ushort)tiles.Count);
        for (int i = 0; i < tiles.Count; i++)
        {
            s += numToString((ushort)tiles[i].pos.x);
            s += numToString((ushort)tiles[i].pos.y);
            s += numToString((ushort)tiles[i].type);
            s += numToString((ushort)tiles[i].rotation);

            s += numToString((ushort)tiles[i].directions.Count);
            foreach (var pair in tiles[i].directions)
            {
                s += numToString((ushort)pair.Key.x);
                s += numToString((ushort)pair.Key.y);
                s += numToString(pair.Value);
                s += numToString(tiles[i].distances[pair.Key]);
            }
        }
        return s;
    }

    public void AddTile(Tile addition)
    {
        tiles.Add(addition);
    }

    public GroundPlatform()
    {
        tiles = new List<Tile>();
    }

    public GroundPlatform(Tile[] tiles)
    {
        this.tiles = new List<Tile>(tiles);
        GenerateDirections();
    }

    /// <summary>
    /// Loads only the tile position data. Do not use in game.
    /// </summary>
    /// <param name="data">Tile data compressed into a string</param>
    public GroundPlatform(string data)
    {
        resetPointer();

        List<Tile> tileList = new List<Tile>();

        ushort version = 0;

        if (data[0] == '+')
        {
            pointer += 1;
            version = getNext(data);
            Debug.Log("Version number: " + version);
        }

        ushort tileCount = getNext(data);
        for (ushort i = 0; i < tileCount; i++)
        {
            ushort x = getNext(data);
            ushort y = getNext(data);
            ushort type = getNext(data);
            byte rotation = (byte)getNext(data);

            ushort dirCount = getNext(data);

            // Skip direction and distance data (useless in Editor)
            pointer += dirCount * 6;
            if (version == 1)
            {
                pointer += dirCount * 2;
            }

            var dirs = new Dictionary<Vector2Int, byte>();
            var dists = new Dictionary<Vector2Int, ushort>();

            Vector2Int pos = new Vector2Int(x, y);
            tileList.Add(
                new Tile()
                {
                    pos = pos,
                    type = (byte)type,
                    rotation = (byte)rotation,
                    directions = dirs,
                    distances = dists,
                    colliders = null
                });
        }
        tiles = tileList;
    }

    public GroundPlatform(string data, GameObject[] prefabs, LandPlatformGenerator lpg)
    {
        resetPointer();

        ushort version = 0;

        if (data[0] == '+')
        {
            pointer += 1;
            version = getNext(data);
        }
        else
        {
            Debug.LogWarning("Warning: Ground pathfinding data missing due to an old level file version. Tanks won't be able to move. To update the file format, open and save the level in the world creator.");
        }

        List<Tile> tileList = new List<Tile>();

        ushort tileCount = getNext(data);

        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        for (ushort i = 0; i < tileCount; i++)
        {
            ushort x = getNext(data);
            ushort y = getNext(data);

            minX = Mathf.Min(minX, x);
            minY = Mathf.Min(minY, y);
            maxX = Mathf.Max(maxX, x);
            maxY = Mathf.Max(maxY, y);

            ushort type = getNext(data);
            byte rotation = (byte)getNext(data);
            ushort dirCount = getNext(data);

            var dirs = new Dictionary<Vector2Int, byte>();
            var dists = new Dictionary<Vector2Int, ushort>();

            for (int j = 0; j < dirCount; j++)
            {
                Vector2Int destination = new Vector2Int(getNext(data), getNext(data));
                dirs.Add(destination, (byte)getNext(data));
                if (version > 0)
                {
                    dists.Add(destination, getNext(data));
                }
            }

            Vector2Int pos = new Vector2Int(x, y);

            GameObject tileObj = null;
            if (prefabs != null)
            {
                tileObj = UnityEngine.Object.Instantiate(prefabs[type], LandPlatformGenerator.TileToWorldPos(pos), Quaternion.identity);
                tileObj.GetComponent<SpriteRenderer>().color = lpg.color;
                tileObj.transform.localEulerAngles = new Vector3(0, 0, 90 * rotation);
                tileObj.transform.SetParent(lpg.transform);
            }


            tileList.Add(
                new Tile()
                {
                    pos = pos,
                    type = (byte)type,
                    rotation = (byte)rotation,
                    directions = dirs,
                    distances = dists,
                    colliders = tileObj?.GetComponentsInChildren<Collider2D>()
                });
        }
        tiles = tileList;

        // Generate grid
        offset = new Vector2Int(minX, minY);
        int w = maxX - minX + 1;
        int h = maxY - minY + 1;
        size = new Vector2Int(w, h);

        //Debug.Log(w + ", " + h + " -> " + (w * h));

        tileGrid = new Tile[w * h];
        for (int i = 0; i < tiles.Count; i++)
        {
            Vector2Int pos = tiles[i].pos - offset;
            //Debug.Log(i + "/" + tiles.Count + ": " + pos.x + ", " + pos.y + " -> " + (pos.x + pos.y * w));
            tileGrid[pos.x + pos.y * w] = tiles[i];
        }
    }

    int pointer = 0;
    void resetPointer()
    {
        pointer = 0;
    }

    ushort getNext(string data)
    {
        ushort value = stringToNum(data.Substring(pointer, 2));
        pointer += 2;
        return value;
    }

    //public static void TEST()
    //{
    //    ushort[] data = { 0, 1, 2, 3, 50, 100, 200, 500, 1000, 3000, 3843 };

    //    for (int i = 0; i < data.Length; i++)
    //    {
    //        string s = numToString(data[i]);
    //        ushort u = stringToNum(s);
    //        Debug.Log(data[i] + " -> " + s + " -> " + u);
    //    }
    //}

    static string numToString(ushort num)
    {
        // Max 3844
        if (num > encoding.Length * encoding.Length)
        {
            Debug.LogError("Number out of bounds!");
            return "ZZ";
        }

        string s = "";
        s += encoding[num / encoding.Length];
        s += encoding[num % encoding.Length];
        return s;
    }

    static ushort stringToNum(string str)
    {
        if (str.Length != 2)
            Debug.LogError("Decoding failed!");

        ushort result = 0;
        result += (ushort)(encoding.IndexOf(str[0]) * encoding.Length);
        result += (ushort)encoding.IndexOf(str[1]);
        return result;
    }

    public static int GetPlatformOpenings(Tile tile)
    {
        int ends = 0; // 1 = right, 2 = up, 4 = left, 8 = down
        switch (tile.type)
        {
            case 0:
            case 7:
                ends = 1 | 8;
                break;
            case 1:
            case 8:
                ends = 4;
                break;
            case 2:
            case 9:
                ends = 1 | 4;
                break;
            case 3:
            case 4:
                ends = 0;
                break;
            case 5:
            case 10:
                ends = 2 | 4 | 8;
                break;
            case 6:
            case 11:
                ends = 1 | 2 | 4 | 8;
                break;
            default:
                Debug.LogWarning("Unknown tile!");
                ends = 1 | 2 | 4 | 8;
                break;
        }
        for (int j = 0; j < tile.rotation; j++)
        {
            ends *= 2;
            if ((ends & 16) == 16)
            {
                ends -= 16;
                ends += 1;
            }
        }

        return ends;
    }

    struct Node
    {
        public Node(Tile tile, ushort dist)
        {
            this.tile = tile;
            this.dist = dist;
        }
        public Tile tile;
        public ushort dist;
    }

    public void GenerateDirections()
    {
        //Debug.Log("Generating directions for " + tiles.Count + " tiles...");

        // Initialize direction vectors
        Vector2Int[] vectors = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1)
        };

        // Generate grid
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        for (ushort i = 0; i < tiles.Count; i++)
        {
            minX = Mathf.Min(minX, tiles[i].pos.x);
            minY = Mathf.Min(minY, tiles[i].pos.y);
            maxX = Mathf.Max(maxX, tiles[i].pos.x);
            maxY = Mathf.Max(maxY, tiles[i].pos.y);
        }

        offset = new Vector2Int(minX, minY);
        int w = maxX - minX + 1;
        int h = maxY - minY + 1;
        size = new Vector2Int(w, h);

        tileGrid = new Tile[w * h];
        for (int i = 0; i < tiles.Count; i++)
        {
            Vector2Int pos = tiles[i].pos - offset;
            tileGrid[pos.x + pos.y * w] = tiles[i];
        }

        // Calculate directions
        for (int i = 0; i < tiles.Count; i++)
        {
            var openList = new List<Node>();
            openList.Add(new Node(tiles[i], 0));

            int count = 0;

            while (openList.Count > 0)
            {
                if (count++ > 10000)
                {
                    Debug.LogError("Infinite loop in direction flooding");
                    return;
                }

                Node current = openList[0];
                ushort dist = current.dist;
                dist++;

                byte dir = 0;
                if (tiles[i].directions.ContainsKey(current.tile.pos))
                    dir = tiles[i].directions[current.tile.pos];

                int openings = GetPlatformOpenings(current.tile);
                //Debug.Log("Tile: " + new Vector2Int(current.pos.x, current.pos.y) + " type: " + current.type + " rot: " + current.rotation + " direction flags: " + (ends & 8) + " " + (ends & 4) + " " + (ends & 2) + " " + (ends & 1));

                for (int j = 0; j < 4; j++)
                {
                    if ((openings & (1 << j)) == (1 << j))
                    {
                        var tile = GetTile(current.tile.pos + vectors[j]);
                        if (tile == tiles[i] || !tile.HasValue)
                            continue;

                        if (!tiles[i].directions.ContainsKey(tile.Value.pos))
                        {
                            if (current.tile == tiles[i])
                                dir = (byte)j;
                            tiles[i].directions.Add(tile.Value.pos, dir);
                            tiles[i].distances.Add(tile.Value.pos, dist);
                            openList.Add(new Node(tile.Value, dist));
                        }
                    }
                }
                openList.RemoveAt(0);
            }

            if (tiles[i].directions.Count < tiles.Count - 1)
            {
                Debug.LogWarning(tiles[i].pos + " has too few directions (" + tiles[i].directions.Count + ")!");
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].colliders.Length > 0)
                UnityEngine.Object.Destroy(tiles[i].colliders[0].gameObject);
        }
    }
}
