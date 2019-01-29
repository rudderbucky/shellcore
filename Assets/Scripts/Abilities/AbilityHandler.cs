using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handler for abilities used by THE PLAYER on the GUI
/// </summary>
public class AbilityHandler : MonoBehaviour {

    public GameObject abilityBackground; // background of the ability
    public Image abilityCDIndicator; // used to indicate if the ability is on cooldown
    public Image abilityGleam; // gleam for the ability
    public GameObject tooltipPrefab; // Prefab for showing information when mouse hovers over the ability button
    public Image HUDbg;
    private bool initialized; // check for the update method
    private PlayerCore core; // the player
    private Ability[] abilities; // ability array of the core
    private Image image; // image prefab
    private Image[] abilityImagesArray; // images of the abilities displayed on the GUI
    private GameObject[] abilityBackgroundArray; // all ability backgrounds displayed on the GUI
    private Image[] abilityCDIndicatorArray; // all ability cooldown indicators displayed on the GUI
    private Image[] abilityGleamArray; // all ability gleams displayed on the GUI
    private bool[] gleaming; // array to check whether an ability is currently gleaming
    private bool[] gleamed; // array to check whether an ability has already gleamed in the cycle

    public enum AbilityTypes {
        Skills,
        Spawns,
        Weapons,
        Passive
    }
    
    public AbilityTypes currentVisibles;
    public List<Ability> visibleAbilities = new List<Ability>();

