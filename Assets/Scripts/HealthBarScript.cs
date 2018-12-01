using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Health Bar script that displays the player's health values
/// </summary>
public class HealthBarScript : MonoBehaviour {

    public UnityEngine.UI.Image[] barsInputArray; // images of the health bars
    private UnityEngine.UI.Image[] barsArray; // instantiated bars
    public UnityEngine.UI.Image[] gleamInputArray; // images of the gleam bars
    private UnityEngine.UI.Image[] gleamArray; // instantiated bars
    private bool initialized; // if this GUI component is initialized
    private bool[] gleaming; // if the bar is gleaming
    private bool[] gleamed; // if the bar has already gleamed in the cycle
    public PlayerCore player; // associated player

    /// <summary>
    /// Initializes the Health Bar UI
    /// </summary>
    public void Initialize()
    {
        barsArray = new UnityEngine.UI.Image[barsInputArray.Length]; // initialize arrays
        gleamArray = new UnityEngine.UI.Image[barsInputArray.Length];
        gleaming = new bool[barsArray.Length];
        gleamed = new bool[barsArray.Length];
        for (int i = 0; i < barsArray.Length; i++) { // iterate through array
            barsArray[i] = Instantiate(barsInputArray[i]) as UnityEngine.UI.Image; // instantiate the image
            barsArray[i].fillAmount = 0; // initialize fill to 0 for cool animation
            barsArray[i].transform.SetParent(transform, false); // set as parent to the object this script is on

            gleamArray[i] = Instantiate(gleamInputArray[i]) as UnityEngine.UI.Image; // instantiate the image
            gleamArray[i].fillAmount = 0; // initialize fill to 0 for cool animation
            gleamArray[i].transform.SetParent(transform, false); // set as parent to the object this script is on
        }
        initialized = true; // set to initialized
    }

    /// <summary>
    /// Deinitializes the Health Bar UI
    /// </summary>
    public void Deinitialize() {
        for (int i = 0; i < barsArray.Length; i++) { // iterate through array
            // destroy the associated bars
            Destroy(barsArray[i].gameObject);
            Destroy(gleamArray[i].gameObject);
        }
        initialized = false; // reset initialized
    }

    /// <summary>
    /// Used to gleam the health bar
    /// </summary>
    /// <param name="index">the index at which to gleam</param>
    private void Gleam(int index) {
        gleamArray[index].fillAmount = barsArray[index].fillAmount; // set the fill amount to match the associated bar
        Color tmpColor = gleamArray[index].color; // get the gleam color
        tmpColor.a = Mathf.Max(0, tmpColor.a - 3 * Time.deltaTime); // reduce the opacity
        gleamArray[index].color = tmpColor; // set the gleam color
        if (tmpColor.a == 0) gleaming[index] = false; // if opacity is now 0 it is done gleaming
    }

    /// <summary>
    /// Helper method to get the current fill amount of a bar
    /// </summary>
    /// <param name="fillAmount">The current fill amount</param>
    /// <param name="currentHealth">the current player health</param>
    /// <param name="maxHealth">the maximum player health</param>
    /// <returns>the new fill amount for the bar</returns>
    private float UpdateBar(float fillAmount, float currentHealth, float maxHealth) {
        if (fillAmount < currentHealth / maxHealth) // bar is too short
        {
            fillAmount += 4 * Time.deltaTime; // increment fill amount 
        }
        if (fillAmount >= currentHealth / maxHealth) // if the fill amount overshot
        {
            return currentHealth / maxHealth; // return the ratio between the health values
        }
        else
        {
            return fillAmount; // otherwise return the new fill amount
        }
    }

    private void Update()
    {
        if (initialized) // check if it is safe to update
        {
            float[] currentHealth = player.GetHealth(); // grab the health values
            float[] maxHealth = player.GetMaxHealth();
            for (int i = 0; i < currentHealth.Length; i++) // iterate through bar arrays
            {
                if (barsArray[i].fillAmount != 1) { // check if the bar can gleam again to indicate full health
                    gleamed[i] = false; // ready bool value for another gleam
                }
                if (gleaming[i]) // check if gleaming
                {
                    Gleam(i); // continue gleaming if so
                }
                // check if the bar is ready to gleam
                if (barsArray[i].fillAmount == 1 && !gleamed[i])
                {
                    gleamArray[i].color = Color.white; // reset gleam bar
                    gleamed[i] = true; // update bool values if so
                    gleaming[i] = true;
                }
                barsArray[i].fillAmount = UpdateBar(barsArray[i].fillAmount, currentHealth[i], maxHealth[i]); 
                // otherwise directly update bar
            }

            // adjust render order of bars
            if (barsArray[0].fillAmount > barsArray[1].fillAmount)
            {
                gleamArray[0].transform.SetAsFirstSibling();
                barsArray[0].transform.SetAsFirstSibling(); // sets the shell and gleam to render first so core texture doesn't get overlapped     
            }
            else if (barsArray[0].fillAmount <= barsArray[1].fillAmount) // explicitly done to minimize method calls
            {
                gleamArray[1].transform.SetAsFirstSibling();
                barsArray[1].transform.SetAsFirstSibling(); // vice-versa
            }
        }
    } 
}
