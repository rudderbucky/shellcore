using System.Collections.Generic;
using UnityEngine;

public class GroundPlatform
{
    public struct Tile
    {
        public Vector2Int pos;
        public byte type;
        public byte rotation;
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
            return hashCode;
        }

        public static bool operator ==(Tile a, Tile b)
        {
            return a.pos == b.pos;
        }

        public static bool operator !=(Tile a, Tile b)
        {
            return a.pos != b.pos;
        }
    }

    public List<Tile> tiles;

    Tile?[] tileGrid;
    Dictionary<Entity, Tile> closestTiles = new Dictionary<Entity, Tile>();

    public Vector2Int offset { get; private set; }
    Vector2Int size;

    const string encoding = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"; // 62 chars
    const ushort versionNumber = 2;

    public Tile? GetTile(Vector2Int pos)
    {
        if (tileGrid != null)
        {
            if (pos.x < offset.x || pos.y < offset.y || pos.x - offset.x >= size.x || pos.y - offset.y >= size.y)
            {
                return null;
            }

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

    public void SetClosestTile(Entity ent, Tile t)
    {
        closestTiles[ent] = t;
    }

    public void RemoveClosestTile(Entity ent)
    {
        if (closestTiles.ContainsKey(ent))
            closestTiles.Remove(ent);
    }

    public Tile GetClosestTile(Entity ent)
    {
        if (closestTiles.ContainsKey(ent))
        {
            return closestTiles[ent];
        }
        else
        {
            Tile tile = LandPlatformGenerator.Instance.GetNearestTile(this, ent.transform.position).Value;
            SetClosestTile(ent, tile);
            return tile;
        }
    }

    public string Encode()
    {
        string s = "+";
        s += numToString(versionNumber);
        s += numToString((ushort)tiles.Count);
        for (int i = 0; i < tiles.Count; i++)
        {
            s += numToString((ushort)tiles[i].pos.x);
            s += numToString((ushort)tiles[i].pos.y);
            s += numToString((ushort)tiles[i].type);
            s += numToString((ushort)tiles[i].rotation);
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
        // Generate grid
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        for (ushort i = 0; i < tiles.Length; i++)
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

        tileGrid = new Tile?[w * h];
        for (int i = 0; i < tiles.Length; i++)
        {
            Vector2Int pos = tiles[i].pos - offset;
            tileGrid[pos.x + pos.y * w] = tiles[i];
        }

        //GenerateDirections();
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

            if (version < 2)
            {
                ushort dirCount = getNext(data);
                
                pointer += dirCount * 6; // Directions
                if (version == 1)
                    pointer += dirCount * 2; // Distances
            }

            Vector2Int pos = new Vector2Int(x, y);
            tileList.Add(
                new Tile()
                {
                    pos = pos,
                    type = (byte)type,
                    rotation = rotation,
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
        if (version < versionNumber)
        {
            Debug.LogWarning($"Warning: Old ground platform data! (version: {version})");
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

            if (version < 2)
            {
                ushort dirCount = getNext(data);

                pointer += dirCount * 6;
                if (version == 1)
                    pointer += dirCount * 2;
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
                    rotation = rotation,
                    colliders = tileObj?.GetComponentsInChildren<Collider2D>()
                });
        }

        tiles = tileList;

        // Generate grid
        offset = new Vector2Int(minX, minY);
        int w = maxX - minX + 1;
        int h = maxY - minY + 1;
        size = new Vector2Int(w, h);

        //Debug.Log("Tile grid: " + w + ", " + h + " -> " + (w * h));

        tileGrid = new Tile?[w * h];
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
        {
            Debug.LogError("Decoding failed!");
        }

        ushort result = 0;
        result += (ushort)(encoding.IndexOf(str[0]) * encoding.Length);
        result += (ushort)encoding.IndexOf(str[1]);
        return result;
    }

    public static short GetPlatformOpenings(Tile tile)
    {
        short ends = 0; // 1 = right, 2 = up, 4 = left, 8 = down
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

    public void Clear()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].colliders.Length > 0)
            {
                UnityEngine.Object.Destroy(tiles[i].colliders[0].gameObject);
            }
        }
    }
}
