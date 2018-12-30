using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handler for abilities used by THE PLAYER on the GUI
/// </summary>
public class AbilityHandler : MonoBehaviour {

    private bool initialized; // check for the update method
    private PlayerCore core; // the player
    public GameObject abilityBackground; // background of the ability
    public UnityEngine.UI.Image abilityCDIndicator; // used to indicate if the ability is on cooldown
    public UnityEngine.UI.Image abilityGleam; // gleam for the ability
    private Ability[] abilities; // ability array of the core
    public Sprite[] abilitySpritesArray; // sprite array for ability images
    private UnityEngine.UI.Image image; // image prefab
    private UnityEngine.UI.Image[] abilityImagesArray; // images of the abilities displayed on the GUI
    private GameObject[] abilityBackgroundArray; // all ability backgrounds displayed on the GUI
    private UnityEngine.UI.Image[] abilityCDIndicatorArray; // all ability cooldown indicators displayed on the GUI
    private UnityEngine.UI.Image[] abilityGleamArray; // all ability gleams displayed on the GUI
    private bool[] gleaming; // array to check whether an ability is currently gleaming
    private bool[] gleamed; // array to check whether an ability has already gleamed in the cycle

    /// <summary>
    /// Initialization of the ability handler that is tied to the player
    /// </summary>
    public void Initialize(PlayerCore player) {
        core = player;
        abilities = core.GetAbilities(); // Get the core's ability array
        abilityImagesArray = new UnityEngine.UI.Image[abilities.Length]; // initialize all the GUI arrays
        abilityBackgroundArray = new GameObject[abilities.Length];
        abilityCDIndicatorArray = new UnityEngine.UI.Image[abilities.Length];
        abilityGleamArray = new UnityEngine.UI.Image[abilities.Length];
        gleaming = new bool[abilities.Length]; // initialize the boolean arrays
        gleamed = new bool[abilities.Length];
        float tileSpacing = abilityBackground.GetComponent<UnityEngine.UI.Image>().sprite.bounds.size.x * 15; // Used to space out the abilities on the GUI
        abilityCDIndicator.fillAmount = 0; // make the cooldown indicator's fill initially 0
        abilityGleam.color = Color.clear;  // start the gleam as clear
        
        if (!image)
        {
            image = new GameObject().AddComponent<UnityEngine.UI.Image>();
            image.transform.localScale = new Vector3(0.25F, 0.25F, 0.25F);
            image.gameObject.SetActive(false);
        }
        for (int i = 0; i < abilities.Length; i++)
        { // iterate through to display all the abilities
            if (abilities[i] == null) break;
            Vector3 pos = new Vector3(tileSpacing * i, this.transform.position.y, this.transform.position.z); // find where to position the images
            // position them all, do not keep the world position
            // instantiate background image
            abilityBackgroundArray[i] = Instantiate(abilityBackground, pos, Quaternion.identity) as GameObject;
            abilityBackgroundArray[i].transform.SetParent(transform, false);
            // set parent (do not keep world position)

            image.sprite = abilitySpritesArray[abilities[i].GetID()];
            abilityImagesArray[i] = Instantiate(image, pos, Quaternion.identity) as UnityEngine.UI.Image;
            abilityImagesArray[i].gameObject.SetActive(true);
            var canvasg = abilityImagesArray[i].gameObject.AddComponent<CanvasGroup>(); // this is done for every image, it allows the buttons to be clicked
            canvasg.blocksRaycasts = false;
            canvasg.interactable = false;

            // instantiate ability image
            abilityImagesArray[i].transform.SetParent(transform, false);
            // set parent (do not keep world position)

            abilityCDIndicatorArray[i] = Instantiate(abilityCDIndicator, pos, Quaternion.identity) as UnityEngine.UI.Image;
            canvasg = abilityCDIndicatorArray[i].gameObject.AddComponent<CanvasGroup>();
            canvasg.blocksRaycasts = false;
            canvasg.interactable = false;
            // instantiate cooldown indicator
            abilityCDIndicatorArray[i].transform.SetParent(transform, false);
            // set parent (do not keep world position)

            abilityGleamArray[i] = Instantiate(abilityGleam, pos, Quaternion.identity) as UnityEngine.UI.Image;
            canvasg = abilityGleamArray[i].gameObject.AddComponent<CanvasGroup>();
            canvasg.blocksRaycasts = false;
            canvasg.interactable = false;
            // instantiate gleam image
            abilityGleamArray[i].transform.SetParent(transform, false);
            // set parent (do not keep world position)
        }
        initialized = true; // handler completely initialized, safe to update now
    }

    /// <summary>
    /// Deinitializes the Ability Handler UI
    /// </summary>
    public void Deinitialize()
    {
        for (int i = 0; i < abilities.Length; i++)
        { // iterate through array
            // destroy the associated bars
            Destroy(abilityBackgroundArray[i].gameObject);
            Destroy(abilityImagesArray[i].gameObject);
            Destroy(abilityCDIndicatorArray[i].gameObject);
            Destroy(abilityGleamArray[i].gameObject);
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
        if (clicked)
        {
            abilities[index].Tick("activate");
        } else abilities[index].Tick((index+1) < 10 ? (index + 1).ToString() : ""); // Tick the ability

        if (gleaming[index]) {
            Gleam(index); // continue gleam if already gleaming
        }
        if (abilities[index].GetCDRemaining() != 0) // on cooldown
        {
            gleamed[index] = false; // no longer gleamed
            abilityCDIndicatorArray[index].fillAmount = abilities[index].GetCDRemaining() / abilities[index].GetCDDuration();
            // adjust the cooldown indicator
        }
        else if (!gleaming[index] && !gleamed[index]) { // ability is off cooldown, check if it should gleam
            //abilityGleamArray[index].fillAmount = 1;
            abilityGleamArray[index].color = Color.white;
            gleaming[index] = true; // start gleaming
            gleamed[index] = true; // has already gleamed once now
        }
        if(abilities[index].IsDestroyed()) // Not available in the current "reduced" shell configuration
        {
            abilityBackgroundArray[index].GetComponent<UnityEngine.UI.Image>().color = new Color(.1f, .1f, .1f); // make the background dark
        }
        else if (abilities[index].GetActiveTimeRemaining() != 0) // active
        {
            abilityBackgroundArray[index].GetComponent<UnityEngine.UI.Image>().color = Color.green; // make the background green
        } 
        else if (core.GetHealth()[2] < abilities[index].GetEnergyCost()) // insufficient energy
        {
            abilityBackgroundArray[index].GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0.3F); // make the background dark blue
        }
        else if(abilityBackgroundArray[index].GetComponent<UnityEngine.UI.Image>().color != Color.white) // ability ready
        {
            abilityBackgroundArray[index].GetComponent<UnityEngine.UI.Image>().color = Color.white; // reset color to white
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
                if (abilities[i] == null || core.GetIsDead()) break; // stop iterating as every ability has been updated already or entity is dead
                var button = abilityBackgroundArray[i].GetComponent<AbilityButtonScript>();
                AbilityUpdate(i, button.clicked); // otherwise update the current update
                button.clicked = false;
            }
        }
	}
}
