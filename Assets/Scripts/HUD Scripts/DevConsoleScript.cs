using System;
using UnityEngine;
using UnityEngine.UI;

public class DevConsoleScript : MonoBehaviour
{
    public Text textBox;
    public Image image;
    public InputField inputField;

    public bool fullLog = false;

    void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }
     
    void Disable() {
        Application.logMessageReceived -= HandleLog;
    }
 
    void HandleLog(string logString, string stackTrace, LogType type) {
        string startingColor = "<color=white>";
        if((type == LogType.Log || type == LogType.Assert) && !fullLog) return;
        if(type == LogType.Exception || type == LogType.Error) startingColor = "<color=red>";
        else if(type == LogType.Warning) startingColor = "<color=orange>";

        stackTrace = stackTrace.Trim("\n".ToCharArray());
        if (textBox)
        {
            string text = textBox.text += "\n" + startingColor + logString + "\n    Stack Trace: " + stackTrace + "</color>";
            while (text.Length > 16000)
            {
                int cutIndex = text.IndexOf("</color>");
                if (cutIndex == -1)
                {
                    text = text.Substring(Mathf.Min(0, text.Length - 16000));
                    break;
                }
                text = text.Substring(cutIndex + 8);
            }
            textBox.text = text;
        }
        // Application.logMessageReceived -= HandleLog;
    }

    public void EnterCommand(string command)
    {
        inputField.text = "";
        inputField.ActivateInputField();

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
            PlayerCore.Instance.Warp(Vector3.zero);
            textBox.text += "\n<color=green>I, for one, welcome our new robotic overlords.</color>";
        }
        else if (command.StartsWith("Add power ", StringComparison.CurrentCultureIgnoreCase))
        {
            int number = int.Parse(command.Substring(10).Trim());
            PlayerCore.Instance.AddPower(number);
        }
        else if (command.Equals("Full log", StringComparison.CurrentCultureIgnoreCase))
        {
            fullLog = true;
            textBox.text += "\n<color=green>I see all, I know all</color>";
        }
        else if (command.Equals("Commit sudoku", StringComparison.CurrentCultureIgnoreCase))
        {
            PlayerCore.Instance.TakeCoreDamage(float.MaxValue);
            textBox.text += "\n<color=green>Die, die, die!</color>";
        }
        else if (command.Equals("Spectate", StringComparison.CurrentCultureIgnoreCase))
        {
            var player = PlayerCore.Instance;
            SpriteRenderer[] renderers = player.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].color = new Color(1f, 1f, 1f, 0.1f);
            }
            Collider2D[] colliders = player.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
            player.GetComponent<TractorBeam>().enabled = false;
            player.GetAbilityHandler().Deinitialize();
            player.hud.DeinitializeHUD();
            player.invisible = true;
            textBox.text += "\n<color=green>You can hide, but you can't run!</color>";
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
            if (image.enabled)
                inputField.ActivateInputField();
        }
    }
}
