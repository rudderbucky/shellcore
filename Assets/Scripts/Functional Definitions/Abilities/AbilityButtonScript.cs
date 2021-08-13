using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    [SerializeField]
    List<Ability> abilities = new List<Ability>();

    public Entity entity;
    public Image movingImage;
    bool gleaming;
    bool gleamed;
    bool dragging;
    Vector3 oldInputMousePos;
    KeyName keycode;
    public bool visualMode = false;
    GameObject rangeCirclePrefab;
    Dictionary<Ability, CircleGraphic> circles = new Dictionary<Ability, CircleGraphic>();

    string GetPrettyStringFromKeycode(KeyCode code)
    {
        var str = code.ToString();

        if (str.Length >= 5 && str.Substring(0, 5) == "Alpha")
        {
            str = str.Remove(0, 5);
        }

        return str;
    }

    public void Init(Ability ability, string hotkeyText, Entity entity, KeyName keycode, bool visualMode = false)
    {
        this.entity = entity;
        abilities.Add(ability);
        this.visualMode = visualMode;
        // set up name, description, tier
        ReflectName(ability);
        ReflectDescription(ability);
        ReflectTier(ability);
        ReflectHotkey(keycode);

        rangeCirclePrefab = ResourceManager.GetAsset<GameObject>("range_circle_prefab");
        this.keycode = keycode;

        // set up image
        abilityImage.sprite = AbilityUtilities.GetAbilityImage(ability);

        gleamed = ability is PassiveAbility;
    }

    public void ReflectHotkey(KeyName keycode)
    {
        this.keycode = keycode;
        // set up hotkey display
        if (hotkeyText != null)
        {
            this.hotkeyText.transform.parent.gameObject.SetActive(true);
            this.hotkeyText.text = GetPrettyStringFromKeycode(InputManager.keys[keycode].overrideKey);
        }
        else
        {
            this.hotkeyText.transform.parent.gameObject.SetActive(false);
        }
    }

    void ReflectDescription(Ability ability)
    {
        string description = AbilityUtilities.GetAbilityName(ability) + (ability.GetTier() > 0 ? " " + ability.GetTier() : "");
        if (ability.GetEnergyCost() > 0)
        {
            description += $"\nEnergy cost: {ability.GetEnergyCost()}";
        }

        if (ability.GetCDDuration() != 0)
        {
            description += $"\nCooldown duration: {ability.GetCDDuration()}";
        }

        if (ability.GetRange() > 0)
        {
            description += $"\nRange: {ability.GetRange()}";
        }

        if (ability is WeaponAbility weaponAbility)
        {
            var bonusDamageType = weaponAbility.GetBonusDamageType();
            if (bonusDamageType != null)
            {
                description += $"\nDeals bonus damage to: {bonusDamageType}";
            }
        }

        description += $"\n{AbilityUtilities.GetDescription(ability)}";
        abilityInfo = description;

        if (tooltip)
        {
            tooltip.transform.Find("Text").GetComponent<Text>().text = abilityInfo;
        }
    }

    void ReflectTier(Ability ability)
    {
        if (ability.GetTier() > 0 && ability.GetTier() <= 5)
        {
            abilityTier.enabled = true;
            abilityTier.sprite = ResourceManager.GetAsset<Sprite>("AbilityTier" + ability.GetTier());
            abilityTier.rectTransform.sizeDelta = abilityTier.sprite.bounds.size * 30;
            abilityTier.color = new Color(abilityTier.color.r, abilityTier.color.g, abilityTier.color.b, 0.4F);
        }
        else
        {
            abilityTier.enabled = false;
        }
    }

    /// <summary>
    /// The gleam function that helps gleam the GUI indicator of the ability at the passed index
    /// </summary>
    /// <param name="index">The index of the ability to gleam</param>
    private void Gleam()
    {
        Color tmpColor = gleam.color; // grab the current color of the gleam
        tmpColor.a = Mathf.Max(0, tmpColor.a - 4 * Time.deltaTime);
        // make it slightly more transparent (if the alpha goes below zero set it to zero)
        gleam.color = tmpColor; // set the color
        if (tmpColor.a == 0)
        {
            gleaming = false; // if it is now transparent it is no longer gleaming
        }
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
        if (dragging)
        {
            var xPos = Input.mousePosition.x;
            AbilityHandler.RearrangeID(xPos, (AbilityID)abilities[0].GetID(), (AbilityID)abilities[0].GetID() == AbilityID.SpawnDrone ? (abilities[0] as SpawnDrone).spawnData.drone : null);
        }

        // update the number of off-CD abilities
        if (offCDCountText)
        {
            offCDCountText.text = abilities.FindAll(a => a && !a.IsDestroyed() &&
                                                         (a.TimeUntilReady() == 0 || a.GetAbilityType() == AbilityHandler.AbilityTypes.Passive)).Count + "";
        }

        if (tooltip)
        {
            tooltip.transform.position = Input.mousePosition;
        }

        if (!entity || visualMode)
        {
            return;
        }

        // there's no point in running Update if there is no ability
        if (!abilities.Exists(ab => ab && !ab.IsDestroyed()) || entity.GetIsDead())
        {
            if (offCDCountText)
            {
                offCDCountText.text = "";
            }

            if (image)
            {
                image.color = new Color(.1f, .1f, .1f); // make the background dark
            }

            if (gleam)
            {
                Destroy(gleam.gameObject); // remove other sprites; destroying makes them irrelevant
            }

            if (cooldown)
            {
                Destroy(cooldown.gameObject);
            }

            return;
        }

        abilities.Sort((a, b) =>
        {
            if (!a.IsDestroyed() && b.IsDestroyed())
            {
                return -1;
            }
            else if (a.IsDestroyed() && !b.IsDestroyed())
            {
                return 1;
            }

            if (a.TimeUntilReady() > b.TimeUntilReady())
            {
                return 1;
            }
            else if (a.TimeUntilReady() < b.TimeUntilReady())
            {
                return -1;
            }
            else if (a.GetTier() > b.GetTier())
            {
                return -1;
            }
            else if (a.GetTier() < b.GetTier())
            {
                return 1;
            }
            else
            {
                return 0;
            }
        });

        ReflectDescription(abilities[0]);
        ReflectTier(abilities[0]);
        ReflectName(abilities[0]);

        if (entity.GetHealth()[2] < abilities[0].GetEnergyCost())
        {
            image.color = new Color(0, 0, 0.3F); // make the background dark blue
        }
        else if (abilities[0].GetAbilityType() != AbilityHandler.AbilityTypes.Passive && (abilities[0].State == Ability.AbilityState.Active ||
                                                                                          abilities[0].State == Ability.AbilityState.Charging ||
                                                                                          (abilities[0] is WeaponAbility && abilities[0].State == Ability.AbilityState.Ready)))
        {
            image.color = PlayerCore.GetPlayerFactionColor();
        }
        else
        {
            image.color = Color.white;
        }

        cooldown.fillAmount = abilities[0].TimeUntilReady() / abilities[0].GetCDDuration();

        if (!entity.GetIsDead())
        {
            bool hotkeyAccepted = (InputManager.GetKeyDown(keycode) && !InputManager.GetKey(KeyName.TurretQuickPurchase))
                                  && !PlayerViewScript.paused && !DialogueSystem.isInCutscene;
            if (abilities[0] is WeaponAbility)
            {
                foreach (var ab in abilities)
                {
                    if (hotkeyAccepted || (clicked && Input.mousePosition == oldInputMousePos))
                    {
                        ab.Activate();
                    }
                }
            }
            else if (hotkeyAccepted || (clicked && Input.mousePosition == oldInputMousePos))
            {
                abilities[0].Activate();
            }
        }

        clicked = false;

        // gleam (ability temporarily going white when cooldown refreshed) stuff
        if (gleaming)
        {
            Gleam();
        }

        if (abilities[0].TimeUntilReady() != 0)
        {
            gleamed = false;
        }
        else if (!gleamed && !gleaming)
        {
            gleamed = true;
            gleaming = true;
            gleam.color = Color.white;
        }
    }

    void LateUpdate()
    {
        if (tooltip)
        {
            PollRangeCircle();
        }
    }

    void PollRangeCircle()
    {
        foreach (var ability in abilities)
        {
            if (ability.GetRange() > 0 && circles.ContainsKey(ability))
            {
                if (ability.IsDestroyed())
                {
                    circles[ability].enabled = false;
                    continue;
                }
                else
                {
                    circles[ability].enabled = true;
                }

                circles[ability].color = ability.TimeUntilReady() > 0 ? Color.gray : PlayerCore.GetPlayerFactionColor();
                var range = ability.GetRange();
                var cameraPos = CameraScript.instance.transform.position;
                cameraPos.z = 0;
                range = Camera.main.WorldToScreenPoint(cameraPos + new Vector3(0, range)).y - Camera.main.WorldToScreenPoint(cameraPos).y;
                range *= 2;

                circles[ability].rectTransform.anchoredPosition = Camera.main.WorldToScreenPoint(ability.transform.position);
                circles[ability].rectTransform.sizeDelta = new Vector2(range, range);
                //Debug.Log(Camera.main.ScreenToWorldPoint((Vector3)rangeCircle.rectTransform.anchoredPosition +
                //    new Vector3(0,range / 2,CameraScript.zLevel) ) - abilities[0].transform.position);
            }
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
        ClearCircles();
        if (abilities.Count > 0 && abilities[0].GetRange() > 0)
        {
            foreach (var ability in abilities)
            {
                if (!ability.IsDestroyed())
                {
                    circles.Add(ability, Instantiate(rangeCirclePrefab, transform.parent.parent.Find("Circle Holder")).GetComponent<CircleGraphic>());
                    circles[ability].color = PlayerCore.GetPlayerFactionColor();
                }
            }
        }

        //rangeCircle.enabled = true;
        PollRangeCircle();
        Text text = tooltip.transform.Find("Text").GetComponent<Text>();
        text.text = abilityInfo;

        rect.sizeDelta = new Vector2(text.preferredWidth + 16f, text.preferredHeight + 16);
    }

    private void ClearCircles()
    {
        foreach (var value in circles.Values)
        {
            Destroy(value.gameObject);
        }

        circles.Clear();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //delete tooltip
        if (tooltip)
        {
            Destroy(tooltip);
        }

        ClearCircles();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clicked = true;
    }

    public void OnDisable()
    {
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
        if (movingImage)
        {
            movingImage.enabled = true;
        }

        oldInputMousePos = Input.mousePosition;
    }
}
