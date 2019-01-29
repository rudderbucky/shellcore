using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShipBuilderPartDisplay : MonoBehaviour {

	public ShipBuilderCursorScript cursorScript;
	public Text partName;
	public Text partStats;
	Image image;
	public Image abilityImage;
	public Text AbilityText;
	void Start() {
		image = GetComponentInChildren<Image>();
	}
	// Update is called once per frame
	void Update () {
		if(cursorScript.lastPart) {
			if(cursorScript.lastPart.info.abilityType != Ability.AbilityType.None) {
				abilityImage.sprite = ResourceManager.GetAsset<Sprite>("AbilitySprite");
			}
			image.gameObject.SetActive(true);
			partName.gameObject.SetActive(true);
			partStats.gameObject.SetActive(true);
			string partID = cursorScript.lastPart.info.partID;
			partName.text = partID;
			float health = ResourceManager.GetAsset<PartBlueprint>(partID).health;
			partStats.text = "Part Shell: " + health / 2 + "\nPart Core: " + health / 4;
			image.sprite = ResourceManager.GetAsset<Sprite>(partID + "_sprite");
			image.rectTransform.sizeDelta = image.sprite.bounds.size * 50;
			image.color = FactionColors.colors[0];
		} else {
			image.gameObject.SetActive(false);
			partName.gameObject.SetActive(false);
			partStats.gameObject.SetActive(false);
		}
	}
}
