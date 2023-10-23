using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using static TaskManager;

public class MapMakerScript : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image sectorPrefab;
    public SectorManager manager;
    public RectTransform greenBox;
    PlayerCore playerCore;
    public Transform player;
    public Font shellcoreFont;
    public Sprite gridSprite;
    RectTransform indicator;
    int zoomoutFactor = 4;
    int sectorCount = 0;
    int minX = int.MaxValue;
    int maxY = int.MinValue;

    public GameObject mapArrowPrefab;

    // Use this for initialization
    static MapMakerScript instance;
    public GameObject tooltipPrefab;
    private RectTransform tooltipTransform;
    private List<(Image, Vector3)> sectorImages = new List<(Image, Vector3)>();
    private Dictionary<Image, (string, Sector.SectorType)> sectorInfo = new Dictionary<Image, (string, Sector.SectorType)>();
    private List<Image> partOriginMarkerImages = new List<Image>();

    void OnEnable()
    {
        instance = this;
        if (player)
        {
            playerCore = player.GetComponent<PlayerCore>();
        }

        if (manager)
        {
            Destroy();
            Draw(manager.sectors);
        }
    }

    // I have barely any idea why these constants are their values, but it works with these
    float const1 = 150 / 4;
    int const2 = 50;
    int const4 = 50;
    const int distancePerTextMarker = 200;

    float gridSizeX;
    float gridSizeY;
    Image gridImg;

    Dictionary<TaskManager.ObjectiveLocation, RectTransform> arrows = new Dictionary<TaskManager.ObjectiveLocation, RectTransform>();

    public static void Redraw(List<Sector> sectors)
    {
        if (instance)
        {
            instance.redraw(sectors);
        }
    }

    public void redraw(List<Sector> sectors, int zoomoutFactor = 4, int dimension = 0, bool displayStations = false)
    {
        Destroy();
        Draw(sectors, zoomoutFactor, dimension, true, displayStations);
    }

    private void InitializeVariables(int zoomoutFactor, bool resetPosition)
    {
        this.zoomoutFactor = zoomoutFactor;
        gridSizeX = canvas.sizeDelta.x;
        gridSizeY = canvas.sizeDelta.y;
        // this sets up the tiled grid
        gridImg = new GameObject().AddComponent<Image>();
        gridImg.gameObject.name = "Grid";
        gridImg.transform.SetParent(transform, false);
        gridImg.rectTransform.localScale = Vector3.one;
        gridImg.rectTransform.pivot = new Vector2(0, 1);
        if (resetPosition)
        {
            gridImg.rectTransform.anchoredPosition = Vector2.zero;
        }

        mouseInBounds = false;
        if (tooltipTransform)
        {
            Destroy(tooltipTransform.gameObject);
        }


        gridImg.sprite = gridSprite;
        gridImg.type = Image.Type.Tiled;
        gridImg.color = new Color32(100, 100, 100, 255);
        minX = int.MaxValue;
        maxY = int.MinValue;
        if (resetPosition)
        {
            canvas.anchoredPosition = Vector2.zero;
        }
    }

    private void CreateLandPlatforms(Sector sector, LandPlatformGenerator lpg, Image sect)
    {
        // set up land platforms
        GameObject prefab = ResourceManager.GetAsset<GameObject>(LandPlatformGenerator.prefabNames[0]);
        float tileSize = prefab.GetComponent<SpriteRenderer>().bounds.size.x;
        if (sector.platforms == null && sector.platformData.Length > 0)
        {

            var cols = sector.bounds.h / (int)tileSize;
            var rows = sector.bounds.w / (int)tileSize;

            Vector2 center = new Vector2(sector.bounds.x + sector.bounds.w / 2, sector.bounds.y - sector.bounds.h / 2);

            Vector2 offset = new Vector2
            {
                x = center.x - tileSize * (rows - 1) / 2F,
                y = center.y + tileSize * (cols - 1) / 2F
            };

            sector.platforms = new GroundPlatform[sector.platformData.Length];
            for (int i = 0; i < sector.platformData.Length; i++)
            {
                var plat = new GroundPlatform(sector.platformData[i], null, lpg);
                sector.platforms[i] = plat;
            }
        }

        if (sector.platforms != null)
        {
            var platforms = sector.platforms;
            foreach (var platform in platforms)
            {
                float finalTileSize = tileSize / zoomoutFactor;
                List<Vector2> vertices = new List<Vector2>();
                for (int i = 0; i < platform.tiles.Count; i++)
                {
                    var tile = platform.tiles[i];

                    var pos = new Vector2(tile.pos.x, -tile.pos.y - 1f) * finalTileSize;

                    vertices.Add(new Vector3(pos.x + finalTileSize, pos.y + finalTileSize));
                    vertices.Add(new Vector3(pos.x, pos.y + finalTileSize));
                    vertices.Add(new Vector3(pos.x, pos.y));
                    vertices.Add(new Vector3(pos.x + finalTileSize, pos.y));
                }

                if (vertices.Count > 0)
                {
                    var obj = new GameObject("LandPlatformMesh");
                    obj.transform.SetParent(transform, false);
                    var rt = obj.AddComponent<RectTransform>();
                    rt.pivot = new Vector2(0f, 1f);
                    rt.anchoredPosition = sect.rectTransform.anchoredPosition;
                    rt.sizeDelta = sect.rectTransform.sizeDelta;
                    var renderer = obj.AddComponent<UILandPlatformRenderer>();
                    renderer.vertices = vertices.ToArray();
                    renderer.color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
        }
    }

    private void DefineGridSize(List<Sector> sectors, int dimension)
    {
        // this sets up the top-left part of the map
        foreach (var sector in sectors)
        {
            if ((playerCore && sector.dimension != playerCore.Dimension) || (!playerCore && sector.dimension != dimension))
            {
                continue;
            }

            if (sector.bounds.x < minX)
            {
                minX = sector.bounds.x;
            }

            if (sector.bounds.y > maxY)
            {
                maxY = sector.bounds.y;
            }
        }

        foreach (var sector in sectors)
        {
            if ((playerCore && sector.dimension != playerCore.Dimension) || (!playerCore && sector.dimension != dimension))
            {
                continue;
            }

            gridSizeX = Mathf.Max(gridSizeX, (sector.bounds.x + sector.bounds.w - minX) / zoomoutFactor);
            gridSizeY = Mathf.Max(gridSizeY, -(sector.bounds.y - sector.bounds.h - maxY) / zoomoutFactor);
        }
    }

    private void CreateMinimapImages(Sector sector)
    {
        foreach (var ent in sector.entities)
        {
            bool carrier = false;
            if (ent.assetID == "carrier_blueprint" || ent.assetID == "groundcarrier_blueprint")
            {
                carrier = true;
            }
            var markerResourceName = "";
            switch (ent.assetID)
            {
                case "outpost_blueprint":
                    markerResourceName = "outpost_minimap_sprite";
                    break;
                case "bunker_blueprint":
                    markerResourceName = "bunker_minimap_sprite";
                    break;
                case "carrier_blueprint":
                case "groundcarrier_blueprint":
                case "missile_station":
                case "air_weapon_station":
                case "energy_rock":
                    markerResourceName = "minimap_sprite";
                    break;
            }

            switch (ent.assetID)
            {
                case "outpost_blueprint":
                case "bunker_blueprint":
                case "carrier_blueprint":
                case "groundcarrier_blueprint":
                case "missile_station":
                case "air_weapon_station":
                case "energy_rock":
                    var gObj = new GameObject();
                    gObj.transform.SetParent(transform, false);
                    var img = gObj.AddComponent<Image>();
                    img.sprite = ResourceManager.GetAsset<Sprite>(markerResourceName);
                    img.color = FactionManager.GetFactionColor(ent.faction);
                    gObj.GetComponent<RectTransform>().sizeDelta = new Vector2(7, 7) / zoomoutFactor;
                    if (ent.assetID == "energy_rock")
                    {
                        gObj.GetComponent<RectTransform>().sizeDelta /= 2;
                        img.color = new Color32(0, 163, 255, 255);
                    }

                    if (carrier) gObj.GetComponent<RectTransform>().sizeDelta *= 1.25F;
                    gObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(ent.position.x - minX, -maxY + ent.position.y) / zoomoutFactor;
                    break;
            }
        }
    }

    private bool DrawSector(Sector sector, int dimension, LandPlatformGenerator lpg, bool displayStations)
    {
        // get every sector to find their representations
        if (SectorManager.testJsonPath == null && !(mapVisibleCheatEnabled || (!playerCore || playerCore.cursave.sectorsSeen.Contains(sector.sectorName)))) 
        {
            return false;
        }
        if ((playerCore && sector.dimension != playerCore.Dimension) || (!playerCore && sector.dimension != dimension))
        {
            return false;
        }

        // set up the sector image
        var sectorStartX = sector.bounds.x - minX;
        var sectorStartY = -maxY + sector.bounds.y;
        Image sect = Instantiate(sectorPrefab, transform, false);
        Image border = sect.GetComponentsInChildren<Image>()[1];
        sect.color = 1.2F * sector.backgroundColor - new Color(0.2F, 0.2F, 0.2F, 0.75F);
        border.color = new Color(1F, 1F, 1F, 0.5F);
        sect.rectTransform.anchoredPosition = new Vector2(sectorStartX, sectorStartY) / zoomoutFactor;

        // Set up markers.
        if (PartIndexInventoryButton.partMarkerSectorNames.Contains(sector.sectorName))
        {
            Image marker = new GameObject($"Marker {sector.sectorName}").AddComponent<Image>();
            marker.transform.SetParent(transform);
            marker.rectTransform.sizeDelta = new Vector2(5, 5);
            marker.rectTransform.anchoredPosition = sect.rectTransform.anchoredPosition;
            marker.rectTransform.anchoredPosition += (new Vector2(sector.bounds.w, -sector.bounds.h) / (2 * zoomoutFactor));
            marker.color = new Color(0, 1, 0, 1);
            partOriginMarkerImages.Add(marker);
        }

        // set up the border image
        border.rectTransform.sizeDelta = sect.rectTransform.sizeDelta = new Vector2(sector.bounds.w, sector.bounds.h) / zoomoutFactor;
        sectorImages.Add((sect, new Vector3(sector.bounds.x + sector.bounds.w / 2, sector.bounds.y - sector.bounds.h / 2)));
        sectorInfo.Add(sect, (sector.sectorName, sector.type));

        CreateLandPlatforms(sector, lpg, sect);
        // set up minimap images if necessary
        if (!displayStations) return true;
        CreateMinimapImages(sector);
        return true;
    }

    IEnumerator DrawCoroutine(List<Sector> sectors, int dimension, bool displayStations)
    {
        if (player)
        {
            sectors = new List<Sector>(sectors.ToArray());
            sectors.Sort((x, y) => 
            {
                var dx = new Vector3(x.bounds.x + x.bounds.w / 2, x.bounds.y - x.bounds.h / 2);
                var dy = new Vector3(y.bounds.x + y.bounds.w / 2, y.bounds.y - y.bounds.h / 2);
                return (int)(dx - player.transform.position).sqrMagnitude - (int)(dy - player.transform.position).sqrMagnitude;
            });
        }
        gridImg.rectTransform.sizeDelta = new Vector2((gridSizeX * zoomoutFactor + distancePerTextMarker) / zoomoutFactor,
            (gridSizeY * zoomoutFactor + distancePerTextMarker) / zoomoutFactor);
        // round to multiple of 100 to maintain grid lining
        gridImg.rectTransform.sizeDelta = new Vector2(((int)gridImg.rectTransform.sizeDelta.x / 100 + 0.5F) * 100,
            ((int)gridImg.rectTransform.sizeDelta.y / 100 + 0.5F) * 100);

        var lpg = LandPlatformGenerator.Instance;

        for (int i = 0; i < Mathf.Max(gridSizeX, gridSizeY) * zoomoutFactor / distancePerTextMarker + 1; i++)
        {
            AddMapDistanceMarker(new Vector2(i * distancePerTextMarker + const4 * zoomoutFactor, const1 * zoomoutFactor) / zoomoutFactor, i, TextAnchor.LowerLeft);
            if (i != 0)
                AddMapDistanceMarker(new Vector2(const2 * zoomoutFactor, -i * distancePerTextMarker - const4 * zoomoutFactor) / zoomoutFactor, i, TextAnchor.UpperLeft);
        }

        if (player)
        {
            if (anchor == Vector2.zero)
            {
                Vector2 playerPos = new Vector2(player.transform.position.x - minX, player.transform.position.y - maxY);
                Vector2 vec = -playerPos / zoomoutFactor;
                vec.x = Mathf.Max(0, vec.x - canvas.sizeDelta.x / 2);
                vec.y = Mathf.Max(0, vec.y - canvas.sizeDelta.y / 2);
                canvas.anchoredPosition = vec;
                anchor = canvas.anchoredPosition;
            }
            else
            {
                canvas.anchoredPosition = anchor;
            }
        }

        int x = 0;
        foreach (Sector sector in sectors)
        {
            var drew = DrawSector(sector, dimension, lpg, displayStations);
            if (drew) x++;
            if (x >= 10)
            {
                x = 0;
                yield return new WaitForEndOfFrame();
            }
        }

        // clear markers
        PartIndexInventoryButton.partMarkerSectorNames.Clear();

        // draw objective locations
        if (player)
        {
            DrawObjectiveLocations();
        }
        foreach(var c in GetComponentsInParent<Canvas>().Reverse())
        {
            c.sortingOrder = ++PlayerViewScript.currentLayer;   
        }
        yield return null;
    }

    private Coroutine coroutine;

    void Draw(List<Sector> sectors, int zoomoutFactor = 4, int dimension = 0, bool resetPosition = true, bool displayStations = false)
    {
        InitializeVariables(zoomoutFactor, resetPosition);
        DefineGridSize(sectors, dimension);
        
        if (coroutine != null) StopCoroutine(coroutine);
        if (!gameObject.activeInHierarchy) return;
        instance.arrows.Clear();
        coroutine = StartCoroutine(DrawCoroutine(sectors, dimension, displayStations));
    }

    private void AddMapDistanceMarker(Vector2 pos, int i, TextAnchor anchor)
    {
        Text textx = new GameObject().AddComponent<Text>();
        textx.font = shellcoreFont;
        textx.transform.SetParent(transform, false);
        textx.rectTransform.anchoredPosition = pos;
        textx.rectTransform.localScale = Vector3.one;
        textx.alignment = anchor;
        textx.text = (i * distancePerTextMarker).ToString();
        textx.fontSize = 12;
        textx.color = gridImg.color + Color.gray;
        textx.raycastTarget  = false;
    }


    // Draw arrows signifying objective locations. Do not constantly call this method.
    public static void DrawObjectiveLocations()
    {
        if (!instance || !instance.player) return;
        // clear the dictionary, then recreate the arrows
        foreach (var rectTransform in instance.arrows.Values)
        {
            if (rectTransform && rectTransform.gameObject)
            {
                Destroy(rectTransform.gameObject);
            }
        }

        instance.arrows.Clear();

        foreach (var ls in TaskManager.objectiveLocations.Values)
        {
            foreach (var loc in ls)
            {
                AddArrow(loc);
            }
        }

        if (!CoreScriptsManager.instance) return;
        foreach (var loc in CoreScriptsManager.instance.objectiveLocations.Values)
        {
            AddArrow(loc);
        }
    }

    private static void AddArrow(ObjectiveLocation loc)
    {
        if (loc.dimension != PlayerCore.Instance.Dimension) return;
        var arrow = Instantiate(instance.mapArrowPrefab, instance.transform, false);
        arrow.GetComponent<Image>().color = Color.red + Color.green / 2;
        instance.arrows.Add(loc, arrow.GetComponent<RectTransform>());
        arrow.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(loc.location.x - instance.minX, loc.location.y - instance.maxY) / instance.zoomoutFactor;
    }

    public static void EnableMapCheat()
    {
        mapVisibleCheatEnabled = true;
        if (instance && instance.manager)
        {
            instance.Destroy();
            instance.Draw(instance.manager.sectors);
        }
    }

    public static void DisableMapCheat()
    {
        mapVisibleCheatEnabled = false;
    }


    RectTransform canvas;

    void Awake()
    {
        canvas = GetComponent<RectTransform>();
        canvas.anchoredPosition = new Vector3(0, 0);
    }

    void Update()
    {
        if (playerCore && playerCore.cursave.sectorsSeen.Count > sectorCount && !mapVisibleCheatEnabled)
        {
            sectorCount = playerCore.cursave.sectorsSeen.Count;
            Destroy();
            Draw(manager.sectors, 4, playerCore.Dimension, false);
        }

        if (clickedOnce)
        {
            if (timer > 0.2F)
            {
                clickedOnce = false;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }

        if (!followPlayerMode)
        {
            PollMouseFollow();
        }
        else if (player)
        {
            greenBox.anchoredPosition = new Vector2(canvas.sizeDelta.x, -canvas.sizeDelta.y) / 2;
            canvas.anchoredPosition = new Vector2(-player.position.x + minX, maxY - player.position.y) / zoomoutFactor + greenBox.anchoredPosition;
            anchor = canvas.anchoredPosition;
        }

        if (!Input.GetMouseButton(0) && !followPlayerMode && updatePos)
        {
            updatePos = false;
            anchor = canvas.anchoredPosition;
        }


        if (player)
        {
            foreach (var objective in arrows.Keys)
            {
                arrows[objective].anchoredPosition = new Vector2(objective.location.x - minX, objective.location.y - maxY) / zoomoutFactor;
            }
        }

        
        InstantiateTooltip();

        // Blink markers.
        bool markerActive = (Time.time % 2 > 1);
        foreach (var marker in partOriginMarkerImages)
        {
            marker.enabled = markerActive;
        }
    }

    private void InstantiateTooltip()
    {

        // Instantiate tooltip. Destroy tooltip if mouse is not over a sector image.
        bool mouseOverSector = false;
        foreach (var sect in sectorImages)
        {
            var pos = sect.Item1.rectTransform.position;
            var sizeDelta = sect.Item1.rectTransform.sizeDelta / UIScalerScript.GetScale();
            var newRect = new Rect(pos.x, pos.y - sizeDelta.y, sizeDelta.x, sizeDelta.y);
            // Mouse over sector. Instantiate tooltip if necessary, move tooltip and set text up

            if (newRect.Contains(Input.mousePosition) && mouseInBounds)
            {
                if (!tooltipTransform)
                {
                    tooltipTransform = Instantiate(tooltipPrefab, transform.parent.parent).GetComponent<RectTransform>();
                }

                tooltipTransform.position = Input.mousePosition;
                mouseOverSector = true;
                var text = tooltipTransform.GetComponentInChildren<Text>();
                text.text =
                    $"{sectorInfo[sect.Item1].Item1}".ToUpper();

                foreach (var objective in arrows.Keys)
                {
                    var img = arrows[objective].GetComponent<Image>();
                    var imgpos = img.rectTransform.position;
                    var imgsizeDelta = img.rectTransform.sizeDelta;
                    var imgnewRect = new Rect(imgpos.x - imgsizeDelta.x / 2, imgpos.y, imgsizeDelta.x, imgsizeDelta.y);
                    if (imgnewRect.Contains(Input.mousePosition))
                    {
                        var missionName = CoreScriptsManager.instance.GetLocalMapString(objective.missionName);
                        if (string.IsNullOrEmpty(missionName)) missionName = objective.missionName;
                        text.text += $"\nCLICK TO VIEW MISSION: {missionName.ToUpper()}";
                        break;
                    }
                }

                tooltipTransform.GetComponent<RectTransform>().sizeDelta = new Vector2(text.preferredWidth + 16f, text.preferredHeight + 16);
            }
        }


        if (!mouseOverSector)
        {
            if (tooltipTransform)
            {
                Destroy(tooltipTransform.gameObject);
            }
        }
    }


    private void UpdateMapPosition(Vector2 vec)
    {
            var width = canvas.sizeDelta.x;
            var height = canvas.sizeDelta.y;
            canvas.anchoredPosition = anchor + vec;
            canvas.anchoredPosition = new Vector2(Mathf.Min(canvas.anchoredPosition.x, 0), Mathf.Max(canvas.anchoredPosition.y, 0));
            canvas.anchoredPosition = new Vector2(Mathf.Max(canvas.anchoredPosition.x,
                    -gridImg.rectTransform.sizeDelta.x + width),
                //(gridSizeY * zoomoutFactor + distancePerTextMarker) / zoomoutFactor - height )
                Mathf.Min(canvas.anchoredPosition.y, gridImg.rectTransform.sizeDelta.y - height));
    }


    void PollMouseFollow()
    {
        if (updatePos)
        {
            UpdateMapPosition((Vector2)Input.mousePosition - mousePos);
        }

        if (player)
        {
            Vector2 playerPos = new Vector2(player.transform.position.x - minX, player.transform.position.y - maxY);
            greenBox.anchoredPosition = canvas.anchoredPosition + playerPos / zoomoutFactor;
        }
    }

    void Destroy()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject); // destroy stray children
        }

        if (tooltipTransform)
        {
            Destroy(tooltipTransform.gameObject);
        }

        sectorImages.Clear();
        sectorInfo.Clear();
        partOriginMarkerImages.Clear();
    }

    void OnDisable()
    {
        Destroy();
    }

    bool updatePos;
    Vector2 mousePos;
    Vector2 anchor;
    float timer;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (arrows == null)
        {
            return;
        }

        foreach (var objective in arrows.Keys)
        {
            var img = arrows[objective].GetComponent<Image>();
            if (!img)
            {
                continue;
            }
            var imgpos = img.rectTransform.position;
            var imgsizeDelta = img.rectTransform.sizeDelta / UIScalerScript.GetScale();
            var imgnewRect = new Rect(imgpos.x - imgsizeDelta.x / 2, imgpos.y, imgsizeDelta.x, imgsizeDelta.y);
            if (imgnewRect.Contains(Input.mousePosition))
            {
                if (StatusMenu.instance)
                {
                    StatusMenu.instance.SwitchSections(1);
                }

                TaskDisplayScript.ShowMission(PlayerCore.Instance.cursave.missions.Find(m => m.name == objective.missionName));
                return;
            }
        }

        if (!followPlayerMode)
        {
            updatePos = true;
            mousePos = Input.mousePosition;
        }


        if ((SectorManager.testJsonPath != null || DevConsoleScript.WarpingEnabled) && !DialogueSystem.isInCutscene && !DialogueSystem.Instance.IsWindowActive())
        {
            foreach (var sect in sectorImages)
            {
                if (!sect.Item1)
                {
                    continue;
                }

                var pos = sect.Item1.rectTransform.position;
                var sizeDelta = sect.Item1.rectTransform.sizeDelta / UIScalerScript.GetScale();
                var newRect = new Rect(pos.x, pos.y - sizeDelta.y, sizeDelta.x, sizeDelta.y);
                if (newRect.Contains(eventData.position) && player)
                {
                    player.GetComponent<PlayerCore>().Warp(sect.Item2);
                }
            }
        }
    }

    bool clickedOnce;
    bool followPlayerMode;
    private static bool mapVisibleCheatEnabled = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickedOnce && player)
        {
            followPlayerMode = !followPlayerMode;
            if (!followPlayerMode)
            {
                canvas.anchoredPosition = anchor;
                mousePos = Input.mousePosition;
                updatePos = true;
                PollMouseFollow();
            }
        }
        else
        {
            timer = 0;
            clickedOnce = true;
        }
    }

    bool mouseInBounds;

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseInBounds = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GetComponent<RectTransform>().rect.Contains(Input.mousePosition)) return;
        mouseInBounds = false;
    }
}
