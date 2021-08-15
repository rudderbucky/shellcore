using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum KeyName
{
    Up,
    Left,
    Down,
    Right,
    Interact,
    ToggleTractorBeam,
    Exit,
    StatusMenu,
    PauseMenu,
    Console,
    CommandWheel,
    HideHUD,
    ShowChatHistory,
    TurretQuickPurchase,

    ShowSkills,
    ShowSpawns,
    ShowWeapons,
    ShowPassives,

    Ability0,
    Ability1,
    Ability2,
    Ability3,
    Ability4,
    Ability5,
    Ability6,
    Ability7,
    Ability8,
    Ability9
}

public class InputManager : MonoBehaviour
{
    public struct Key
    {
        //public KeyName name;
        public KeyCode defaultKey;
        public KeyCode overrideKey;
        public string description;

        public Key(KeyCode defaultKey, string description)
        {
            //this.name = name;
            this.defaultKey = defaultKey;
            overrideKey = defaultKey;
            this.description = description;
        }
    }

    // TODO: Move to a scriptable object?
    public static Dictionary<KeyName, Key> keys = new Dictionary<KeyName, Key>
    {
        {KeyName.Up, new Key(KeyCode.W, "Move up")},
        {KeyName.Left, new Key(KeyCode.A, "Move left")},
        {KeyName.Down, new Key(KeyCode.S, "Move down")},
        {KeyName.Right, new Key(KeyCode.D, "Move right")},
        {KeyName.Interact, new Key(KeyCode.Q, "Interact")},
        {KeyName.ToggleTractorBeam, new Key(KeyCode.Space, "Toggle tractor beam")},
        {KeyName.Exit, new Key(KeyCode.Escape, "Exit/Cancel")},
        {KeyName.StatusMenu, new Key(KeyCode.E, "Status menu")},
        {KeyName.PauseMenu, new Key(KeyCode.Escape, "Pause menu")},
        {KeyName.Console, new Key(KeyCode.F3, "Open developer console")},
        {KeyName.CommandWheel, new Key(KeyCode.LeftControl, "Party command wheel")},
        {KeyName.HideHUD, new Key(KeyCode.F1, "Hide HUD")},
        {KeyName.ShowChatHistory, new Key(KeyCode.Return, "Show chat history")},
        {KeyName.TurretQuickPurchase, new Key(KeyCode.LeftShift, "Turret quick purchase (+ number)")},

        {KeyName.ShowSkills, new Key(KeyCode.Z, "Switch to skill hotbar")},
        {KeyName.ShowSpawns, new Key(KeyCode.X, "Switch to spawn hotbar")},
        {KeyName.ShowWeapons, new Key(KeyCode.C, "Switch to weapon hotbar")},
        {KeyName.ShowPassives, new Key(KeyCode.V, "Switch to passive hotbar")},

        {KeyName.Ability0, new Key(KeyCode.Alpha1, "Use ability #1")},
        {KeyName.Ability1, new Key(KeyCode.Alpha2, "Use ability #2")},
        {KeyName.Ability2, new Key(KeyCode.Alpha3, "Use ability #3")},
        {KeyName.Ability3, new Key(KeyCode.Alpha4, "Use ability #4")},
        {KeyName.Ability4, new Key(KeyCode.Alpha5, "Use ability #5")},
        {KeyName.Ability5, new Key(KeyCode.Alpha6, "Use ability #6")},
        {KeyName.Ability6, new Key(KeyCode.Alpha7, "Use ability #7")},
        {KeyName.Ability7, new Key(KeyCode.Alpha8, "Use ability #8")},
        {KeyName.Ability8, new Key(KeyCode.Alpha9, "Use ability #9")},
        {KeyName.Ability9, new Key(KeyCode.Alpha0, "Use ability #10")}
    };

    static InputManager instance;
    KeyName? inputToChange = null;
    Text keyText = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        LoadControls();
    }

    public static void SaveControls()
    {
        PlayerPrefs.SetString("Controls", JsonUtility.ToJson(keys));
        PlayerPrefs.Save();
    }

    public static void LoadControls()
    {
        if (PlayerPrefs.HasKey("Controls"))
        {
            var controls = JsonUtility.FromJson<Dictionary<KeyName, Key>>(PlayerPrefs.GetString("Controls"));
            if (controls.Count == keys.Count)
            {
                keys = controls;
            }
        }
    }

    public static void ResetControls()
    {
        foreach (var pair in keys)
        {
            Key key = pair.Value;
            key.overrideKey = key.defaultKey;
            keys[pair.Key] = key;
        }
    }

    public static void ChangeControl(KeyName name, Text text)
    {
        if (instance.inputToChange == null)
        {
            instance.inputToChange = name;
            instance.keyText = text;
            instance.keyText.color = Color.red;
            Debug.Log("Waiting for input for " + name);
        }
    }

    private void Update()
    {
        if (inputToChange != null)
        {
            var keycodes = System.Enum.GetValues(typeof(KeyCode));
            foreach (var key in keycodes)
            {
                if (Input.GetKeyDown((KeyCode)key))
                {
                    SetControl(inputToChange.Value, (KeyCode)key);

                    keyText.text = ((KeyCode)key).ToString();
                    keyText.color = Color.white;
                    keyText = null;

                    if (inputToChange.Value >= KeyName.Ability0 && inputToChange.Value <= KeyName.Ability9)
                    {
                        if (AbilityHandler.instance) AbilityHandler.instance.ReorientAbilityBoxes();
                    }

                    Debug.Log($"Set binding for {inputToChange.Value} to {(KeyCode)key}");
                    inputToChange = null;

                    break;
                }
            }
        }
    }

    public static void SetControl(KeyName name, KeyCode code)
    {
        var key = keys[name];
        key.overrideKey = code;
        keys[name] = key;
    }

    public static bool GetKey(KeyName key)
    {
        return Input.GetKey(keys[key].overrideKey);
    }

    public static bool GetKeyDown(KeyName key)
    {
        return Input.GetKeyDown(keys[key].overrideKey);
    }

    public static bool GetKeyUp(KeyName key)
    {
        return Input.GetKeyUp(keys[key].overrideKey);
    }
}
