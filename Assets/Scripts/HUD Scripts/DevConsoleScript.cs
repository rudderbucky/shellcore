using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DevConsoleScript : MonoBehaviour
{
    public Text textBox;
    public Image image;
    public InputField inputField;
    public Scrollbar scrollbar;
    public ScrollRect scrollRect;

    public static bool componentEnabled = false;
    public static bool fullLog = false;
    public static bool godModeEnabled = false;
    public static bool spectateEnabled = false;
    private static bool warpingEnabled = false;
    public static bool WarpingEnabled
    {
        get { return godModeEnabled || warpingEnabled; }
        set { warpingEnabled = value; }
    }
    public bool updateLog = false;

    Queue<string> textToAdd = new Queue<string>();

    public static DevConsoleScript Instance;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        godModeEnabled = false;
        spectateEnabled = false;
        Instance = this;
        componentEnabled = false;
    }

    void Disable()
    {
        componentEnabled = false;
        Application.logMessageReceived -= HandleLog;
        Instance = null;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
        updateErrors.Clear();
    }

    // stores update errors to prevent duplicates
    static List<string> updateErrors = new List<string>();

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string startingColor = "<color=white>";
        if ((type == LogType.Log || type == LogType.Assert) && !fullLog)
        {
            return;
        }

        var isException = type == LogType.Exception || type == LogType.Error;
        if (isException)
        {
            startingColor = "<color=red>";
        }
        else if (type == LogType.Warning)
        {
            startingColor = "<color=orange>";
        }

        stackTrace = stackTrace.Trim("\n".ToCharArray());
        var text = $"\n{startingColor}{logString}\n    Stack Trace: {stackTrace}</color>";

        if (isException)
        {
            var important = "<color=red>I</color><color=orange>M</color><color=yellow>P</color><color=lime>O</color><color=violet>R</color><color=cyan>T</color><color=grey>A</color><color=white>N</color><color=red>T</color>: ";
            text += $"\n {important}In addition to screenshotting this error, please save a copy of Player.log and send it to rudderbucky/the ShellCore Command Discord at your earliest convenience."
                    + "\nThe file can be found at %APPDATA%/../LocalLow/rudderbucky_Ormanus/Shellcore Command/Player.log (Press F3 to close the console)";
        }

        var enqueued = false;
        if ((!stackTrace.Contains("Update ()") || !updateErrors.Contains(logString)) || updateLog)
        {
            updateErrors.Add(logString);
            textToAdd.Enqueue(text);
            enqueued = true;
        }

        if (isException && !componentEnabled && enqueued)
        {
            ToggleActive();
        }
        // Application.logMessageReceived -= HandleLog;
    }

    public static void Print(string logString)
    {
        Instance.textToAdd.Enqueue("\n <color=white>{logString}</color>");
    }

    private void GodPowers()
    {
        var player = PlayerCore.Instance;
        player.SetMaxHealth(new float[] { 99999, 99999, 99999 }, true);
        player.SetRegens(new float[] { 99999, 99999, 99999 });
        player.speed = 9999f;
        player.CalculatePhysicsConstants();
        player.DamageBoostStacks += 1000;
        player.AddPower(10000);
        godModeEnabled = true;
        MapMakerScript.EnableMapCheat();
    }

    public void EnterCommand(string command)
    {
        inputField.text = "";
        inputField.ActivateInputField();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SampleScene")
        {
            if (command.Equals("poor", StringComparison.CurrentCultureIgnoreCase))
            {
                GodPowers();
                textBox.text += "\n<color=lime>Have you tried being born into wealth?</color>";
            }
            else if (command.Equals("I am God", StringComparison.CurrentCultureIgnoreCase))
            {
                GodPowers();
                var player = PlayerCore.Instance;
                player.AddCredits(999999);
                textBox.text += "\n<color=lime>I am noob.</color>";
            }
            else if (command.Equals("Immortality", StringComparison.CurrentCultureIgnoreCase))
            {
                var player = PlayerCore.Instance;
                player.SetMaxHealth(new float[] { 99999, 99999, 99999 }, true);
                player.SetRegens(new float[] { 99999, 99999, 99999 });
                textBox.text += "\n<color=lime>Immortality is an illusion, enjoy it while it lasts.</color>";
            }
            else if (command.StartsWith("network ", StringComparison.CurrentCultureIgnoreCase))
            {
                switch(command.Substring(8).Trim())
                {
                    case "client":
                        MasterNetworkAdapter.StartClient();
                        textBox.text += "\n<color=lime>Running as client</color>";
                        break;
                    case "server":
                        MasterNetworkAdapter.StartServer();
                        textBox.text += "\n<color=lime>Running as server</color>";
                        break;
                }
                PlayerCore.Instance.Warp(Vector3.zero);
            }
            else if (command.StartsWith("Add power ", StringComparison.CurrentCultureIgnoreCase))
            {
                int number = int.Parse(command.Substring(10).Trim());
                if (PlayerCore.Instance)
                    PlayerCore.Instance.AddPower(number);
                else
                {
                    foreach (ShellCore core in AIData.entities)
                    {
                        if (core)
                            core.AddPower(number);
                    }
                }
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
                    {
                        Debug.Log("Wrong number format!");
                    }
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

                spectateEnabled = true;
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
                WarpingEnabled = true;
                textBox.text += "\n<color=lime>Polo.</color>";
            }
            else if (command.Equals("caught em all", StringComparison.CurrentCultureIgnoreCase))
            {
                PartIndexScript.partsObtainedCheat = true;
                textBox.text += "\n<color=lime>There's a time and place for everything! But not now.</color>";
            }
            else if (command.Equals("counting cards", StringComparison.CurrentCultureIgnoreCase))
            {
                NodeEditorFramework.Standard.RandomizerNode.PrintRandomRolls = true;
                textBox.text += "\n<color=lime>Don't let the casino catch you!</color>";
            }
            else if (command.Equals("Update debug", StringComparison.CurrentCultureIgnoreCase))
            {
                updateLog = true;
                textBox.text += "\n<color=lime>You're probably not gonna be able to see this.</color>";
            }
            else if (command.Equals("killp", StringComparison.CurrentCultureIgnoreCase))
            {
                foreach (ShellCore partyMember in PartyManager.instance.partyMembers)
                {
                    partyMember.KillShellCore();
                }
                textBox.text += "\n<color=lime>Killing all party members...</color>";
            }
            else if (command.StartsWith("addp ", StringComparison.CurrentCultureIgnoreCase))
            {
                string entityID = command.Substring(5).Trim();
                if (!PlayerCore.Instance.cursave.unlockedPartyIDs.Contains(entityID))
                {
                    PlayerCore.Instance.cursave.unlockedPartyIDs.Add(entityID);
                }

                PartyManager.instance.AssignBackend(entityID);
                textBox.text += $"\n<color=lime>{entityID}</color>";
            }
            else if (command.Equals("Damacy", StringComparison.CurrentCultureIgnoreCase))
            {
                /* Adds all part/ability/tier/drone permutations to the player's inventory.*/
                var parts = PlayerCore.Instance.GetInventory();
                EntityBlueprint.PartInfo info;
                for (int i = 0; i < 8; i++)
                {
                    info = new EntityBlueprint.PartInfo();
                    info.partID = "SmallCenter1";
                    info.abilityID = 10;
                    DroneSpawnData data = DroneUtilities.GetDefaultData((DroneType)i);
                    info.secondaryData = JsonUtility.ToJson(data);
                    if (info.abilityID == 0 || info.abilityID == 10)
                    {
                        info.tier = 0;
                    }

                    parts.Add(info);
                }

                info = new EntityBlueprint.PartInfo();
                foreach (string name in ResourceManager.allPartNames)
                {
                    for (int i = 0; i < 42; i++)
                    {
                        info.partID = name;
                        info.abilityID = i;
                        if ((info.abilityID >= 14 && info.abilityID <= 16) || info.abilityID == 3 || info.abilityID == 39)
                        {
                            info.abilityID = 0;
                        }

                        if (info.abilityID == 10)
                        {
                            DroneSpawnData data = DroneUtilities.GetDefaultData((DroneType)UnityEngine.Random.Range(0, 8));
                            info.secondaryData = JsonUtility.ToJson(data);
                        }

                        if (info.abilityID == 0 || info.abilityID == 10 || info.abilityID == 21 || info.abilityID == 24 || info.abilityID == 27 || info.abilityID == 28 || info.abilityID == 29 || info.abilityID == 41)
                        {
                            info.tier = 0;
                        }
                        else
                        {
                            info.tier = 1;
                        }

                        parts.Add(info);
                        parts.Add(info);
                        parts.Add(info);
                    }
                }

                textBox.text += "\n<color=lime>Katamete korogasu I LOVE YOU!</color>";
            }
            else if (command.Equals("Win siege", StringComparison.CurrentCultureIgnoreCase))
            {
                NodeEditorFramework.Standard.WinSiegeCondition.OnSiegeWin.Invoke(SectorManager.instance.current.sectorName);
            }
            else if (command.Equals("Win bz", StringComparison.CurrentCultureIgnoreCase))
            {
                NodeEditorFramework.Standard.WinBattleCondition.OnBattleWin.Invoke(SectorManager.instance.current.sectorName);
            }
            else if (command.Equals("No limits", StringComparison.CurrentCultureIgnoreCase))
            {
                if (PlayerCore.Instance?.cursave != null)
                {
                    PlayerCore.Instance.cursave.abilityCaps = PlayerCore.Instance.abilityCaps = new int[] { 99999, 99999, 99999, 99999 };
                    textBox.text += "\n<color=lime>Ouch, now I have a headache.</color>";
                }
            }
            else if (command.Equals("I am an uncivilized barbarian", StringComparison.CurrentCultureIgnoreCase))
            {
                if (PlayerCore.Instance)
                {
                    PlayerCore.Instance.SetWeaponGCD(0);
                    textBox.text += "\n<color=lime>Heathen.</color>";
                }
            }
            else if (command.Equals("Stuff is broken", StringComparison.CurrentCultureIgnoreCase))
            {
                if (PlayerCore.Instance)
                {
                    TaskDisplayScript.EditMode = true;
                    textBox.text += "\n<color=lime>Surely a really bad bug went off and you don't just want to skip content we worked so hard to make, right?</color>";
                }
            }
            else if (command.StartsWith("loadbp ", StringComparison.CurrentCultureIgnoreCase))
            {
                string blueprint = command.Substring(7).Trim();
                ToggleActive();
                MasterNetworkAdapter.instance.CreatePlayerServerRpc(MasterNetworkAdapter.playerName, blueprint, 0);
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

    public void SetActive()
    {
        if (!textBox.enabled) ToggleActive();
    }

    public void ToggleActive()
    {
        textBox.enabled = image.enabled = !image.enabled;
        componentEnabled = image.enabled;
        scrollRect.enabled = image.enabled;
        scrollbar.gameObject.SetActive(image.enabled);
        inputField.gameObject.SetActive(image.enabled);
        if (image.enabled)
        {
            scrollRect.verticalNormalizedPosition = 0;
            inputField.ActivateInputField();
        }
    }

    void Update()
    {
        if (InputManager.GetKeyDown(KeyName.Console))
        {
            ToggleActive();
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
