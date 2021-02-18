using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.

public class AbilityButtonScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
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
    public Text offCDCountText;
    List<Ability> abilities = new List<Ability>();
    public Entity entity;
    public Image movingImage;
    bool gleaming;
    bool gleamed;
    bool dragging;
    Vector3 oldInputMousePos;

    public void Init(Ability ability, string hotkeyText, Entity entity)
    {
        this.entity = entity;
        abilities.Add(ability);

        // set up name, description, tier
        ReflectName(ability);
        ReflectDescription(ability);
        ReflectTier(ability);
        ReflectHotkey(hotkeyText);

        // set up image
        abilityImage.sprite = AbilityUtilities.GetAbilityImage(ability);

        gleamed = ability is PassiveAbility;
    }

    public void ReflectHotkey(string hotkeyText)
    {
        // set up hotkey display
        if(hotkeyText != null)
        {
            this.hotkeyText.transform.parent.gameObject.SetActive(true);
            this.hotkeyText.text = hotkeyText;
        } 
        else this.hotkeyText.transform.parent.gameObject.SetActive(false);
    }

    void ReflectDescription(Ability ability)
    {
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
        if(tooltip)
        {
            tooltip.transform.Find("Text").GetComponent<Text>().text = abilityInfo;
        }
    }

    void ReflectTier(Ability ability)
    {
        if(ability.GetTier() > 0 && ability.GetTier() <= 3)
        {
            abilityTier.enabled = true;
            abilityTier.sprite = ResourceManager.GetAsset<Sprite>("AbilityTier" + ability.GetTier());
            abilityTier.rectTransform.sizeDelta = abilityTier.sprite.bounds.size * 30;
            abilityTier.color = new Color(abilityTier.color.r, abilityTier.color.g, abilityTier.color.b, 0.4F);
        } else abilityTier.enabled = false;
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

    public void ReflectName(Ability ability)
    {
        GetComponentInChildren<Text>().text = AbilityUtilities.GetAbilityName(ability)
             + (ability.GetTier() > 0 ? " " + ability.GetTier() : "");
    }

    public void AddAbility(Ability ability)
    {
        abilities.Add(ability);
    }

    private void Update()
    {
        if(dragging)
        {
            var xPos = Input.mousePosition.x;
            AbilityHandler.RearrangeID(xPos, (AbilityID)abilities[0].GetID());
        }

        // update the number of off-CD abilities
        if(offCDCountText)
            offCDCountText.text = abilities.FindAll(a => a && !a.IsDestroyed() && a.GetCDRemaining() == 0).Count + "";

        if(tooltip)
        {
            tooltip.transform.position = Input.mousePosition;
        }

        if(!entity || (entity as PlayerCore).GetIsInteracting()) return;

        // there's no point in running Update if there is no ability
        if (!abilities.Exists(ab => ab && !ab.IsDestroyed()) || entity.GetIsDead())
        {
            if(offCDCountText) offCDCountText.text = "";
            if(image) image.color = new Color(.1f, .1f, .1f); // make the background dark
            if(gleam) Destroy(gleam.gameObject); // remove other sprites; destroying makes them irrelevant
            if(cooldown) Destroy(cooldown.gameObject);
            return;
        }

        abilities.Sort((a, b) => {
            if(a.GetCDRemaining() > b.GetCDRemaining())
            {
                return 1;
            }
            else if(a.GetCDRemaining() < b.GetCDRemaining())
            {
                return -1;
            }
            else if(a.GetTier() > b.GetTier())
            {
                return -1;
            }
            else if(a.GetTier() < b.GetTier())
            {
                return 1;
            }
            else return 0;
        });

        ReflectDescription(abilities[0]);
        ReflectTier(abilities[0]);
        ReflectName(abilities[0]);

        if(entity.GetHealth()[2] < abilities[0].GetEnergyCost())
        {
            image.color = new Color(0, 0, 0.3F); // make the background dark blue
        }
        else image.color = abilities[0].GetActiveTimeRemaining() != 0 ? Color.green : Color.white;
        cooldown.fillAmount = abilities[0].GetCDRemaining() / abilities[0].GetCDDuration();

        if(!entity.GetIsDead())
        {
            if(abilities[0] is WeaponAbility)
            {
                foreach(var ab in abilities)
                {
                    ab.Tick(Input.GetKeyDown(hotkeyText.text) || (clicked && Input.mousePosition == oldInputMousePos) ? 1 : 0);
                }
            }
            else
            {
                abilities[0].Tick(Input.GetKeyDown(hotkeyText.text) || (clicked && Input.mousePosition == oldInputMousePos) ? 1 : 0);
                for(int i = 1; i < abilities.Count; i++)
                {
                    abilities[i].Tick(0);
                }
            }
        }
            
        clicked = false;

        // gleam (ability temporarily going white when cooldown refreshed) stuff
        if(gleaming)
        {
            Gleam();
        }

        if(abilities[0].GetCDRemaining() != 0)
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

    public void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
        movingImage.enabled = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
        movingImage.enabled = true;
        oldInputMousePos = Input.mousePosition;
    }
}