    public void SetCurrentVisible(AbilityTypes type) {
        if(currentVisibles != type) {
            currentVisibles = type;
            Deinitialize();
            Initialize(core);
        }
    }
    /// <summary>
    /// Initialization of the ability handler that is tied to the player
    /// </summary>
    public void Initialize(PlayerCore player) {
        core = player;
        abilities = core.GetAbilities(); // Get the core's ability array
        visibleAbilities.Clear();
        foreach (Ability ab in abilities) {
            switch(currentVisibles) {
                case AbilityTypes.Skills:
                    if(ab as Ability && !(ab as SpawnDrone) && !(ab as WeaponAbility) && !(ab as PassiveAbility))
                        visibleAbilities.Add(ab);
                    break;
                case AbilityTypes.Passive:
                    if(ab as PassiveAbility)
                        visibleAbilities.Add(ab);
                    break;
                case AbilityTypes.Spawns:
                    if(ab as SpawnDrone)
                        visibleAbilities.Add(ab);
                    break;
                case AbilityTypes.Weapons:
                    if(ab as WeaponAbility)
                        visibleAbilities.Add(ab);
                    break;
            }
        }
        abilityImagesArray = new Image[abilities.Length]; // initialize all the GUI arrays
        abilityBackgroundArray = new GameObject[abilities.Length];
        abilityCDIndicatorArray = new Image[abilities.Length];
        abilityGleamArray = new Image[abilities.Length];
        gleaming = new bool[abilities.Length]; // initialize the boolean arrays
        gleamed = new bool[abilities.Length];
        for(int i = 0; i < gleamed.Length; i++) {
            gleamed[i] = true;
        }
        float tileSpacing = abilityBackground.GetComponent<Image>().sprite.bounds.size.x * 15; // Used to space out the abilities on the GUI
        abilityCDIndicator.fillAmount = 0; // make the cooldown indicator's fill initially 0
        abilityGleam.color = Color.clear;  // start the gleam as clear
        
        if (!image)
        {
            image = new GameObject().AddComponent<Image>();
            image.transform.localScale = new Vector3(0.25F, 0.25F, 0.25F);
            image.rectTransform.anchorMax = Vector2.zero;
            image.rectTransform.anchorMin = Vector2.zero;
            image.gameObject.SetActive(false);
        }
        for (int i = 0; i < visibleAbilities.Count; i++)
        { // iterate through to display all the abilities
            if (visibleAbilities[i] == null) break;
            Vector3 pos = new Vector3(tileSpacing * (0.8F*i+0.5F), tileSpacing*0.8F, this.transform.position.z); // find where to position the images
            // position them all, do not keep the world position

            // instantiate background image
            abilityBackgroundArray[i] = Instantiate(abilityBackground, pos, Quaternion.identity) as GameObject;
            abilityBackgroundArray[i].transform.SetParent(transform, false); // set parent (do not keep world position)
            abilityBackgroundArray[i].GetComponentInChildren<Text>().text = AbilityUtilities.GetAbilityNameByID(visibleAbilities[i].GetID());
            var button = abilityBackgroundArray[i].GetComponent<AbilityButtonScript>();
            button.tooltipPrefab = tooltipPrefab;
            string description = "";
            description += AbilityUtilities.GetAbilityNameByID(visibleAbilities[i].GetID()) + "\n";
            if(visibleAbilities[i].GetEnergyCost() > 0)
                description += "Energy cost: " + visibleAbilities[i].GetEnergyCost() + "\n";
            if (visibleAbilities[i].GetCDDuration() != 0)
            {
                description += "Cooldown duration: " + visibleAbilities[i].GetCDDuration() + "\n";
            }
            description += AbilityUtilities.GetDescriptionByID(visibleAbilities[i].GetID());
            button.abilityInfo = description;

            image.sprite = ResourceManager.GetAsset<Sprite>("AbilitySprite" + visibleAbilities[i].GetID());
            abilityImagesArray[i] = Instantiate(image, pos, Quaternion.identity) as Image;
            abilityImagesArray[i].gameObject.SetActive(true);
            var canvasg = abilityImagesArray[i].gameObject.AddComponent<CanvasGroup>(); // this is done for every image, it allows the buttons to be clicked
            canvasg.blocksRaycasts = false;
            canvasg.interactable = false;

            // instantiate ability image
            abilityImagesArray[i].transform.SetParent(transform, false);
            // set parent (do not keep world position)

            abilityCDIndicatorArray[i] = Instantiate(abilityCDIndicator, pos, Quaternion.identity) as Image;
            canvasg = abilityCDIndicatorArray[i].gameObject.AddComponent<CanvasGroup>();
            canvasg.blocksRaycasts = false;
            canvasg.interactable = false;
            // instantiate cooldown indicator
            abilityCDIndicatorArray[i].transform.SetParent(transform, false);
            // set parent (do not keep world position)

            abilityGleamArray[i] = Instantiate(abilityGleam, pos, Quaternion.identity) as Image;
            canvasg = abilityGleamArray[i].gameObject.AddComponent<CanvasGroup>();
            canvasg.blocksRaycasts = false;
            canvasg.interactable = false;
            // instantiate gleam image
            abilityGleamArray[i].transform.SetParent(transform, false);
            // set parent (do not keep world position)
        }

        if(visibleAbilities.Count > 0)
        {
            var y = HUDbg.GetComponent<RectTransform>().anchoredPosition;
            y.x = abilityGleamArray[0].rectTransform.anchoredPosition.x - 0.5F * tileSpacing;
            HUDbg.GetComponent<RectTransform>().anchoredPosition = y;

            var x = HUDbg.GetComponent<RectTransform>().sizeDelta;
            x.x = abilityGleamArray[visibleAbilities.Count - 1].rectTransform.anchoredPosition.x + 0.5F * tileSpacing - y.x;
            HUDbg.GetComponent<RectTransform>().sizeDelta = x;
        } else HUDbg.GetComponent<RectTransform>().sizeDelta = new Vector2(0, HUDbg.GetComponent<RectTransform>().sizeDelta.y);

        if (image) Destroy(image.gameObject);
        initialized = true; // handler completely initialized, safe to update now
    }

