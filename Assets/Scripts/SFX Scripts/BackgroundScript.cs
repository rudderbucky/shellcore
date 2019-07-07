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
    public Transform mcamera; // mcamera to follow
    private Vector3 displacement; // displacement between tile and mcamera
    private GameObject[] ingameTiles; // array of generated tiles
    public static bool active = true;
    public static Color bgCol;
    
    public void SetActive(bool act) {
        active = act;
    }

    /// <summary>
    /// Updates the tiles' positions
    /// </summary>
    /// <param name="tile">the array of tiles</param>
    private void TileUpdate(GameObject[] tile)
    {
        if(tile != null)
        for (int i = 0; i < tile.Length; i++) // iterate through every tile
        {
            TileWrapper(tile[i], 0); // update each tile for both dimensions
            TileWrapper(tile[i], 1); 
        }
    }

    /// <summary>
    /// Helper method for TileUpdate, wraps the tiles around the screen if they move too far
    /// </summary>
    /// <param name="tile">the tile to wrap</param>
    /// <param name="dimension">the dimension (0 is x, 1 is y)</param>
    private void TileWrapper(GameObject tile, int dimension)
    {
        if(tile) {
            float limit = dimension == 0 ? gridWidth * tileSpacing.x / 2 : gridHeight * tileSpacing.y / 2; 
            // the limit before the tile should wrap

            if (Mathf.Abs(tile.transform.position[dimension] - mcamera.position[dimension]) > limit) // this means it is at an axis edge
            {
                limit = tile.transform.position[dimension] - mcamera.position[dimension] > 0 ? -limit : limit; // right edge
                // (this may be slightly inefficient but I don't care it's cool)

                // if limit remains positive left edge
                displacement = tile.transform.position; // grab the tile position
                displacement[dimension] = displacement[dimension] + 2 * limit; // update the x position to be at the other edge
                tile.transform.position = displacement; // update the tile position, similar process for the other checks as well
            }
        }
    }

    // Use this for initialization
    void Build()
    {
        if(active) {
            if(transform.Find("Tile Holder")) Destroy(transform.Find("Tile Holder").gameObject);
            mcamera = Camera.main.transform;
            tileSpacing = tile[0].GetComponent<Renderer>().bounds.size; 
            GameObject parent = new GameObject("Tile Holder");
            parent.transform.SetParent(transform, true);
            // grab tile spacing (this should be constant between the tile sprites given)
            Vector3 dimensions = Camera.main.ScreenToWorldPoint(
                new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, gridDepth - Camera.main.transform.position.z));
            // grab camera dimensions
            gridWidth = 1 + (int)Mathf.Ceil((dimensions.x - mcamera.position.x) * 2/ tileSpacing.x); // calculate height and width using camera dimensions
            gridHeight = 1 + (int)Mathf.Ceil((dimensions.y - mcamera.position.y) * 2 / tileSpacing.y);
            ingameTiles = new GameObject[gridWidth * gridHeight]; // create an array of tile references
            tileStartPos = new Vector2 // get the tile start position (this project needs the tiles to center at 0,0)
            {
                x = mcamera.position.x -tileSpacing.x * (gridWidth-1)/2,
                y = mcamera.position.y -tileSpacing.y * (gridHeight-1)/2
            };
            int count = 0; // used for array assignment (to keep a 1d count in the 2d loop)
            for (int i = 0; i < gridHeight; i++) {
                for (int j = 0; j < gridWidth; j++) {
                    int randomTile = Random.Range(0, tile.Length); // grabs a random tile from the array of sprites
                    instancedPos = new Vector3(tileStartPos.x + j * tileSpacing.x, tileStartPos.y + i * tileSpacing.y, gridDepth);
                    // the position of the tile
                    GameObject go = Instantiate(tile[randomTile], instancedPos, Quaternion.identity) as GameObject;
                    go.transform.SetParent(parent.transform, true);
                    // create the tile, no rotation desired
                    
                    ingameTiles[count] = go; // assign to array
                    count++; // increment count
                    // I don't want the tiles to be a child of the object using this script 
                    // as I want the tiles to warp like the particles instead of constantly follow
                }
            }
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if(active)
            TileUpdate(ingameTiles); // tile update called on tile array
    }

    void Awake() {
        Build();
    }
    public void Restart() {
        if(active) {
            if(GameObject.Find("Tile Holder")) Destroy(GameObject.Find("Tile Holder"));
            Build();
            setColor(bgCol);
        }
    }
    Color lastColor; // used like bgCol, just without the static attribute
    public void setColor(Color color)
    {
        Camera.main.backgroundColor = color;
        if(ingameTiles == null) {
            bgCol = lastColor = color;
            return;
        }
        if(lastColor == Color.clear) {
            lastColor = color;
            foreach(GameObject tile in ingameTiles) {
                tile.GetComponent<SpriteRenderer>().color = color;
            }
            return;
        }

        if(active) {
            for(int i = 0; i < ingameTiles.Length; i++)
            {
                var renderer = ingameTiles[i].GetComponent<SpriteRenderer>();
                renderer.color = lastColor;
                StartCoroutine(FadeColor(color, renderer));
                //ingameTiles[i].GetComponent<SpriteRenderer>().color = color;
            }
        }
        bgCol = lastColor = color; 
        // this entire method happens in 1 frame so these are updated even while the renderers are lerping
    }

    private IEnumerator FadeColor(Color newColor, SpriteRenderer renderer) {
        float beginLerp = 0;
        while(renderer.color != newColor) {
            renderer.color = Color.Lerp(renderer.color, newColor, beginLerp);
            beginLerp += 0.0125F;
            if(beginLerp > 1) beginLerp = 1;
            yield return null;
        }
    }
}