using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandPlatformGenerator : MonoBehaviour {

    public GameObject[] prefabs;
    public LandPlatform blueprint;
	// Use this for initialization
	void Start () {
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
                        GameObject tile = Instantiate(prefabs[row[j].type], new Vector3(j * spacing, ySpacing, 1), Quaternion.identity);
                        tile.transform.SetParent(transform);
                    }
                }
                ySpacing += spacing;
            }
        }
	}
}
