using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum KeyName // Do not modify the order, it may break the keybinds
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
    AutoCastBuyTurret,

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
    [System.Serializable]
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
        {KeyName.CommandWheel, new Key(KeyCode.LeftAlt, "Party command wheel")},
        {KeyName.HideHUD, new Key(KeyCode.F1, "Hide HUD")},
        {KeyName.ShowChatHistory, new Key(KeyCode.Return, "Show chat history")},
        {KeyName.AutoCastBuyTurret, new Key(KeyCode.LeftShift, "Turret quick purchase (+ number) and Auto cast")},

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

    [System.Serializable]
    public class KeyMapping
    {
        public KeyName keyName;
        public Key key;
        public KeyMapping(KeyName keyName, Key key)
        {
            this.keyName = keyName;
            this.key = key;
        }
    }

    [System.Serializable]
    public class KeyMappingList
    {
        public List<KeyMapping> mappings;
    }



    public static void SaveControls()
    {
        var keyList = new List<KeyMapping>();
        foreach (var kvp in keys)
        {
            keyList.Add(new KeyMapping(kvp.Key, kvp.Value));
        }

        var list = new KeyMappingList();
        list.mappings = keyList;
        PlayerPrefs.SetString("Controls", JsonUtility.ToJson(list));
        PlayerPrefs.Save();
    }

    public static void LoadControls()
    {
        if (PlayerPrefs.HasKey("Controls"))
        {
            var c = JsonUtility.FromJson<KeyMappingList>(PlayerPrefs.GetString("Controls"));
            var controls = c.mappings;
            if (controls.Count == keys.Count)
            {
                keys.Clear();
                foreach (var km in controls)
                {
                    keys.Add(km.keyName, km.key);
                }
            }
        }
    }

    public static void ResetControls()
    {
        var keyNames = new List<KeyName>(keys.Keys);
        for (int i = 0; i < keyNames.Count; i++)
        {
            var keyName = keyNames[i];
            var key = keys[keyName];
            key.overrideKey = key.defaultKey;
            keys[keyName] = key;
        }
        SaveControls();
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
                    SaveControls();
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
        return keys.ContainsKey(key) ? Input.GetKey(keys[key].overrideKey) : false;
    }

    public static KeyCode GetKeyCode(KeyName key)
    {
        return keys.ContainsKey(key) ? keys[key].overrideKey : KeyCode.None;
    }

    public static bool GetKeyDown(KeyName key)
    {
        return keys.ContainsKey(key) ? Input.GetKeyDown(keys[key].overrideKey) : false;
    }

    public static bool GetKeyUp(KeyName key)
    {
        return keys.ContainsKey(key) ? Input.GetKeyUp(keys[key].overrideKey) : false;
    }
}