    /// <summary>
    /// Deinitializes the Ability Handler UI
    /// </summary>
    public void Deinitialize()
    {
        for(int i = 5; i < transform.childCount; i++)  //Start from 5, because index 1 is the background
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        initialized = false; // reset initialized
    }
    /// <summary>
    /// Used to update the ability and its representation to account 
    /// for whether or not the ability is active, on cooldown, or whether the core has sufficient energy
    /// </summary>
    /// <param name="index">index of the ability to update</param>
    private void AbilityUpdate(int index, bool clicked)
    {
        // TODO: Fix the problem regarding a bug with having more than 10 abilities, since after 9 the numbers are not valid inputs
        // this system relies on a conversion of index into a string to find a hotkey to activate
        // temporary workaround in place in line below, but ability type segmenting must be done eventually 



        if (!visibleAbilities[index] || visibleAbilities[index].IsDestroyed())
        {
            abilityBackgroundArray[index].GetComponent<Image>().color = new Color(.1f, .1f, .1f); // make the background dark
            if(abilityGleamArray[index]) Destroy(abilityGleamArray[index].gameObject); // remove other sprites; destroying makes them irrelevant
            if(abilityCDIndicatorArray[index]) Destroy(abilityCDIndicatorArray[index].gameObject);
            return;
        }
        if (clicked)
        {
            visibleAbilities[index].Tick("activate");
        } else visibleAbilities[index].Tick((index+1) < 10 ? (index + 1).ToString() : ""); // Tick the ability

        if (abilityGleamArray[index])
        {
            if (gleaming[index])
            {
                Gleam(index); // continue gleam if already gleaming
            }
            if (visibleAbilities[index].GetCDRemaining() != 0) // on cooldown
            {
                gleamed[index] = false; // no longer gleamed
                abilityCDIndicatorArray[index].fillAmount = visibleAbilities[index].GetCDRemaining() / visibleAbilities[index].GetCDDuration();
                // adjust the cooldown indicator
            }
            else if (!gleaming[index] && !gleamed[index])
            { // ability is off cooldown, check if it should gleam
              //abilityGleamArray[index].fillAmount = 1;
                abilityGleamArray[index].color = Color.white;
                gleaming[index] = true; // start gleaming
                gleamed[index] = true; // has already gleamed once now
            }

            if (core.GetHealth()[2] < visibleAbilities[index].GetEnergyCost()) // insufficient energy
            {
                abilityBackgroundArray[index].GetComponent<Image>().color = new Color(0, 0, 0.3F); // make the background dark blue
            }
            else if (visibleAbilities[index].GetActiveTimeRemaining() != 0) // active
            {
                abilityBackgroundArray[index].GetComponent<Image>().color = Color.green; // make the background green
            }
            else if (abilityBackgroundArray[index].GetComponent<Image>().color != Color.white) // ability ready
            {
                abilityBackgroundArray[index].GetComponent<Image>().color = Color.white; // reset color to white
            }
        }
    }

    /// <summary>
    /// The gleam function that helps gleam the GUI indicator of the ability at the passed index
    /// </summary>
    /// <param name="index">The index of the ability to gleam</param>
    private void Gleam(int index) {
        Color tmpColor = abilityGleamArray[index].color; // grab the current color of the gleam
        tmpColor.a = Mathf.Max(0, tmpColor.a - 4 * Time.deltaTime); 
        // make it slightly more transparent (if the alpha goes below zero set it to zero)
        abilityGleamArray[index].color = tmpColor; // set the color
        if (tmpColor.a == 0) gleaming[index] = false; // if it is now transparent it is no longer gleaming
    }

    // Update is called once per frame
    private void Update () {
        if (initialized) // check if safe to update
        {
            for (int i = 0; i < core.GetAbilities().Length; i++)
            { // update all abilities
                if (abilities[i] == null || core.GetIsDead()) continue; 
                // skip ability instead of break because further abilities may not be destroyed
                if(visibleAbilities.Contains(abilities[i])) {
                    int ind = visibleAbilities.IndexOf(abilities[i]);
                    var button = abilityBackgroundArray[ind].GetComponent<AbilityButtonScript>();
                    AbilityUpdate(ind, button.clicked); // otherwise update the current update
                    button.clicked = false;
                } else abilities[i].Tick("");
            }
            if(core.GetIsDead()) return;
            if(Input.GetKeyDown(KeyCode.E)) {
                SetCurrentVisible((AbilityTypes)(Mathf.Min((int)currentVisibles + 1, 3)));
            }
            if(Input.GetKeyDown(KeyCode.Q)) {
                SetCurrentVisible((AbilityTypes)(Mathf.Max((int)currentVisibles - 1, 0)));
            }
        }
	}
}
