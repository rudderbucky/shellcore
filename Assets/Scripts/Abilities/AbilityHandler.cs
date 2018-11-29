using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handler for abilities used by THE PLAYER on the GUI
/// </summary>
public class AbilityHandler : MonoBehaviour {

    private bool initialized;
    public PlayerCore core; // the player
    public UnityEngine.UI.Image abilityBackground; // background of the ability
    public UnityEngine.UI.Image abilityCDIndicator; // used to indicate if the ability is on cooldown
    public UnityEngine.UI.Image abilityGleam;
    private Ability[] abilities; // ability array of the core
    public UnityEngine.UI.Image[] abilityImagesInput; // images of every ability
    private UnityEngine.UI.Image[] abilityImagesArray; // images of the abilities displayed on the GUI
    private UnityEngine.UI.Image[] abilityBackgroundArray; // all ability backgrounds displayed on the GUI
    private UnityEngine.UI.Image[] abilityCDIndicatorArray; // all ability cooldown indicators displayed on the GUI
    private UnityEngine.UI.Image[] abilityGleamArray;
    private bool[] gleaming;
    private bool[] gleamed;

    //private float CDRemaining; //
    // Use this for initialization

    public void Initialize() {
        abilities = core.GetAbilities(); // Get the core's ability array
        abilityImagesArray = new UnityEngine.UI.Image[abilities.Length]; // initialize all the GUI arrays
        abilityBackgroundArray = new UnityEngine.UI.Image[abilities.Length];
        abilityCDIndicatorArray = new UnityEngine.UI.Image[abilities.Length];
        gleaming = new bool[abilities.Length];
        gleamed = new bool[abilities.Length];
        abilityGleamArray = new UnityEngine.UI.Image[abilities.Length];
        float tileSpacing = abilityBackground.sprite.bounds.size.x * 15; // Used to space out the abilities on the GUI

        abilityCDIndicator.fillAmount = 0;
        abilityGleam.color = Color.clear;

        for (int i = 0; i < core.GetAbilities().Length; i++)
        { // iterate through to display all the abilities
            if (abilities[i] == null) break;
            Vector3 pos = new Vector3(tileSpacing * i, this.transform.position.y, this.transform.position.z); // find where to position the images
            // position them all, do not keep the world position (?)
            abilityBackgroundArray[i] = Instantiate(abilityBackground, pos, Quaternion.identity) as UnityEngine.UI.Image;
            abilityBackgroundArray[i].transform.SetParent(transform, false);
            //Debug.Log("hi");
            //Debug.Log(abilities[i] + "" + abilities[i].GetID());
            abilityImagesArray[i] = Instantiate(abilityImagesInput[abilities[i].GetID()], pos, Quaternion.identity) as UnityEngine.UI.Image;
            abilityImagesArray[i].transform.SetParent(transform, false);
            
            abilityCDIndicatorArray[i] = Instantiate(abilityCDIndicator, pos, Quaternion.identity) as UnityEngine.UI.Image;
            abilityCDIndicatorArray[i].transform.SetParent(transform, false);

            abilityGleamArray[i] = Instantiate(abilityGleam, pos, Quaternion.identity) as UnityEngine.UI.Image;
            abilityGleamArray[i].transform.SetParent(transform, false);
        }
        initialized = true;
    }

    /// <summary>
    /// Used to update the ability and its representation to account 
    /// for whether or not the ability is active, on cooldown, or whether the core has sufficient energy
    /// </summary>
    /// <param name="index">index of the ability to update</param>
    private void AbilityUpdate(int index)
    {
        abilities[index].Tick((index+1).ToString()); // Tick the ability
        if (gleaming[index]) {
            Gleam(index);
        }
        if (abilities[index].GetCDRemaining() != 0) // on cooldown
        {
            gleamed[index] = false;
            abilityCDIndicatorArray[index].fillAmount = abilities[index].GetCDRemaining() / abilities[index].GetCDDuration();
            // adjust the cooldown indicator
        }
        else if (!gleaming[index] && !gleamed[index]) {
            //abilityGleamArray[index].fillAmount = 1;
            abilityGleamArray[index].color = Color.white;
            gleaming[index] = true;
            gleamed[index] = true;
        }
        if (abilities[index].GetActiveTimeRemaining() != 0) // active
        {
            abilityBackgroundArray[index].color = Color.green; // make the background green
        } 
        else if (core.GetHealth()[2] < abilities[index].GetEnergyCost()) // insufficient energy
        {
            abilityBackgroundArray[index].color = new Color(0, 0, 0.3F); // make the background dark blue
        }
        else if(abilityBackgroundArray[index].color != Color.white) // ability ready, reset color to white
        {
            abilityBackgroundArray[index].color = Color.white;
        }
    }

    private void Gleam(int index) {
        Color tmpColor = abilityGleamArray[index].color;
        tmpColor.a = Mathf.Max(0, tmpColor.a - 0.05F);
        abilityGleamArray[index].color = tmpColor;
        if (tmpColor.a == 0) gleaming[index] = false;
    }

    // Update is called once per frame
    private void Update () {
        if (initialized)
        {
            for (int i = 0; i < core.GetAbilities().Length; i++)
            { // update all abilities
                if (abilities[i] == null) break;
                AbilityUpdate(i);
            }
        }
	}
}
