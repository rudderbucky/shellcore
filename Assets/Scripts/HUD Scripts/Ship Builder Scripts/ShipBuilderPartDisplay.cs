using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShipBuilderPartDisplay : MonoBehaviour {

	public ShipBuilder builder;
	public ShipBuilderCursorScript cursorScript;
	public Text partName;
	public Text partStats;
	Image image;
	public Image abilityImage;
	public Image abilityTier;
	public Text abilityText;

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
		else if(builder.GetButtonPartCursorIsOn() != null) {
			part = builder.GetButtonPartCursorIsOn();
		}
		else if(cursorScript.GetLastInfo() != null) {
			part = cursorScript.GetLastInfo();
		}

		if(part != null) {
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
			float mass = ResourceManager.GetAsset<PartBlueprint>(partID).mass;
			float health = ResourceManager.GetAsset<PartBlueprint>(partID).health;
			partStats.text = "Part Shell: " + health / 2 + "\nPart Core: " + health / 4 + "\nPart Mass: " + mass;
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
			abilityTier.gameObject.SetActive(false);
		}
	}
}
