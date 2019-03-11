using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapMakerScript : MonoBehaviour {

	public Image sectorPrefab;
	public SectorManager manager;
	public Transform player;
	RectTransform indicator;
	// Use this for initialization
	void OnEnable() {
		//indicator = (RectTransform)transform.GetChild(0);
		foreach(Sector sector in manager.sectors) { // get every sector to find their representations
			Image sect = Instantiate(sectorPrefab, transform, false);
			sect.transform.SetAsFirstSibling();
			Image body = sect.GetComponentsInChildren<Image>()[1];
			sect.color = sector.backgroundColor;
			body.color = sect.color + Color.grey;
			sect.rectTransform.anchoredPosition = new Vector2(sector.bounds.x, sector.bounds.y);
			body.rectTransform.sizeDelta = sect.rectTransform.sizeDelta = new Vector2(sector.bounds.w, sector.bounds.h);
		}
	}

	void Update() {
		GetComponent<RectTransform>().anchoredPosition = -player.transform.position;
	}
	void OnDisable() {
		for(int i = 0; i < transform.childCount; i++) {
			Destroy(transform.GetChild(i).gameObject); // destroy stray children
		}
	}
}
