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
	public Text abilityText;

	public AbilityButtonScript buttonScript;

	public Image abilityBox;
	void Start() {
		image = GetComponentInChildren<Image>();
	}
	// Update is called once per frame
	void Update () {
		var part = cursorScript.lastPart;
		if(part) {
			if(part.info.abilityID != 0) {
				abilityImage.sprite = AbilityUtilities.GetAbilityImageByID(part.info.abilityID);
				abilityImage.gameObject.SetActive(true);
				abilityText.text = AbilityUtilities.GetAbilityNameByID(part.info.abilityID);
				abilityText.gameObject.SetActive(true);
				abilityBox.gameObject.SetActive(true);

				string description = "";

				description += AbilityUtilities.GetAbilityNameByID(part.info.abilityID) + "\n";
				description += AbilityUtilities.GetDescriptionByID(part.info.abilityID);
				buttonScript.abilityInfo = description;
			} else {
				abilityBox.gameObject.SetActive(false);
				abilityImage.gameObject.SetActive(false);
				abilityText.gameObject.SetActive(false);
			}
			image.gameObject.SetActive(true);
			partName.gameObject.SetActive(true);
			partStats.gameObject.SetActive(true);
			string partID = part.info.partID;
			partName.text = partID;
			float health = ResourceManager.GetAsset<PartBlueprint>(partID).health;
			partStats.text = "Part Shell: " + health / 2 + "\nPart Core: " + health / 4;
			image.sprite = ResourceManager.GetAsset<Sprite>(partID + "_sprite");
			image.rectTransform.sizeDelta = image.sprite.bounds.size * 50;
			image.color = FactionColors.colors[0];
		} else {
			abilityBox.gameObject.SetActive(false);
			abilityImage.gameObject.SetActive(false);
			abilityText.gameObject.SetActive(false);
			image.gameObject.SetActive(false);
			partName.gameObject.SetActive(false);
			partStats.gameObject.SetActive(false);
		}
	}
}
