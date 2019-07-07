using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShipBuilderPartDisplay : MonoBehaviour {

	public ShipBuilder builder;
	public GameObject emptyInfoMarker;
	public ShipBuilderCursorScript cursorScript;
	public Text partName;
	public Text partStats;
	Image image;
	public Image abilityImage;
	public Image abilityTier;
	public Text abilityText;
	public RectTransform builderBG;

	public AbilityButtonScript buttonScript;

	public Image abilityBox;
	void Start() {
		image = GetComponentInChildren<Image>();
	}
	// Update is called once per frame
	void Update () {
		EntityBlueprint.PartInfo? part = null;
		if(cursorScript.GetCurrentInfo() != null) {
			part = cursorScript.GetCurrentInfo();
		}
		else if(cursorScript.GetPartCursorIsOn() != null) {
			part = cursorScript.GetPartCursorIsOn();
		}
		else if(RectTransformUtility.RectangleContainsScreenPoint(builderBG, Input.mousePosition) &&
			!RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, Input.mousePosition) 
			&& builder.GetButtonPartCursorIsOn() != null) {
			part = builder.GetButtonPartCursorIsOn();
		}
		else if(cursorScript.GetLastInfo() != null) {
			part = cursorScript.GetLastInfo();
		}

		if(part != null) {
			emptyInfoMarker.SetActive(false);
			EntityBlueprint.PartInfo info = (EntityBlueprint.PartInfo)part;
			if(info.abilityID != 0) {
				if(info.tier != 0) {
					abilityTier.gameObject.SetActive(true);
					abilityTier.sprite = ResourceManager.GetAsset<Sprite>("AbilityTier" + info.tier);
					abilityTier.rectTransform.sizeDelta = abilityTier.sprite.bounds.size * 20;
					abilityTier.color = new Color(1,1,1,0.4F);
				} else abilityTier.gameObject.SetActive(false);
				abilityImage.sprite = AbilityUtilities.GetAbilityImageByID(info.abilityID);
				abilityImage.gameObject.SetActive(true);
				abilityText.text = AbilityUtilities.GetAbilityNameByID(info.abilityID) + (info.tier > 0 ? " " + info.tier : "");
				abilityText.gameObject.SetActive(true);
				abilityBox.gameObject.SetActive(true);

				string description = "";

				description += AbilityUtilities.GetAbilityNameByID(info.abilityID) + (info.tier > 0 ? " " + info.tier : "") + "\n";
				description += AbilityUtilities.GetDescriptionByID(info.abilityID, info.tier);
				buttonScript.abilityInfo = description;
			} else {
				abilityTier.gameObject.SetActive(false);
				abilityBox.gameObject.SetActive(false);
				abilityImage.gameObject.SetActive(false);
				abilityText.gameObject.SetActive(false);
			}
			image.gameObject.SetActive(true);
			partName.gameObject.SetActive(true);
			partStats.gameObject.SetActive(true);
			string partID = info.partID;
			partName.text = partID;
			var blueprint = ResourceManager.GetAsset<PartBlueprint>(partID);
			float mass = blueprint.mass;
			float health = blueprint.health;
			int value = EntityBlueprint.GetPartValue(info);
			partStats.text = "Part Shell: " + health / 2 + "\nPart Core: " + health / 4 + "\nPart Mass: " + mass 
				+ "\nPart Value: \n" + value + " Credits";
			image.sprite = ResourceManager.GetAsset<Sprite>(partID + "_sprite");
			image.rectTransform.sizeDelta = image.sprite.bounds.size * 50;
			image.color = FactionColors.colors[0];
		} else {
			emptyInfoMarker.SetActive(true);
			abilityBox.gameObject.SetActive(false);
			abilityImage.gameObject.SetActive(false);
			abilityText.gameObject.SetActive(false);
			image.gameObject.SetActive(false);
			partName.gameObject.SetActive(false);
			partStats.gameObject.SetActive(false);
			abilityTier.gameObject.SetActive(false);
		}
	}
}
