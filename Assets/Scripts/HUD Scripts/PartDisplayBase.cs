using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartDisplayBase : MonoBehaviour
{
    public Text partName;
	public Text partStats;
	public Image image;
	public Image abilityImage;
	public Image abilityTier;
	public Text abilityText;
	public AbilityButtonScript buttonScript;
	public Image abilityBox;
    public void DisplayPartInfo(EntityBlueprint.PartInfo info)
    {
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
        image.color = info.shiny ? FactionManager.GetFactionShinyColor(0) : FactionManager.GetFactionColor(0);
    }
}
