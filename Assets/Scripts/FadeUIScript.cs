using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to fade the UI if there is no activity
/// </summary>
public class FadeUIScript : MonoBehaviour {

    public PlayerCore core; // the core to check to fade the UI with
    public Canvas canvas; // the canvas to fade

    /// <summary>
    /// Used to fade the UI
    /// </summary>
    private void Fade() {
        if (canvas.GetComponent<CanvasGroup>().alpha > 0.1F) // check if opacity is above the threshold
        {
            canvas.GetComponent<CanvasGroup>().alpha -= 2 * Time.deltaTime; // reduce opacity if so
        }
        else canvas.GetComponent<CanvasGroup>().alpha = 0.1F; // set opacity to minimum threshold
    }

    // Update is called once per frame
    private void Update () {
        if (core.GetIsBusy()) // if the core is busy make the canvas opaque
        {
            canvas.GetComponent<CanvasGroup>().alpha = 1;
        }
        else // core not busy
        {
            Fade(); // fade UI
        }
    }
}
