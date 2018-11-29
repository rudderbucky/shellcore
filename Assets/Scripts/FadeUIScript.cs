using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeUIScript : MonoBehaviour {

    public PlayerCore core;
    // bool someKeyPressed;
    public Canvas canvas;
    // private float timeCounter;

    private void Fade() {
        if (canvas.GetComponent<CanvasGroup>().alpha > 0.1F)
        {
            canvas.GetComponent<CanvasGroup>().alpha -= 0.05F;
        }
    }
	// Use this for initialization

    /*private void OnGUI()
    {
        Event e = Event.current; // event used to grab keystrokes
        if (e != null && e.isKey) // check if event is parseable
        {
            switch (e.character)
            {
                case 'w': // input keys that shouldn't trigger snap
                case 'a':
                case 's':
                case 'd':
                case (char)0:
                    someKeyPressed = false;
                    break;
                default: // everything else triggers snap
                    timeCounter = 0;
                    someKeyPressed = true;
                    break;
            }
        }
    }*/
    // Update is called once per frame
    private void Update () {
        /*if(!someKeyPressed) {
            timeCounter += Time.deltaTime;
        }*/
        if (core.GetIsBusy())
        {
            canvas.GetComponent<CanvasGroup>().alpha = 1;
        }
        else
        {
            Fade();
        }
    }
}
