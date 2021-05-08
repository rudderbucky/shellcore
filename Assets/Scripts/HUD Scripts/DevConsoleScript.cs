using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevConsoleScript : MonoBehaviour
{
    public Text textBox;
    public Image image;
    public InputField inputField;

    public static bool componentEnabled = false;
    public bool fullLog = false;
    public static bool godModeEnabled = false;
    public bool updateLog = false;

    Queue<string> textToAdd = new Queue<string>();

    static DevConsoleScript Instance;

    void OnEnable() {
        Application.logMessageReceived += HandleLog;
        godModeEnabled = false;
        Instance = this;
    }
     
    void Disable() {
        componentEnabled = false;
        Application.logMessageReceived -= HandleLog;
        Instance = null;
    }
 
    void HandleLog(string logString, string stackTrace, LogType type) {
        string startingColor = "<color=white>";
        if((type == LogType.Log || type == LogType.Assert) && !fullLog) return;
        if(type == LogType.Exception || type == LogType.Error) startingColor = "<color=red>";
        else if(type == LogType.Warning) startingColor = "<color=orange>";

        stackTrace = stackTrace.Trim("\n".ToCharArray());
        if(!stackTrace.Contains("Update ()") || updateLog)
            textToAdd.Enqueue("\n" + startingColor + logString + "\n    Stack Trace: " + stackTrace + "</color>");
        // Application.logMessageReceived -= HandleLog;
    }

    public static void Print(string logString)
    {
        Instance.textToAdd.Enqueue("\n <color=white>" + logString + "</color>");
    }

    public void EnterCommand(string command)
    {
        inputField.text = "";
        inputField.ActivateInputField();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SampleScene")
        {
            if (command.Equals("I am God", StringComparison.CurrentCultureIgnoreCase))
            {
                var player = PlayerCore.Instance;
                player.SetMaxHealth(new float[] { 99999, 99999, 99999 }, true);
                player.SetRegens(new float[] { 99999, 99999, 99999 });
                player.AddCredits(999999);
                player.enginePower = 9999f;
                player.speed = 9999f;
                player.CalculatePhysicsConstants();
                player.damageAddition = 99999f;
                player.AddPower(10000);
                godModeEnabled = true;
                MapMakerScript.EnableMapCheat();
                textBox.text += "\n<color=lime>I am noob.</color>";
            }
            else if (command.Equals("Immortality", StringComparison.CurrentCultureIgnoreCase))
            {
                var player = PlayerCore.Instance;
                player.SetMaxHealth(new float[] { 99999, 99999, 99999 }, true);
                player.SetRegens(new float[] { 99999, 99999, 99999 });
                textBox.text += "\n<color=lime>Immortality is an illusion, enjoy it while it lasts.</color>";
            }
            else if (command.Equals("Skynet will rise", StringComparison.CurrentCultureIgnoreCase))
            {
                SectorManager.instance.Clear();
                SectorManager.instance.LoadSectorFile(System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors/AI-Test"));
                PlayerCore.Instance.Warp(Vector3.zero);
                textBox.text += "\n<color=lime>I, for one, welcome our new robotic overlords.</color>";
            }
            else if (command.StartsWith("Add power ", StringComparison.CurrentCultureIgnoreCase))
            {
                int number = int.Parse(command.Substring(10).Trim());
                PlayerCore.Instance.AddPower(number);
            }
            else if (command.StartsWith("Add rep ", StringComparison.CurrentCultureIgnoreCase))
            {
                int number = int.Parse(command.Substring(8).Trim());
                PlayerCore.Instance.reputation += number;
            }
            else if (command.StartsWith("Add shards ", StringComparison.CurrentCultureIgnoreCase))
            {
                int number = int.Parse(command.Substring(11).Trim());
                PlayerCore.Instance.shards += number;
            }
            else if (command.StartsWith("Add money ", StringComparison.CurrentCultureIgnoreCase))
            {
                int number = int.Parse(command.Substring(10).Trim());
                PlayerCore.Instance.AddCredits(number);
            }
            else if (command.Equals("Full log", StringComparison.CurrentCultureIgnoreCase))
            {
                fullLog = true;
                textBox.text += "\n<color=lime>I see all, I know all</color>";
            }
            else if (command.Equals("Commit sudoku", StringComparison.CurrentCultureIgnoreCase))
            {
                PlayerCore.Instance.TakeCoreDamage(float.MaxValue);
                textBox.text += "\n<color=lime>Die, die, die!</color>";
            }
            else if (command.StartsWith("Speed of light", StringComparison.CurrentCultureIgnoreCase))
            {
                int locNum = 0;
                if (command.Length > 14)
                {
                    bool success = int.TryParse(command.Substring(14).Trim(), out locNum);
                    if (!success)
                        Debug.Log("Wrong number format!");
                }

                // TODO: broke, fix
                /*
                if (locNum < TaskManager.objectiveLocations.Count)
                {
                    PlayerCore.Instance.Warp(TaskManager.objectiveLocations[locNum].location);
                }
                */
                textBox.text += "\n<color=lime>Country roads, take me home. To the place I belong!</color>";
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
                player.IsInvisible = true;
                textBox.text += "\n<color=lime>You can hide, but you can't run!</color>";
            }
            else if (command.Equals("Exit", StringComparison.CurrentCultureIgnoreCase))
            {
                textBox.enabled = image.enabled = !image.enabled;
                inputField.gameObject.SetActive(image.enabled);
            }
            else if (command.Equals("I am Ormanus", StringComparison.CurrentCultureIgnoreCase))
            {
                EnterCommand("I am god");
                EnterCommand("spectate");
                EnterCommand("skynet will rise");
            }
            else if (command.Equals("fps", StringComparison.CurrentCultureIgnoreCase))
            {
                textBox.text += $"\n{1f / Time.smoothDeltaTime}";
            }
            else if (command.Equals("parts please", StringComparison.CurrentCultureIgnoreCase))
            {
                ShipBuilder.heavyCheat = true;
                textBox.text += "\n<color=lime>I just wanna equip DeadZone parts for god's sake.</color>";
            }
            else if (command.Equals("moar data", StringComparison.CurrentCultureIgnoreCase))
            {
                ReticleScript.instance.DebugMode = true;
            }
            else if (command.Equals("marco", StringComparison.CurrentCultureIgnoreCase))
            {
                MapMakerScript.EnableMapCheat();
                textBox.text += "\n<color=lime>Polo.</color>";
            }
            else if(command.Equals("caught em all", StringComparison.CurrentCultureIgnoreCase))
            {
                PartIndexScript.partsObtainedCheat = true;
                textBox.text += "\n<color=lime>There's a time and place for everything! But not now.</color>";
            }
            else if (command.Equals("counting cards", StringComparison.CurrentCultureIgnoreCase))
            {
                NodeEditorFramework.Standard.RandomizerNode.PrintRandomRolls = true;
                textBox.text += "\n<color=lime>Don't let the casino catch you!</color>";
            }
            else if(command.Equals("Update debug", StringComparison.CurrentCultureIgnoreCase))
            {
                updateLog = true;
                textBox.text += "\n<color=lime>You're probably not gonna be able to see this.</color>";
            }
        }
        else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (command.StartsWith("Load ", StringComparison.CurrentCultureIgnoreCase))
            {
                string directory = command.Substring(5).Trim();
                string finalPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", directory);
                if (System.IO.Directory.Exists(finalPath))
                {
                    SectorManager.customPath = finalPath;
                    VersionNumberScript.version = directory + " [Custom]";
                    VersionNumberScript.Refresh();
                    textBox.text += "\n<color=lime>Custom world loaded!</color>";
                }
                else
                {
                    textBox.text += "\n<color=orange>Invalid path.</color>";
                }
            }
        }

    }

    void Update()
    {
        if(InputManager.GetKeyDown(KeyName.Console))
        {
            textBox.enabled = image.enabled = !image.enabled;
            componentEnabled = image.enabled;
            inputField.gameObject.SetActive(image.enabled);
            if (image.enabled)
                inputField.ActivateInputField();
        }

        if (textBox)
        {
            while (textToAdd.Count > 0)
            {
                string text = textBox.text += textToAdd.Dequeue();
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
        }
    }
}
