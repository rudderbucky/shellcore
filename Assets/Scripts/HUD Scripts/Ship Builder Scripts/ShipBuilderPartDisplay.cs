using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShipBuilderPartDisplay : MonoBehaviour {

	public IBuilderInterface builder;
	public GameObject emptyInfoMarker;
	public ShipBuilderCursorScript cursorScript;
	public Text partName;
	public Text partStats;
	public Image image;
	public Image abilityImage;
	public Image abilityTier;
	public Text abilityText;
	public RectTransform builderBG;
	bool initialized = false;

	public AbilityButtonScript buttonScript;

	public Image abilityBox;

	public void Initialize(IBuilderInterface inter) {
		builder = inter;
		initialized = true;
		image.type = Image.Type.Sliced;
		abilityImage.type = Image.Type.Sliced;
		abilityTier.type = Image.Type.Sliced;
		SetInactive();
	}

	void SetInactive() {
		emptyInfoMarker.SetActive(true);
		abilityBox.gameObject.SetActive(false);
		abilityImage.gameObject.SetActive(false);
		abilityText.gameObject.SetActive(false);
		image.gameObject.SetActive(false);
		partName.gameObject.SetActive(false);
		partStats.gameObject.SetActive(false);
		abilityTier.gameObject.SetActive(false);
	}
	// Update is called once per frame
	void Update () {

		// Gets the part to diplsay using the method GetButtonPartCursorIsOn() in IBuilderInterface
		
		if(initialized) {
			EntityBlueprint.PartInfo? part = null;
			if(cursorScript.GetPartCursorIsOn() != null) {
				part = cursorScript.GetPartCursorIsOn();
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
					abilityImage.sprite = AbilityUtilities.GetAbilityImageByID(info.abilityID, info.secondaryData);
					abilityImage.gameObject.SetActive(true);
					abilityText.text = AbilityUtilities.GetAbilityNameByID(info.abilityID, info.secondaryData) + (info.tier > 0 ? " " + info.tier : "");
					abilityText.gameObject.SetActive(true);
					abilityBox.gameObject.SetActive(true);

					string description = "";

					description += AbilityUtilities.GetAbilityNameByID(info.abilityID, info.secondaryData) + (info.tier > 0 ? " " + info.tier : "") + "\n";
					description += AbilityUtilities.GetDescriptionByID(info.abilityID, info.tier, info.secondaryData);
					buttonScript.abilityInfo = description;
				} else {
					abilityTier.gameObject.SetActive(false);
					abilityBox.gameObject.SetActive(false);
					abilityImage.gameObject.SetActive(false);
					abilityText.gameObject.SetActive(false);
				}
				emptyInfoMarker.SetActive(false);
				image.gameObject.SetActive(true);
				partName.gameObject.SetActive(true);
				partStats.gameObject.SetActive(true);
				string partID = info.partID;
				partName.text = partID;
				var blueprint = ResourceManager.GetAsset<PartBlueprint>(partID);
				float mass = blueprint.mass;
				float health = blueprint.health;
				int value = EntityBlueprint.GetPartValue(info);
				partStats.text = "PART SHELL: " + health / 2 + "\nPART CORE: " + health / 4 + "\nPART MASS: " + mass 
					+ "\nPART VALUE: \n" + value + " CREDITS";
				image.sprite = ResourceManager.GetAsset<Sprite>(partID + "_sprite");
				image.rectTransform.sizeDelta = image.sprite.bounds.size * 50;
				image.color = FactionColors.colors[0];
			} else {
				SetInactive();
			}
		}
	}
}
