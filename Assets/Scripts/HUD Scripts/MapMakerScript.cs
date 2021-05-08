using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapMakerScript : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler {

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
		playerCore = player.GetComponent<PlayerCore>();
		Draw();
	}

	// I have barely any idea why these constants are their values, but it works with these
	int const1 = 150;
	int const2 = 200;
	int const4 = 200;

	// this is an arbitrary number, sets the size of the grid
	int const3 = 4000;

	Dictionary<TaskManager.ObjectiveLocation, RectTransform> arrows = new Dictionary<TaskManager.ObjectiveLocation, RectTransform>();
	void Draw()
	{
		Image img = new GameObject().AddComponent<Image>();
		img.transform.SetParent(transform, false);
		img.rectTransform.localScale = Vector3.one;
		img.rectTransform.pivot = new Vector2(0,1);
		img.rectTransform.sizeDelta = new Vector2(const3, const3);
		img.rectTransform.anchoredPosition = Vector2.zero;
		img.sprite = gridSprite;
		img.type = Image.Type.Tiled;
		img.color = new Color32(100, 100, 100, 255);

		foreach(var sector in manager.sectors) 
        {
            if(sector.bounds.x < minX) minX = sector.bounds.x;
            if(sector.bounds.y > maxY) maxY = sector.bounds.y;
		}

		foreach(Sector sector in manager.sectors) { // get every sector to find their representations
			if(SectorManager.testJsonPath != null || (mapVisibleCheatEnabled || playerCore.cursave.sectorsSeen.Contains(sector.sectorName)))
			{
				Image sect = Instantiate(sectorPrefab, transform, false);
				Image border = sect.GetComponentsInChildren<Image>()[1];
				sect.color =  1.2F * sector.backgroundColor - new Color(0.2F, 0.2F, 0.2F, 0.75F); 
				border.color = new Color(1F, 1F, 1F, 0.5F);
				sect.rectTransform.anchoredPosition = new Vector2(sector.bounds.x - minX, -maxY + sector.bounds.y) / zoomoutFactor;

				// Set up markers.
				if(PartIndexInventoryButton.partMarkerSectorNames.Contains(sector.sectorName))
				{
					Image marker = new GameObject($"Marker {sector.sectorName}").AddComponent<Image>();
					marker.transform.SetParent(transform);
					marker.rectTransform.sizeDelta = new Vector2(5, 5);
					marker.rectTransform.anchoredPosition = sect.rectTransform.anchoredPosition;
					marker.rectTransform.anchoredPosition += (new Vector2(sector.bounds.w, -sector.bounds.h) / (2 * zoomoutFactor));
					marker.color = new Color(0,1,0,1);
					partOriginMarkerImages.Add(marker);
				}

				border.rectTransform.sizeDelta = sect.rectTransform.sizeDelta = new Vector2(sector.bounds.w, sector.bounds.h) / zoomoutFactor;
				sectorImages.Add((sect, new Vector3(sector.bounds.x + sector.bounds.w / 2, sector.bounds.y - sector.bounds.h / 2)));
				sectorInfo.Add(sect, (sector.sectorName, sector.type));


                var lpg = LandPlatformGenerator.Instance;

                if (sector.platforms == null && sector.platformData.Length > 0)
                {
                    GameObject prefab = ResourceManager.GetAsset<GameObject>(LandPlatformGenerator.prefabNames[0]);
                    float tileSize = prefab.GetComponent<SpriteRenderer>().bounds.size.x;
                    lpg.tileSize = tileSize;

                    var cols = sector.bounds.h / (int)tileSize;
                    var rows = sector.bounds.w / (int)tileSize;

                    Vector2 center = new Vector2(sector.bounds.x + sector.bounds.w / 2, sector.bounds.y - sector.bounds.h / 2);

                    Vector2 offset = new Vector2
                    {
                        x = center.x - tileSize * (rows - 1) / 2F,
                        y = center.y + tileSize * (cols - 1) / 2F
                    };

                    lpg.Offset = offset;

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
                        float tileSize = LandPlatformGenerator.Instance.tileSize / zoomoutFactor;

                        List<Vector2> vertices = new List<Vector2>();
                        for (int i = 0; i < platform.tiles.Count; i++)
                        {
                            var tile = platform.tiles[i];

                            var pos = new Vector2(tile.pos.x, -tile.pos.y - 1f) * tileSize;

                            vertices.Add(new Vector3(pos.x + tileSize, pos.y + tileSize));
                            vertices.Add(new Vector3(pos.x, pos.y + tileSize));
                            vertices.Add(new Vector3(pos.x, pos.y));
                            vertices.Add(new Vector3(pos.x + tileSize, pos.y));
                        }

                        if (vertices.Count > 0)
                        {
                            var obj = new GameObject("LandPlatformMesh");
                            obj.transform.SetParent(transform);
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
		}

		for(int i = 0; i < 21; i++)
		{
			Text textx = new GameObject().AddComponent<Text>();
			Text texty = new GameObject().AddComponent<Text>();
			textx.font = texty.font = shellcoreFont;
			textx.transform.SetParent(transform,false);
			texty.transform.SetParent(transform,false);
			textx.rectTransform.anchoredPosition = new Vector2(i * 200 + const4, const1) / zoomoutFactor;
			texty.rectTransform.anchoredPosition = new Vector2(const2, -i * 200 - const4) / zoomoutFactor;
			textx.rectTransform.localScale = texty.rectTransform.localScale = Vector3.one;
			textx.alignment = TextAnchor.LowerLeft;
			texty.alignment = TextAnchor.UpperLeft;
			textx.text = texty.text = i * 200 + "";
			textx.fontSize = texty.fontSize = 12;
			textx.color = texty.color = img.color + Color.gray;

		}

		// draw objective locations
		DrawObjectiveLocations();

		// clear markers
		PartIndexInventoryButton.partMarkerSectorNames.Clear();
	}

	// Draw arrows signifying objective locations. Do not constantly call this method.
	public static void DrawObjectiveLocations()
	{
		if(instance)
		{
			// clear the dictionary, then recreate the arrows
			foreach(var rectTransform in instance.arrows.Values)
			{
				if(rectTransform && rectTransform.gameObject) Destroy(rectTransform.gameObject);
			}
			instance.arrows.Clear();

			foreach(var ls in TaskManager.objectiveLocations.Values)
			{
				foreach(var loc in ls)
				{
					var arrow = Instantiate(instance.mapArrowPrefab, instance.transform, false);
					arrow.GetComponent<Image>().color = Color.red + Color.green / 2;
					instance.arrows.Add(loc, arrow.GetComponent<RectTransform>());
					arrow.GetComponent<RectTransform>().anchoredPosition = 
						new Vector2(loc.location.x - instance.minX, loc.location.y - instance.maxY) / instance.zoomoutFactor;
				}
			}
		}
		
	}

	public static void EnableMapCheat()
	{
		mapVisibleCheatEnabled = true;
		if(instance)
		{
			instance.Destroy();
			instance.Draw();
		}
	}

	RectTransform canvas;
	void Awake()
	{
		canvas = GetComponent<RectTransform>();
		canvas.anchoredPosition = new Vector3(0, 0);
	}
	void Update() {
		if(playerCore.cursave.sectorsSeen.Count > sectorCount)
		{
			sectorCount = playerCore.cursave.sectorsSeen.Count;
			Destroy();
			Draw();
		}
		
		if(clickedOnce)
		{
			if(timer > 0.2F)
			{
				clickedOnce = false;
			}
			else timer += Time.deltaTime;
		}

		if(!followPlayerMode)
		{
			PollMouseFollow();
		}
		else
		{
			canvas.anchorMin = new Vector2(0, 1);
			canvas.anchorMax = new Vector2(0, 1);
			greenBox.anchoredPosition = new Vector2(canvas.sizeDelta.x, -canvas.sizeDelta.y) / 2;
			canvas.anchoredPosition = new Vector2(-player.position.x + minX, maxY - player.position.y) / zoomoutFactor + greenBox.anchoredPosition;
		}

		foreach(var objective in arrows.Keys)
		{
			arrows[objective].anchoredPosition = new Vector2(objective.location.x - minX, objective.location.y - maxY) / zoomoutFactor;
		}

		// Instantiate tooltip. Destroy tooltip if mouse is not over a sector image.
		bool mouseOverSector = false;
		foreach(var sect in sectorImages)
		{
			var pos = sect.Item1.rectTransform.position;
			var sizeDelta = sect.Item1.rectTransform.sizeDelta;
			var newRect = new Rect(pos.x, pos.y - sizeDelta.y, sizeDelta.x, sizeDelta.y);

			// Mouse over sector. Instantiate tooltip if necessary, move tooltip and set text up
			if(newRect.Contains(Input.mousePosition))
			{
				if(!tooltipTransform) tooltipTransform = Instantiate(tooltipPrefab, transform.parent.parent).GetComponent<RectTransform>();
				tooltipTransform.position = Input.mousePosition;
				mouseOverSector = true;
				var text = tooltipTransform.GetComponentInChildren<Text>();
				text.text = 
					$"{sectorInfo[sect.Item1].Item1}".ToUpper();

				foreach(var objective in arrows.Keys)
				{
					var img = arrows[objective].GetComponent<Image>();
					var imgpos = img.rectTransform.position;
					var imgsizeDelta = img.rectTransform.sizeDelta;
					var imgnewRect = new Rect(imgpos.x - imgsizeDelta.x / 2, imgpos.y, imgsizeDelta.x, imgsizeDelta.y);
					if(imgnewRect.Contains(Input.mousePosition))
					{
						text.text += $"\nCLICK TO VIEW MISSION: {objective.missionName.ToUpper()}";
						break;
					}
				}

				tooltipTransform.GetComponent<RectTransform>().sizeDelta = new Vector2(text.preferredWidth + 16f, text.preferredHeight + 16);
			}
		}

		if(!mouseOverSector) 
		{
			if(tooltipTransform) Destroy(tooltipTransform.gameObject);
		}

		// Blink markers.
		bool markerActive = (Time.time % 2 > 1);
		foreach(var marker in partOriginMarkerImages)
		{
			marker.enabled = markerActive;
		}
	}

	void PollMouseFollow()
	{
		canvas.anchorMin = new Vector2(0,1);
		canvas.anchorMax = new Vector2(0,1);
		Vector2 playerPos = new Vector2(player.transform.position.x - minX, player.transform.position.y - maxY);

		if(updatePos)
		{
			var width = canvas.sizeDelta.x;
			canvas.anchoredPosition = anchor + (Vector2)Input.mousePosition - mousePos;
			canvas.anchoredPosition = new Vector2(Mathf.Min(canvas.anchoredPosition.x, 0), Mathf.Max(canvas.anchoredPosition.y, 0));
			canvas.anchoredPosition = new Vector2(Mathf.Max(canvas.anchoredPosition.x, -((const3 - width)/zoomoutFactor - const1) / zoomoutFactor), Mathf.Min(canvas.anchoredPosition.y, (1400 + (const3 - width)/zoomoutFactor - const1) / zoomoutFactor));
		}
		greenBox.anchoredPosition =  canvas.anchoredPosition + playerPos / zoomoutFactor;
	}
	void Destroy()
	{
		for(int i = 0; i < transform.childCount; i++) {
			Destroy(transform.GetChild(i).gameObject); // destroy stray children
		}
		if(tooltipTransform) Destroy(tooltipTransform.gameObject);
		sectorImages.Clear();
		sectorInfo.Clear();
		partOriginMarkerImages.Clear();
	}
	void OnDisable() {
		Destroy();
	}

	bool updatePos;
	Vector2 mousePos;
	Vector2 anchor;
	float timer;
    public void OnPointerDown(PointerEventData eventData)
    {
		if(!followPlayerMode)
		{
			updatePos = true;
			mousePos = Input.mousePosition;
		}

		foreach(var objective in arrows.Keys)
		{
			var img = arrows[objective].GetComponent<Image>();
			var imgpos = img.rectTransform.position;
			var imgsizeDelta = img.rectTransform.sizeDelta;
			var imgnewRect = new Rect(imgpos.x - imgsizeDelta.x / 2, imgpos.y, imgsizeDelta.x, imgsizeDelta.y);
			if(imgnewRect.Contains(Input.mousePosition))
			{
				StatusMenu.instance.SwitchSections(1);
				TaskDisplayScript.ShowMission(PlayerCore.Instance.cursave.missions.Find(m => m.name == objective.missionName));
				return;
			}
		}

		if(SectorManager.testJsonPath != null || DevConsoleScript.godModeEnabled == true)
			foreach(var sect in sectorImages)
			{
				var pos = sect.Item1.rectTransform.position;
				var sizeDelta = sect.Item1.rectTransform.sizeDelta;
				var newRect = new Rect(pos.x, pos.y - sizeDelta.y, sizeDelta.x, sizeDelta.y);
				if(newRect.Contains(eventData.position))
				{
					player.GetComponent<PlayerCore>().Warp(sect.Item2);
				}
			}
    }

    public void OnPointerUp(PointerEventData eventData)
    {
		if(!followPlayerMode)
		{
			updatePos = false;
			anchor = canvas.anchoredPosition;
		}
    }

	bool clickedOnce;
	bool followPlayerMode;
	private static bool mapVisibleCheatEnabled = false;
    public void OnPointerClick(PointerEventData eventData)
    {
        if(clickedOnce)
		{
			followPlayerMode = !followPlayerMode;
			if(!followPlayerMode) 
			{
				canvas.anchoredPosition = anchor;
				canvas.anchorMin = new Vector2(0,1);
				canvas.anchorMax = new Vector2(0,1);
			}
		} 
		else 
		{
			timer = 0;
			clickedOnce = true;
		}
    }
}
