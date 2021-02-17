using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.

public class AbilityButtonScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool clicked;
    public GameObject tooltipPrefab;
    public string abilityInfo;
    private GameObject tooltip;
    public Image abilityTier;
    public Image abilityImage;
    public Image gleam;
    public Image cooldown;
    public Image image;
    public Text hotkeyText;
    Ability ability;
    public Entity entity;
    bool gleaming;
    bool gleamed;

    public void Init(Ability ability, string hotkeyText, Entity entity)
    {
        this.entity = entity;
        this.ability = ability;
        // set up tooltip
        GetComponentInChildren<Text>().text = AbilityUtilities.GetAbilityName(ability)
             + (ability.GetTier() > 0 ? " " + ability.GetTier() : "");

        string description = "";
        description += AbilityUtilities.GetAbilityName(ability) + (ability.GetTier() > 0 ? " " + ability.GetTier() : "") + "\n";
        if(ability.GetEnergyCost() > 0)
            description += "Energy cost: " + ability.GetEnergyCost() + "\n";
        if (ability.GetCDDuration() != 0)
        {
            description += "Cooldown duration: " + ability.GetCDDuration() + "\n";
        }
        description += AbilityUtilities.GetDescription(ability);
        abilityInfo = description;

        // set up image
        abilityImage.sprite = AbilityUtilities.GetAbilityImage(ability);

        // set up hotkey display
        if(hotkeyText != null)
        {
            this.hotkeyText.transform.parent.gameObject.SetActive(true);
            this.hotkeyText.text = hotkeyText;
        }

        // set up ability tier;
        if(ability.GetTier() > 0 && ability.GetTier() <= 3)
        {
            abilityTier.enabled = true;
            abilityTier.sprite = ResourceManager.GetAsset<Sprite>("AbilityTier" + ability.GetTier());
            abilityTier.rectTransform.sizeDelta = abilityTier.sprite.bounds.size * 30;
            abilityTier.color = new Color(abilityTier.color.r, abilityTier.color.g, abilityTier.color.b, 0.4F);
        }

        gleamed = ability is PassiveAbility;
    }

    /// <summary>
    /// The gleam function that helps gleam the GUI indicator of the ability at the passed index
    /// </summary>
    /// <param name="index">The index of the ability to gleam</param>
    private void Gleam() {
        Color tmpColor = gleam.color; // grab the current color of the gleam
        tmpColor.a = Mathf.Max(0, tmpColor.a - 4 * Time.deltaTime); 
        // make it slightly more transparent (if the alpha goes below zero set it to zero)
        gleam.color = tmpColor; // set the color
        if (tmpColor.a == 0) gleaming = false; // if it is now transparent it is no longer gleaming
    }

    private void Update()
    {
        if(tooltip)
        {
            tooltip.transform.position = Input.mousePosition;
        }

        if(entity.GetHealth()[2] < ability.GetEnergyCost())
        {
            image.color = new Color(0, 0, 0.3F); // make the background dark blue
        }
        else image.color = ability.GetActiveTimeRemaining() != 0 ? Color.green : Color.white;
        cooldown.fillAmount = ability.GetCDRemaining() / ability.GetCDDuration();

        if(!entity.GetIsDead())
            ability.Tick(Input.GetKeyDown(hotkeyText.text) || clicked ? 1 : 0);
        clicked = false;

        // gleam (ability temporarily going white when cooldown refreshed) stuff
        if(gleaming)
        {
            Gleam();
        }

        if(ability.GetCDRemaining() != 0)
        {
            gleamed = false;
        }
        else if(!gleamed && !gleaming)
        {
            gleamed = true;
            gleaming = true;
            gleam.color = Color.white;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //create tooltip
        tooltip = Instantiate(tooltipPrefab);
        RectTransform rect = tooltip.GetComponent<RectTransform>();
        rect.position = eventData.position;
        rect.SetParent(transform.parent, true);
        rect.SetAsLastSibling();

        Text text = tooltip.transform.Find("Text").GetComponent<Text>();
        text.text = abilityInfo;

        rect.sizeDelta = new Vector2(text.preferredWidth + 16f, text.preferredHeight + 16);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //delete tooltip
        if (tooltip)
            Destroy(tooltip);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clicked = true;
    }

    public void OnDisable() {
        OnPointerExit(null);
    }
}
