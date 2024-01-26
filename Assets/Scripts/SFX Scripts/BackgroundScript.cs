using System.Collections;
using UnityEngine;

public enum BackgroundTileSkin
{
    Squares,
    Clouds,
    Web,
    Tetrominoes
}

public class BackgroundScript : MonoBehaviour
{
    public GameObject[] tile; // array of tile images, prefabbed into sprites
    public static BackgroundTileSkin currentSkin = BackgroundTileSkin.Squares;
    private Vector2 tileStartPos; // the start position of the background (lower left tile)
    Vector2 tileSpacing; // the size of the tile (contains length and height as x and y)
    private Vector3 instancedPos; // vector that stores the position of a generated tile
    public int gridWidth; // grid width
    public int gridHeight; // grid height
    public static int gridDepth = 15; // grid depth
    public Transform mcamera; // mcamera to follow
    private Vector3 displacement; // displacement between tile and mcamera
    private GameObject[] ingameTiles; // array of generated tiles
    public static bool active = true;
    public static Color bgCol;
    public static BackgroundScript instance;

    public void Awake()
    {
        instance = this;
        active = PlayerPrefs.GetString("BackgroundScript_active", "True") == "True";
    }

    public static void SetActive(bool act)
    {
        active = act;
        if (instance && SystemLoader.AllLoaded)
        {
            instance.Restart();
        }
    }

