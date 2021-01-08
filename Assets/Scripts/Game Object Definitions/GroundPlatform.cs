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
        public Collider2D[] colliders;
    }

    static Encoding encoder = new ASCIIEncoding();

    public List<Tile> tiles;

    public Tile? GetTile(Vector2Int pos)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].pos == pos)
            {
                return tiles[i];
            }
        }
        return null;
    }

    public string Encode()
    {
        string s = "";
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

        ushort tileCount = getNext(data);
        for (ushort i = 0; i < tileCount; i++)
        {
            ushort x = getNext(data);
            ushort y = getNext(data);
            ushort type = getNext(data);
            byte rotation = (byte)getNext(data);

            ushort dirCount = getNext(data);
            // Skip direction data (useless in Editor)
            pointer += dirCount * 6;

            var dirs = new Dictionary<Vector2Int, byte>();

            Vector2Int pos = new Vector2Int(x, y);
            tileList.Add(
                new Tile()
                {
                    pos = pos,
                    type = (byte)type,
                    rotation = (byte)rotation,
                    directions = dirs,
                    colliders = null
                });
        }
        tiles = tileList;
    }

    public GroundPlatform(string data, GameObject[] prefabs, LandPlatformGenerator lpg)
    {
        resetPointer();

        List<Tile> tileList = new List<Tile>();

        ushort tileCount = getNext(data);
        for (ushort i = 0; i < tileCount; i++)
        {
            ushort x = getNext(data);
            ushort y = getNext(data);
            ushort type = getNext(data);
            byte rotation = (byte)getNext(data);
            ushort dirCount = getNext(data);

            var dirs = new Dictionary<Vector2Int, byte>();

            for (int j = 0; j < dirCount; j++)
            {
                dirs.Add(new Vector2Int(getNext(data), getNext(data)), (byte)getNext(data));
            }

            Vector2Int pos = new Vector2Int(x, y);

            GameObject tileObj = UnityEngine.Object.Instantiate(prefabs[type], LandPlatformGenerator.TileToWorldPos(pos), Quaternion.identity);
            tileObj.GetComponent<SpriteRenderer>().color = lpg.color;
            tileObj.transform.localEulerAngles = new Vector3(0, 0, 90 * rotation);
            tileObj.transform.SetParent(lpg.transform);

            tileList.Add(
                new Tile()
                {
                    pos = pos,
                    type = (byte)type,
                    rotation = (byte)rotation,
                    directions = dirs,
                    colliders = tileObj.GetComponentsInChildren<Collider2D>()
                });
        }
        tiles = tileList;
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

    const string encoding = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"; // 62 chars

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

    public static int GetPlatformEnds(Tile tile)
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

    public void GenerateDirections()
    {
        //Debug.Log("Generating directions for " + tiles.Count + " tiles...");
        for (int i = 0; i < tiles.Count; i++)
        {
            var openList = new List<Tile>();
            openList.Add(tiles[i]);

            int count = 0;

            while (openList.Count > 0)
            {
                if (count++ > 10000)
                {
                    Debug.LogError("Infinite loop in direction flooding");
                    return;
                }

                Tile current = openList[0];
                byte dir = 0;
                if (tiles[i].directions.ContainsKey(current.pos))
                    dir = tiles[i].directions[current.pos];

                int ends = GetPlatformEnds(current);
                //Debug.Log("Tile: " + new Vector2Int(current.pos.x, current.pos.y) + " type: " + current.type + " rot: " + current.rotation + " direction flags: " + (ends & 8) + " " + (ends & 4) + " " + (ends & 2) + " " + (ends & 1));

                for (int j = 0; j < tiles.Count; j++)
                {
                    if (i == j)
                        continue;

                    if ((ends & 1) == 1 && tiles[j].pos == current.pos + new Vector2Int(1, 0) &&
                        !tiles[i].directions.ContainsKey(tiles[j].pos))
                    {
                        // If at start, set the flooding direction
                        if (current.pos == tiles[i].pos)
                            dir = 0;
                        tiles[i].directions.Add(tiles[j].pos, dir);
                        openList.Add(tiles[j]);
                    }
                    if ((ends & 2) == 2 && tiles[j].pos == current.pos + new Vector2Int(0, -1) &&
                        !tiles[i].directions.ContainsKey(tiles[j].pos))
                    {
                        if (current.pos == tiles[i].pos)
                            dir = 1;
                        tiles[i].directions.Add(tiles[j].pos, dir);
                        openList.Add(tiles[j]);
                    }
                    if ((ends & 4) == 4 && tiles[j].pos == current.pos + new Vector2Int(-1, 0) &&
                        !tiles[i].directions.ContainsKey(tiles[j].pos))
                    {
                        if (current.pos == tiles[i].pos)
                            dir = 2;
                        tiles[i].directions.Add(tiles[j].pos, dir);
                        openList.Add(tiles[j]);
                    }
                    if ((ends & 8) == 8 && tiles[j].pos == current.pos + new Vector2Int(0, 1) &&
                        !tiles[i].directions.ContainsKey(tiles[j].pos))
                    {
                        if (current.pos == tiles[i].pos)
                            dir = 3;
                        tiles[i].directions.Add(tiles[j].pos, dir);
                        openList.Add(tiles[j]);
                    }
                }
                openList.RemoveAt(0);
            }

            if (tiles[i].directions.Count < tiles.Count - 1)
            {
                Debug.LogWarning(tiles[i].pos + " has too few directions!");
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
