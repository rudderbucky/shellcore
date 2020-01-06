using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Health Bar script that displays the player's health values
/// </summary>
public class HealthBarScript : MonoBehaviour {

    public GameObject inputBar;
    //public UnityEngine.UI.Image[] barsInputArray; // images of the health bars
    private UnityEngine.UI.Image[] barsArray; // instantiated bars
    //public UnityEngine.UI.Image[] gleamInputArray; // images of the gleam bars
    private UnityEngine.UI.Image[] gleamArray; // instantiated bars
    private bool initialized; // if this GUI component is initialized
    private bool[] gleaming; // if the bar is gleaming
    private bool[] gleamed; // if the bar has already gleamed in the cycle
    private string[] names = new string[] {"SHELL ", "CORE ", "ENERGY "};
    private PlayerCore player; // associated player

    /// <summary>
    /// Initializes the Health Bar UI
    /// </summary>
    public void Initialize(PlayerCore player)
    {
        this.player = player;
        barsArray = new UnityEngine.UI.Image[3]; // initialize arrays
        gleamArray = new UnityEngine.UI.Image[3];
        gleaming = new bool[barsArray.Length];
        gleamed = new bool[barsArray.Length];
        Color[] colors = new Color[] { Color.green, new Color(0.8F,0.8F,0.8F), new Color(0.4F,0.8F,1.0F) };
        for (int i = 0; i < barsArray.Length; i++) { // iterate through array
            barsArray[i] = Instantiate(inputBar).GetComponent<Image>(); // instantiate the image
            barsArray[i].fillAmount = 0; // initialize fill to 0 for cool animation
            barsArray[i].color = colors[i];
            Vector3 tmp = barsArray[i].transform.position;
            tmp.y -= 24*i;
            barsArray[i].transform.position = tmp;
            barsArray[i].transform.SetParent(transform, false); // set as parent to the object this script is on

            gleamArray[i] = Instantiate(inputBar).GetComponent<Image>(); // instantiate the image
            if(gleamArray[i].GetComponentInChildren<Text>()) 
            {
                foreach(Text text in gleamArray[i].GetComponentsInChildren<Text>())
                    Destroy(text.gameObject);
            }
            gleamArray[i].fillAmount = 0; // initialize fill to 0 for cool animation
            tmp = gleamArray[i].transform.position;
            tmp.y -= 24*i;
            gleamArray[i].transform.position = tmp;
            gleamArray[i].transform.SetParent(transform, false); // set as parent to the object this script is on
        }
        initialized = true; // set to initialized
    }

    /// <summary>
    /// Deinitializes the Health Bar UI
    /// </summary>
    public void Deinitialize() {
        for (int i = 1; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
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
                if(barsArray[i].GetComponentInChildren<Text>()) {
                    var x = barsArray[i].GetComponentsInChildren<Text>();
                    x[0].text = (int)currentHealth[i] + "/" + maxHealth[i];
                    x[1].text = names[i];
                }
            }
        }
    } 
}
