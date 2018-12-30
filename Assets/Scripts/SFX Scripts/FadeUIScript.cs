using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to fade the UI if there is no activity
/// </summary>
public class FadeUIScript : MonoBehaviour {

    public PlayerCore core; // the core to check to fade the UI with
    public Canvas canvas; // the canvas to fade
    private bool initialized;
    private float groupalpha;

    public void Initialize(PlayerCore player)
    {
        core = player;
        if (!canvas) canvas = GetComponent<Canvas>();
        initialized = true;
    }
    /// <summary>
    /// Used to fade the UI
    /// </summary>
    private void Fade() {
        if (groupalpha > 0.1F) // check if opacity is above the threshold
        {
            groupalpha -= 2 * Time.deltaTime;
        }
        else
        {
            groupalpha = 0.1F;
            GroupUpdate();
        }// set opacity to minimum threshold
    }

    private void GroupUpdate()
    {
        foreach (CanvasGroup group in GetComponentsInChildren<CanvasGroup>())
        {

            group.alpha = groupalpha;
        }
    }
    // Update is called once per frame
    private void Update () {
        if (initialized)
        {
            if (core.GetIsBusy()) // if the core is busy make the canvas opaque
            {
                groupalpha = 1;
                GroupUpdate();
            }
            else // core not busy
            {
                Fade(); // fade UI
            }
        }
    }
}
