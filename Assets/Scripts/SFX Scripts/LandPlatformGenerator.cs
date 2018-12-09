using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandPlatformGenerator : MonoBehaviour {

    public GameObject[] prefabs;
    public LandPlatform blueprint;
    private static List<GameObject> tiles;
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

	void Start () {
        tiles = new List<GameObject>();
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
                    }
                }
                ySpacing += spacing;
            }
        }
	}
}
