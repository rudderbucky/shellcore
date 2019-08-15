using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoText : MonoBehaviour
{
    public Text text;
    public Transform player;
    float timer;

    private void Start()
    {
        text.color = Color.clear;
    }

    public void showMessage(string message, string soundID = null)
    {
        timer = 0;
        if(soundID != null) {
            ResourceManager.PlayClipByID(soundID, true);
        }
        text.text = message;
        text.color = Color.white;
    }

    void Update() {
        if(text.color.a > 0) {
            timer += Time.deltaTime;
            if(timer > 3) text.color = text.color - new Color(0,0,0,1);
        }
    }
}
