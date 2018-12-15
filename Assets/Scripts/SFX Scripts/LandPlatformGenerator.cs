using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandPlatformGenerator : MonoBehaviour {

    public GameObject[] prefabs;
    public LandPlatform blueprint;
    private static List<GameObject> tiles;
    private static List<int> directions;
    // Use this for initialization

    public static bool CheckOnGround(Vector3 position)
    {
        for(int i = 0; i < tiles.Count; i++)
        {
            if(tiles[i].GetComponent<SpriteRenderer>().bounds.Contains(position))
            {
                return true;
            }
        }
        return false;
    }

    public static Vector3 getDirection(Vector3 position)
    {
        int tileIndex = -1;
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].GetComponent<SpriteRenderer>().bounds.Contains(position))
            {
                tileIndex = i;
                break;
            }
        }
        if (tileIndex == -1)
            return Vector3.zero;

        Vector3 center = tiles[tileIndex].transform.position;
        float offset = 0.1f;
        Vector3 direction = Vector3.zero;

        switch (directions[tileIndex])
        {
            case 0:
                if (position.y < center.y - offset)
                    direction += Vector3.up;
                else if (position.y > center.y + offset)
                    direction -= Vector3.up;
                direction += Vector3.right;
                break;
            case 1:
                if (position.x < center.x - offset)
                    direction += Vector3.right;
                else if (position.x > center.x + offset)
                    direction -= Vector3.right;
                direction += Vector3.up;
                break;
            case 2:
                if (position.y < center.y - offset)
                    direction += Vector3.up;
                else if (position.y > center.y + offset)
                    direction -= Vector3.up;
                direction += -Vector3.right;
                break;
            case 3:
                if (position.x < center.x - offset)
                    direction += Vector3.right;
                else if (position.x > center.x + offset)
                    direction -= Vector3.right;
                direction += -Vector3.up;
                break;
        }
        return direction.normalized;
    }

	void Start () {
        tiles = new List<GameObject>();
        directions = new List<int>();
        if (blueprint)
        {
            float spacing = prefabs[1].GetComponent<SpriteRenderer>().sprite.bounds.size.x;
            float ySpacing = 0;
            for (int i = 0; i < blueprint.platformRows.Length; i++)
            {
                LandPlatform.Platform[] row = blueprint.platformRows[i].platformRow;
                for (int j = 0; j < row.Length; j++)
                {
                    if (row[j].type != 0)
                    {
                        GameObject tile = Instantiate(prefabs[row[j].type], new Vector3(j * spacing, ySpacing, 0), Quaternion.identity);
                        tiles.Add(tile);
                        tile.transform.SetParent(transform);
                        directions.Add(row[j].direction);
                    }
                }
                ySpacing += spacing;
            }
        }
	}
}
