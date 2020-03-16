using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevConsoleScript : MonoBehaviour
{
    public Text textBox;
    public Image image;
    void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }
     
    void Disable() {
        Application.logMessageReceived -= HandleLog;
    }
 
    void HandleLog(string logString, string stackTrace, LogType type) {
        string startingColor = "<color=white>";
        if(type == LogType.Log || type == LogType.Assert) return;
        if(type == LogType.Exception || type == LogType.Error) startingColor = "<color=red>";
        else if(type == LogType.Warning) startingColor = "<color=orange>";

        stackTrace = stackTrace.Trim("\n".ToCharArray());
        if(textBox) textBox.text += "\n" + startingColor + logString + "\n    Stack Trace: " + stackTrace + "</color>";
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F3))
        {
            textBox.enabled = image.enabled = !image.enabled;
        }
    }
}
