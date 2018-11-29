using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScript : MonoBehaviour {

    public GameObject[] tile; // array of tile images, prefabbed into sprites
    private Vector2 tileStartPos; // the start position of the background (lower left tile)
    Vector2 tileSpacing; // the size of the tile (contains length and height as x and y)
    private Vector3 instancedPos; // vector that stores the position of a generated tile
    public int gridWidth; // grid width
    public int gridHeight; // grid height
    public int gridDepth; // grid depth
    public Transform core; // core to follow
    private Vector3 displacement; // displacement between tile and core
    private GameObject[] ingameTiles; // array of generated tiles

    /// <summary>
    /// Updates the tiles' positions
    /// </summary>
    /// <param name="tile">the array of tiles</param>
    private void tileUpdate(GameObject[] tile)
    {
        for (int i = 0; i < tile.Length; i++)
        {
            tileWrapper(tile[i], 0); // update each tile for both dimensions
            tileWrapper(tile[i], 1); 
        }
    }

    /// <summary>
    /// Helper method for tileUpdate, wraps the tiles around the screen if they move too far
    /// </summary>
    /// <param name="tile">the tile to wrap</param>
    /// <param name="dimension">the dimension (0 is x, 1 is y)</param>
    private void tileWrapper(GameObject tile, int dimension)
    {
        float limit;
        switch (dimension)
        {
            case 0:
                limit = gridWidth * tileSpacing.x / 2; // x axis
                break;
            case 1:
                limit = gridHeight * tileSpacing.y / 2; // y axis
                break;
            default: // not supposed to happen lol, too lazy to learn C# exception handling
                limit = 0;
                break;
        }
        if (Mathf.Abs(tile.transform.position[dimension] - core.position[dimension]) > limit) // this means it is at an axis edge
        {
            if (tile.transform.position[dimension] - core.position[dimension] > 0) // right edge
            {
                limit = -limit; // make it so that you subtract the limit later on
            }
            // if limit remains positive left edge
            displacement = tile.transform.position; // grab the tile position
            displacement[dimension] = displacement[dimension] + 2 * limit; // update the x position to be at the other edge
            tile.transform.position = displacement; // update the tile position, similar process for the other checks as well
        }
    }

    // Use this for initialization
    void Start()
    {
        //ingameTiles = new GameObject[gridWidth * gridHeight]; // grab an array of tile references
        tileSpacing = tile[0].GetComponent<Renderer>().bounds.size; // grab tile spacing (this should be constant between the tile sprites given)
        Vector2 dimensions = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, gridDepth - Camera.main.transform.position.z));
        gridWidth = 1 + (int)Mathf.Ceil(dimensions.x * 2/ tileSpacing.x);
        gridHeight = 1 + (int)Mathf.Ceil(dimensions.y * 2/ tileSpacing.y);
        ingameTiles = new GameObject[gridWidth * gridHeight];
        tileStartPos = new Vector2 // get the tile start position (this project needs the tiles to center at 0,0)
        {
            x = -tileSpacing.x * (gridWidth-1)/2,
            y = -tileSpacing.y * (gridHeight-1)/2
        };
        int count = 0; // used for array assignment (to keep a 1d count in the 2d loop)
        for (int i = 0; i < gridHeight; i++) {
            for (int j = 0; j < gridWidth; j++) {
                int randomTile = Random.Range(0, tile.Length); // grabs a random tile from the array of sprites
                instancedPos = new Vector3(tileStartPos.x + j * tileSpacing.x, tileStartPos.y + i * tileSpacing.y, gridDepth);
                // the position of the tile
                GameObject go = Instantiate(tile[randomTile], instancedPos, Quaternion.identity) as GameObject;
                // create the tile, no rotation desired
                go.GetComponent<SpriteRenderer>().color = new Color(0.039F, 0.188F, 0.184F);
                // change the color (will be changing this line later)
                ingameTiles[count] = go; // assign to array
                count++; // increment count
                // I don't want the tiles to be a child of the object using this script 
                // as I want the tiles to warp like the particles instead of constantly follow
            }
        }
    }
        // Update is called once per frame
        void LateUpdate()
        {
        tileUpdate(ingameTiles);
        }
    }