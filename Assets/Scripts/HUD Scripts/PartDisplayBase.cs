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
    public GameObject emptyInfoMarker;
    
    public void SetInactive()
    {
        if (emptyInfoMarker) emptyInfoMarker.SetActive(true);
        abilityBox.gameObject.SetActive(false);
        abilityImage.gameObject.SetActive(false);
        abilityText.gameObject.SetActive(false);
        image.gameObject.SetActive(false);
        partName.gameObject.SetActive(false);
        partStats.gameObject.SetActive(false);
        abilityTier.gameObject.SetActive(false);
    }


    public void DisplayPartInfo(EntityBlueprint.PartInfo info)
    {
        if (emptyInfoMarker) emptyInfoMarker.SetActive(false);
        if (info.abilityID != 0)
        {
            if (info.tier != 0)
            {
                abilityTier.gameObject.SetActive(true);
                abilityTier.sprite = ResourceManager.GetAsset<Sprite>("AbilityTier" + info.tier);
                if (abilityTier.sprite)
                {
                    abilityTier.rectTransform.sizeDelta = abilityTier.sprite.bounds.size * 20;
                }

                abilityTier.color = new Color(1, 1, 1, 0.4F);
            }
            else
            {
                abilityTier.gameObject.SetActive(false);
            }

            abilityImage.sprite = AbilityUtilities.GetAbilityImageByID(info.abilityID, info.secondaryData);
            abilityImage.gameObject.SetActive(true);
            abilityText.text = AbilityUtilities.GetAbilityNameByID(info.abilityID, info.secondaryData) + (info.tier > 0 ? " " + info.tier : "");
            abilityText.gameObject.SetActive(true);
            abilityBox.gameObject.SetActive(true);

            string description = "";

            description += AbilityUtilities.GetAbilityNameByID(info.abilityID, info.secondaryData) + (info.tier > 0 ? " " + info.tier : "") + "\n";
            description += AbilityUtilities.GetDescriptionByID(info.abilityID, info.tier, info.secondaryData);
            buttonScript.abilityInfo = description;
        }
        else
        {
            abilityTier.gameObject.SetActive(false);
            abilityBox.gameObject.SetActive(false);
            abilityImage.gameObject.SetActive(false);
            abilityText.gameObject.SetActive(false);
        }

        image.gameObject.SetActive(true);
        image.rectTransform.localScale = new Vector3(0.5F, 0.5F, 1);
        partName.gameObject.SetActive(true);
        partStats.gameObject.SetActive(true);
        string partID = info.partID;
        if (info.playerGivenName == null || info.playerGivenName == "") 
        {
            partName.text = partID;
        }
        else
        {
            partName.text = info.playerGivenName + " (" + info.partID + ")";
        }
        var blueprint = ResourceManager.GetAsset<PartBlueprint>(partID);
        float mass = blueprint.mass;
        float health = blueprint.health;
        int value = EntityBlueprint.GetPartValue(info);
        partStats.text = $"PART SHELL: {health / 2}\nPART CORE: {health / 4}\nPART WEIGHT: {mass * Entity.weightMultiplier}\nPART VALUE: \n{value} CREDITS";
        image.sprite = ResourceManager.GetAsset<Sprite>(partID + "_sprite");
        if (image.sprite)
            image.rectTransform.sizeDelta = image.sprite.bounds.size * 100;
        image.color = info.shiny ? FactionManager.GetFactionShinyColor(0) : FactionManager.GetFactionColor(0);
    }
}