    /// <summary>
    /// Updates the tiles' positions
    /// </summary>
    /// <param name="tile">the array of tiles</param>
    private void TileUpdate(GameObject[] tile)
    {
        if (tile != null)
        {
            for (int i = 0; i < tile.Length; i++) // iterate through every tile
            {
                TileWrapper(tile[i], 0); // update each tile for both dimensions
                TileWrapper(tile[i], 1);
                if (currentSkin != BackgroundTileSkin.Web)
                {
                    var x = tile[i].GetComponent<SpriteRenderer>().color;
                    x.a = Mathf.Sin(Time.time / 2 + (i % 2 == 0 ? Mathf.PI : 0)) / 2 + 0.5F;
                    tile[i].GetComponent<SpriteRenderer>().color = x;
                    if (x.a < 0.05F && !refreshed[i]) {
                        refreshed[i] = true;
                        SetSprite(i);
                    }
                    if (x.a > 0.95F) {
                        refreshed[i] = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Helper method for TileUpdate, wraps the tiles around the screen if they move too far
    /// </summary>
    /// <param name="tile">the tile to wrap</param>
    /// <param name="dimension">the dimension (0 is x, 1 is y)</param>
    private void TileWrapper(GameObject tile, int dimension)
    {
        if (tile)
        {
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

    Rect pixelRect;

    // Use this for initialization
    void Build()
    {
        if (active)
        {
            pixelRect = Camera.main.pixelRect;
            if (transform.Find("Tile Holder"))
            {
                Destroy(transform.Find("Tile Holder").gameObject);
            }

            mcamera = Camera.main.transform;
            tileSpacing = tile[4 * (int)currentSkin].GetComponent<Renderer>().bounds.size;
            GameObject parent = new GameObject("Tile Holder");
            parent.transform.SetParent(transform, true);
            // grab tile spacing (this should be constant between the tile sprites given)
            Vector3 dimensions = Camera.main.ScreenToWorldPoint(
                new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, gridDepth + CameraScript.GetMaxZoomLevel()));
            // grab camera dimensions
            gridWidth = 1 + (int)Mathf.Ceil((dimensions.x - mcamera.position.x) * 2 / tileSpacing.x); // calculate height and width using camera dimensions
            gridHeight = 1 + (int)Mathf.Ceil((dimensions.y - mcamera.position.y) * 2 / tileSpacing.y);
            ingameTiles = new GameObject[gridWidth * gridHeight * 2]; // create an array of tile references
            tileStartPos = new Vector2 // get the tile start position (this project needs the tiles to center at 0,0)
            {
                x = mcamera.position.x - tileSpacing.x * (gridWidth - 1) / 2,
                y = mcamera.position.y - tileSpacing.y * (gridHeight - 1) / 2
            };
            int count = 0; // used for array assignment (to keep a 1d count in the 2d loop)
            for (int i = 0; i < gridHeight; i++)
            {
                for (int j = 0; j < gridWidth; j++)
                {
                    for (int k = 0; k < 2; k++) 
                    {
                        instancedPos = new Vector3(tileStartPos.x + j * tileSpacing.x, tileStartPos.y + i * tileSpacing.y, gridDepth);
                        // the position of the tile
                        GameObject go = Instantiate(tile[0], instancedPos, Quaternion.identity);
                        go.transform.SetParent(parent.transform, true);
                        
                        
                        // create the tile, no rotation desired

                        ingameTiles[count] = go; // assign to array
                        SetSprite(count);
                        count++; // increment count
                        // I don't want the tiles to be a child of the object using this script 
                        // as I want the tiles to warp like the particles instead of constantly follow
                    }
                }
            }
            refreshed = new bool[count];
        }
    }

    private bool[] refreshed;

    private void SetSprite(int count) {
        int randomTile = Random.Range(4 * (int)currentSkin, 4 * (int)currentSkin + 4); // grabs a random tile from the array of sprites
        ingameTiles[count].GetComponent<SpriteRenderer>().sprite = tile[randomTile].GetComponent<SpriteRenderer>().sprite;

        GameObject go = ingameTiles[count];
        if (currentSkin == BackgroundTileSkin.Web || currentSkin == BackgroundTileSkin.Tetrominoes) 
        {
            go.transform.localScale = new Vector3(Random.Range(0, 2) > 0.5F ? 1 : -1, Random.Range(0, 2) > 0.5F ? 1 : -1,1);
        }
        else if (currentSkin == BackgroundTileSkin.Squares)
        {
            go.transform.localScale = new Vector3(Random.Range(0, 2) > 0.5F ? 1 : -1, 1, 1);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!SystemLoader.AllLoaded) return;
        if (active)
        {
            if (Camera.main.pixelRect != pixelRect)
            {
                Restart();
            }

            TileUpdate(ingameTiles); // tile update called on tile array
        }
    }

    public void Initialize()
    {
        Build();
    }

    public void Restart()
    {
        if (GameObject.Find("Tile Holder"))
        {
            Destroy(GameObject.Find("Tile Holder"));
        }

        if (active)
        {
            Build();
            setColor(bgCol);
        }
    }

    Color lastColor; // used like bgCol, just without the static attribute

    public void setColor(Color color, bool force = false)
    {
        Camera.main.backgroundColor = color / 2F;
        if (ingameTiles == null)
        {
            bgCol = lastColor = color;
            return;
        }

        if (lastColor == Color.clear || force)
        {
            bgCol = lastColor = color;
            foreach (GameObject tile in ingameTiles)
            {
                if (!tile || !tile.GetComponent<SpriteRenderer>()) continue;
                tile.GetComponent<SpriteRenderer>().color = color;
            }

            return;
        }

        if (active)
        {
            for (int i = 0; i < ingameTiles.Length; i++)
            {
                if (!ingameTiles[i] || !ingameTiles[i].GetComponent<SpriteRenderer>()) continue;
                var renderer = ingameTiles[i].GetComponent<SpriteRenderer>();
                renderer.color = lastColor;
                StartCoroutine(FadeColor(color, renderer));
                //ingameTiles[i].GetComponent<SpriteRenderer>().color = color;
            }
        }

        bgCol = lastColor = color;
        // this entire method happens in 1 frame so these are updated even while the renderers are lerping
    }

    private IEnumerator FadeColor(Color newColor, SpriteRenderer renderer)
    {
        float beginLerp = 0;
        while (renderer && renderer.color != newColor)
        {
            renderer.color = Color.Lerp(renderer.color, newColor, beginLerp);
            beginLerp += 0.0125F;
            if (beginLerp > 1)
            {
                beginLerp = 1;
                renderer.color = Color.Lerp(renderer.color, newColor, beginLerp);
                break;
            }

            yield return null;
        }
    }
}
