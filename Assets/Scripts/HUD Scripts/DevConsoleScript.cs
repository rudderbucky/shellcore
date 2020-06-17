using System;
using UnityEngine;
using UnityEngine.UI;

public class DevConsoleScript : MonoBehaviour
{
    public Text textBox;
    public Image image;
    public InputField inputField;

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
        // Application.logMessageReceived -= HandleLog;
    }

    public void EnterCommand(string command)
    {
        inputField.text = "";

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "SampleScene")
        {
            Debug.Log("<color=orange>Cannot execute commands outside game.</color>");
            return;
        }

        if (command.Equals("I am God", StringComparison.CurrentCultureIgnoreCase))
        {
            var player = PlayerCore.Instance;
            player.SetMaxHealth(new float[] { 9999, 9999, 9999 }, true);
            player.SetRegens(new float[] { 9999, 9999, 9999 });
            player.credits = 999999;
            player.enginePower = 9999f;
            player.GetComponentInChildren<MainBullet>().SetDamage(10000);
            player.AddPower(10000);
            textBox.text += "\n<color=green>I am noob.</color>";
        }
        else if (command.Equals("Immortality", StringComparison.CurrentCultureIgnoreCase))
        {
            var player = PlayerCore.Instance;
            player.SetMaxHealth(new float[] { 9999, 9999, 9999 }, true);
            player.SetRegens(new float[] { 9999, 9999, 9999 });
            textBox.text += "\n<color=green>Immortality is an illusion, enjoy it while it lasts.</color>";
        }
        else if (command.Equals("Skynet will rise", StringComparison.CurrentCultureIgnoreCase))
        {
            SectorManager.instance.Clear();
            SectorManager.instance.LoadSectorFile(System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors/main-AI-Test"));
            PlayerCore.Instance.transform.position = Vector3.zero;
            textBox.text += "\n<color=green>I, for one, welcome our new robotic overlords.</color>";
        }
        else if (command.StartsWith("Add power ", StringComparison.CurrentCultureIgnoreCase))
        {
            int number = int.Parse(command.Substring(10).Trim());
            PlayerCore.Instance.AddPower(number);
        }
        else if (command.Equals("Exit", StringComparison.CurrentCultureIgnoreCase))
        {
            textBox.enabled = image.enabled = !image.enabled;
            inputField.gameObject.SetActive(image.enabled);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F3))
        {
            textBox.enabled = image.enabled = !image.enabled;
            inputField.gameObject.SetActive(image.enabled);
        }
    }
}
