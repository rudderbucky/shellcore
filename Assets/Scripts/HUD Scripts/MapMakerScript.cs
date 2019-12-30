using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapMakerScript : MonoBehaviour {

	public Image sectorPrefab;
	public SectorManager manager;

	PlayerCore playerCore;
	public Transform player;
	public Font shellcoreFont;
	public Sprite gridSprite;
	RectTransform indicator;
	int zoomoutFactor = 2;
	int sectorCount = 0;
	// Use this for initialization
	void OnEnable() 
	{
		playerCore = player.GetComponent<PlayerCore>();
		Draw();
	}

	void Draw()
	{
		Image img = new GameObject().AddComponent<Image>();
		img.transform.parent = transform;
		img.rectTransform.localScale = Vector3.one;
		img.rectTransform.sizeDelta = new Vector2(1000, 1000);
		img.rectTransform.anchoredPosition = Vector2.zero;
		img.sprite = gridSprite;
		img.type = Image.Type.Tiled;
		img.color = new Color32(80, 80, 80, 255);
		for(int i = 0; i < 20; i++)
		{
			Text textx = new GameObject().AddComponent<Text>();
			Text texty = new GameObject().AddComponent<Text>();
			textx.font = texty.font = shellcoreFont;
			textx.transform.parent = texty.transform.parent = transform;
			textx.rectTransform.anchoredPosition = new Vector2(i * 100, 125) / zoomoutFactor;
			texty.rectTransform.anchoredPosition = new Vector2(-125, -i * 100) / zoomoutFactor;
			textx.rectTransform.localScale = texty.rectTransform.localScale = Vector3.one;
			textx.alignment = TextAnchor.LowerCenter;
			texty.alignment = TextAnchor.MiddleRight;
			textx.text = texty.text = i * 100 + "";

		}

		foreach(Sector sector in manager.sectors) { // get every sector to find their representations
			if(playerCore.cursave.sectorsSeen.Contains(sector.sectorName))
			{
				Debug.Log(player.GetComponent<PlayerCore>().cursave.sectorsSeen.Count);
				Image sect = Instantiate(sectorPrefab, transform, false);
				sect.transform.SetAsFirstSibling();
				Image body = sect.GetComponentsInChildren<Image>()[1];
				sect.color = sector.backgroundColor;
				body.color = sect.color + Color.grey;
				sect.rectTransform.anchoredPosition = new Vector2(sector.bounds.x, sector.bounds.y) / zoomoutFactor;
				body.rectTransform.sizeDelta = sect.rectTransform.sizeDelta = new Vector2(sector.bounds.w, sector.bounds.h) / zoomoutFactor;
			}
		}
	}

	void Update() {
		if(playerCore.cursave.sectorsSeen.Count > sectorCount)
		{
			sectorCount = playerCore.cursave.sectorsSeen.Count;
			Destroy();
			Draw();
		}
		GetComponent<RectTransform>().anchoredPosition = -player.transform.position / zoomoutFactor;
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
}
