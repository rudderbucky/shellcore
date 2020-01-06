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
	// Use this for initialization
	void OnEnable() 
	{
		playerCore = player.GetComponent<PlayerCore>();
		Draw();
	}

	// I have barely any idea why these constants are their values, but it works with these
	int const1 = 150;
	int const2 = 200;
	int const4 = 200;

	// this is an arbitrary number, sets the size of the grid
	int const3 = 2000;
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
			textx.color = texty.color = img.color + Color.gray;

		}

		foreach(var sector in manager.sectors) 
        {
            if(sector.bounds.x < minX) minX = sector.bounds.x;
            if(sector.bounds.y > maxY) maxY = sector.bounds.y;
		}

		foreach(Sector sector in manager.sectors) { // get every sector to find their representations
			if(playerCore.cursave.sectorsSeen.Contains(sector.sectorName))
			{
				Image sect = Instantiate(sectorPrefab, transform, false);
				sect.transform.SetAsFirstSibling();
				Image body = sect.GetComponentsInChildren<Image>()[1];
				sect.color = sector.backgroundColor - new Color(0.2F, 0.2F, 0.2F, 0);
				body.color = sect.color + 0.75F * Color.white;
				sect.rectTransform.anchoredPosition = new Vector2(sector.bounds.x - minX, -maxY + sector.bounds.y) / zoomoutFactor;
				body.rectTransform.sizeDelta = sect.rectTransform.sizeDelta = new Vector2(sector.bounds.w, sector.bounds.h) / zoomoutFactor;
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
    }
}
