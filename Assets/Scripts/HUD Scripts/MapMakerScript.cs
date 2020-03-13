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
	int const3 = 2000;

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
		for(int i = 0; i < 20; i++)
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

		foreach(var sector in manager.sectors) 
        {
            if(sector.bounds.x < minX) minX = sector.bounds.x;
            if(sector.bounds.y > maxY) maxY = sector.bounds.y;
		}

		foreach(Sector sector in manager.sectors) { // get every sector to find their representations
			if(SectorManager.testJsonPath != null || playerCore.cursave.sectorsSeen.Contains(sector.sectorName))
			{
				Image sect = Instantiate(sectorPrefab, transform, false);
				sect.transform.SetAsFirstSibling();
				Image body = sect.GetComponentsInChildren<Image>()[1];
				sect.color = sector.backgroundColor - new Color(0.2F, 0.2F, 0.2F, 0);
				body.color = sect.color + 0.75F * Color.white;
				sect.rectTransform.anchoredPosition = new Vector2(sector.bounds.x - minX, -maxY + sector.bounds.y) / zoomoutFactor;
				body.rectTransform.sizeDelta = sect.rectTransform.sizeDelta = new Vector2(sector.bounds.w, sector.bounds.h) / zoomoutFactor;
				sectorImages.Add((sect, new Vector3(sector.bounds.x + sector.bounds.w / 2, sector.bounds.y - sector.bounds.h / 2)));
			}
		}

		// draw objective locations
		DrawObjectiveLocations();
	}

	private List<(Image, Vector3)> sectorImages = new List<(Image, Vector3)>();

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

			foreach(var loc in TaskManager.objectiveLocations)
			{
				var arrow = Instantiate(instance.mapArrowPrefab, instance.transform, false);
				arrow.GetComponent<Image>().color = Color.red + Color.green / 2;
				instance.arrows.Add(loc, arrow.GetComponent<RectTransform>());
				arrow.GetComponent<RectTransform>().anchoredPosition = 
					new Vector2(loc.location.x - instance.minX, loc.location.y - instance.maxY) / instance.zoomoutFactor;
			}
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
			PollFollowPlayer();
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
	}

	void PollFollowPlayer()
	{
		canvas.anchorMin = new Vector2(0,1);
		canvas.anchorMax = new Vector2(0,1);
		Vector2 playerPos = new Vector2(player.transform.position.x - minX, player.transform.position.y - maxY);

		if(updatePos)
		{
			var width = canvas.sizeDelta.x;
			canvas.anchoredPosition = anchor + (Vector2)Input.mousePosition - mousePos;
			canvas.anchoredPosition = new Vector2(Mathf.Min(canvas.anchoredPosition.x, 0), Mathf.Max(canvas.anchoredPosition.y, 0));
			canvas.anchoredPosition = new Vector2(Mathf.Max(canvas.anchoredPosition.x, -((const3 - width)/zoomoutFactor - const1) / zoomoutFactor), Mathf.Min(canvas.anchoredPosition.y, ((const3 - width)/zoomoutFactor - const1) / zoomoutFactor));
		}
		greenBox.anchoredPosition =  canvas.anchoredPosition + playerPos / zoomoutFactor;
	}
	void Destroy()
	{
		for(int i = 0; i < transform.childCount; i++) {
			Destroy(transform.GetChild(i).gameObject); // destroy stray children
		}
		sectorImages.Clear();
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

		if(SectorManager.testJsonPath != null)
			foreach(var sect in sectorImages)
			{
				var pos = sect.Item1.rectTransform.position;
				var sizeDelta = sect.Item1.rectTransform.sizeDelta;
				var newRect = new Rect(pos.x, pos.y - sizeDelta.y, sizeDelta.x, sizeDelta.y);
				if(newRect.Contains(eventData.position))
				{
					Debug.Log("click");
					player.GetComponent<AirCraft>().Warp(sect.Item2);
				}
			}
    }
}
