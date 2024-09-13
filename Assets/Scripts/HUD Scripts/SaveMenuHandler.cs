using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SaveMenuHandler : GUIWindowScripts
{
    List<PlayerSave> saves;
    List<string> paths;
    public Transform worldContents;
    public Transform contents;
    public GameObject worldIconPrefab;
    public GameObject saveIconPrefab;
    List<Button> worldButtons;
    List<SaveMenuIcon> icons;
    public InputField inputField;
    public Text inputFeedback;
    int indexToDelete;
    int indexToMigrate;
    public GUIWindowScripts deletePrompt;
    public GUIWindowScripts migratePrompt;
    public static float? migratedTimePlayed = null;
    public static SaveMenuHandler instance;
    public string resourcePath = "";
    public GameObject saveView;
    public GameObject worldView;
    public Text selectIndicatorText;

    public static List<string> migrationVersions = new List<string>()
    {
        "Alpha 1.0.0",
        "Alpha 2.0.0",
        "Alpha 2.1.0",
        "Alpha 4.0.0",
        "Alpha 4.1.0",
        "Alpha 4.1.1",
        "Alpha 4.2.0",
        "Alpha 4.3.0",
        "Beta 0.0.0",
        "Beta 0.1.1",
        "Beta 1.0.0",
        "Beta 2.0.0",
        "Beta 2.0.1",
        "Beta 2.1.0"
    };

    public Sprite[] episodeSprites;

    // Use to check before world load if the format is correct. Helps prevent loading incorrectly installed worlds
    private bool CheckWorldValidity(string dir)
    {
        // check the 3 main folders are in the world
        bool foundEntities = false;
        bool foundCanvases = false;

        foreach (var childDir in System.IO.Directory.GetDirectories(dir))
        {
            if (childDir.Contains("Entities"))
            {
                foundEntities = true;
            }

            if (childDir.Contains("Canvases"))
            {
                foundCanvases = true;
            }
        }

        return foundCanvases && foundEntities;
    }

    void SetUpWorlds()
    {
        worldButtons = new List<Button>();
        selectIndicatorText.text = "SELECT WORLD";
        string[] directories = Directory.GetDirectories(System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors"));
        foreach (var dir in directories)
        {
            if (dir.Contains("main") && !dir.Contains(VersionNumberScript.mapVersion))
            {
                continue;
            }

            if (dir.Contains(VersionNumberScript.rdbMap))
            {
                continue;
            }

            if (dir.Contains("TestWorld"))
            {
                continue;
            }

            string xdir = dir;
            Button worldButton = Instantiate(worldIconPrefab, worldContents).GetComponent<Button>();
            worldButtons.Add(worldButton);
            worldButton.GetComponentInChildren<Text>().text = dir.Contains("main") ? "Main world" : System.IO.Path.GetFileName(dir);

            if (!CheckWorldValidity(dir))
            {
                worldButton.GetComponentInChildren<Text>().color = Color.red;
                worldButton.GetComponentInChildren<Text>().text += " - World may be incorrectly installed";
            }

            worldButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                this.resourcePath = dir.Contains("main") ? "" : xdir;
                worldView.SetActive(false);
                InitializeSaves();
            }));
            if (dir.Contains("main"))
            {
                worldButton.transform.SetSiblingIndex(0);
            }
        }
    }

    void SetUpSaves()
    {
        saves = new List<PlayerSave>();
        paths = new List<string>();
        icons = new List<SaveMenuIcon>();
        string path = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        else
        {
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                if (file.Contains("TestSave"))
                {
                    continue;
                }

                try
                {
                    string savejson = File.ReadAllText(file);
                    PlayerSave save = JsonUtility.FromJson<PlayerSave>(savejson);
                    saves.Add(save);
                    paths.Add(file);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                    continue;
                }
            }
        }
    }

    void InitializeSaves()
    {
        saveView.SetActive(true);
        selectIndicatorText.text = "SELECT SAVE";
        string curpath = null;

        var CurrentSavePath = System.IO.Path.Combine(Application.persistentDataPath, "CurrentSavePath");
        if (File.Exists(CurrentSavePath))
        {
            curpath = File.ReadAllText(CurrentSavePath);
        }

        for (int i = 0; i < saves.Count; i++)
        {
            if (saves[i] == null)
            {
                continue;
            }

            SaveMenuIcon icon = Instantiate(saveIconPrefab, contents).GetComponent<SaveMenuIcon>();
            icon.GetComponent<Image>().sprite = episodeSprites[Mathf.Min(Mathf.Max(1, saves[i].episode), episodeSprites.Length) - 1];
            icon.save = saves[i];
            icon.index = i;
            icon.handler = this;
            icon.path = paths[i];
            icons.Add(icon);
            if (resourcePath == "")
            {
                if (icon.path == curpath || i == 0)
                {
                    icon.transform.SetAsFirstSibling();
                }
                else
                {
                    icon.transform.SetSiblingIndex(1);
                }
            }
            else
            {
                icon.transform.SetSiblingIndex(0);
            }

            if ((resourcePath == "" && saves[i].resourcePath != null && !saves[i].resourcePath.Contains("main") && saves[i].resourcePath != "")
                || (resourcePath != "" && saves[i].resourcePath != resourcePath))
            {
                icon.gameObject.SetActive(false);
            }
        }
    }

    void Awake()
    {
        instance = this;
    }

    void Initialize(string resourcePath = "")
    {
        this.resourcePath = resourcePath;
        worldView.SetActive(true);
        saveView.SetActive(false);
        SetUpWorlds();
        SetUpSaves();
    }

    public override void Activate()
    {
        base.Activate();
        Initialize();
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    public void Activate(string resourcePath = "")
    {
        base.Activate();
        Initialize(resourcePath);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    public override void CloseUI()
    {
        foreach (SaveMenuIcon icon in icons)
        {
            Destroy(icon.gameObject);
        }

        foreach (var worldButton in worldButtons)
        {
            Destroy(worldButton.gameObject);
        }

        icons.Clear();
        worldButtons.Clear();
        base.CloseUI();
    }

    public void OpenSavePrompt()
    {
        inputField.transform.parent.GetComponentInChildren<GUIWindowScripts>().ToggleActive();
        inputField.transform.parent.Find("Background").GetComponentInChildren<Text>().text = "Name your ShellCore:\n" +
                                                                                             "(Warning: closing this box, entering nothing, or entering a name already in use by another save will terminate this process.)";
        inputField.transform.parent.Find("Create Save").GetComponentInChildren<Text>().text = "Create ShellCore!";
    }

    public void PromptDelete(int index)
    {
        indexToDelete = index;
        deletePrompt.ToggleActive();
    }

    public void PromptMigrate(int index)
    {
        indexToMigrate = index;
        switch (saves[index].version)
        {
            case "Beta 2.1.0":
                migratePrompt.transform.Find("Background").GetComponentInChildren<Text>().text = "This will remove old EP3 mission data and place you in the Spawning Grounds. "
                                                                                                 + "Backup first! (Below save icon delete button)";
                break;
            case "Beta 1.0.0":
                migratePrompt.transform.Find("Background").GetComponentInChildren<Text>().text = "This will move your presets to the new dedicated folder the game uses. "
                                                                                                 + "Backup first! (Below save icon delete button)";
                break;
            case "Beta 0.1.1":
                migratePrompt.transform.Find("Background").GetComponentInChildren<Text>().text = "This will fix Trial by Combat's mission name and a couple of others as well. "
                                                                                                 + "Backup first! (Below save icon delete button)";
                break;
            case "Alpha 2.1.0":
                migratePrompt.transform.Find("Background").GetComponentInChildren<Text>().text = "This will reset your task progress, reputation and place you in the "
                                                                                                 + "Spawning Grounds. Backup first! (Below save icon delete button)";
                break;
            case "Beta 0.0.0":
                migratePrompt.transform.Find("Background").GetComponentInChildren<Text>().text = "This will change some data related to Truthful Revelation. Backup first! (Below save icon delete button)";
                break;
            default:
                migratePrompt.transform.Find("Background").GetComponentInChildren<Text>().text = "This will simply add Sukrat to your party list. Regardless, backup first! (Below save icon delete button)";
                break;
        }
        migratePrompt.ToggleActive();
    }

    private void ChangeMissionName(PlayerSave save, string oldName, string newName)
    {
        var mission = save.missions.Find(m => m.name == oldName);
        if (mission == null) return;
        mission.name = newName;
        if (mission.checkpoint != null && mission.checkpoint.Contains(oldName))
        {
            int oldIndex = mission.checkpoint.IndexOf(oldName);
            mission.checkpoint = mission.checkpoint.Remove(oldIndex, oldName.Length);
            mission.checkpoint = mission.checkpoint.Insert(oldIndex, newName);
        }

        foreach (Mission prereqMission in save.missions)
        {
            if (!prereqMission.prerequisites.Contains(oldName)) continue;
            prereqMission.prerequisites.Remove(oldName);
            prereqMission.prerequisites.Add(newName);
        }
    }

    public void Migrate()
    {
        var save = saves[indexToMigrate];
        switch (save.version)
        {
            case "Beta 2.0.0":
            case "Beta 2.0.1":
            case "Beta 2.1.0":
                var missionsNames = new string[] 
                {
                    "Abandonment", 
                    "Awakening the Holy Citadel", 
                    "Derelict Vanquish", 
                    "Forsaken Declaration", 
                    "Gunning Triumph",
                    "The Stronghold",
                    "Reclamation"
                };
                var missionsToRemove = save.missions.Where(m => missionsNames.Contains(m.name)).ToArray();
                save.lastDimension = 0;
                save.position = new Vector2(260, -145);
                foreach (var m in missionsToRemove)
                {
                    save.missions.Remove(m);
                }
                File.WriteAllText(paths[indexToMigrate], JsonUtility.ToJson(save));
                SaveMenuIcon.LoadSaveByPath(paths[indexToMigrate], true);
                break;                


            case "Beta 1.0.0":
                int presetNum = 0;
                if (WCWorldIO.PRESET_DIRECTORY == null) WCWorldIO.PRESET_DIRECTORY = System.IO.Path.Combine(Application.persistentDataPath, "PresetBlueprints");
                if (!Directory.Exists(WCWorldIO.PRESET_DIRECTORY))
                {
                    Directory.CreateDirectory(WCWorldIO.PRESET_DIRECTORY);
                }
                foreach(var preset in save.presetBlueprints)
                {
                    presetNum++;
                    var path = System.IO.Path.Combine(WCWorldIO.PRESET_DIRECTORY, $"{save.name} - Preset {presetNum}.json");
                    int x = 1;
                    if (File.Exists(path))
                    {
                        while (File.Exists(System.IO.Path.Combine(WCWorldIO.PRESET_DIRECTORY, $"{save.name} - Preset {presetNum} + ({x}).json")))
                        {
                            x++;
                        }
                        path = System.IO.Path.Combine(WCWorldIO.PRESET_DIRECTORY, $"{save.name} - Preset {presetNum} + ({x}).json");
                    }
                    if (!string.IsNullOrEmpty(preset))
                    ShipBuilder.SaveBlueprint(null, path, preset);
                }
                SaveMenuIcon.LoadSaveByPath(paths[indexToMigrate], true);
                break;
            case "Beta 0.1.1":
                ChangeMissionName(save, "The Carrier Conundrum", "Carrier Conundrum");
                ChangeMissionName(save, "The Turret Turmoil", "Turret Turmoil");
                ChangeMissionName(save, "Trial By Combat", "Trial by Combat");
                File.WriteAllText(paths[indexToMigrate], JsonUtility.ToJson(save));
                SaveMenuIcon.LoadSaveByPath(paths[indexToMigrate], true);
                break;
            case "Beta 0.0.0":
                var mission = save.missions.Find(m => m.name == "Truthful Revelation?");
                if (mission != null) mission.name = "Truthful Revelation";
                File.WriteAllText(paths[indexToMigrate], JsonUtility.ToJson(save));
                SaveMenuIcon.LoadSaveByPath(paths[indexToMigrate], true);
                break;
            case "Alpha 4.0.0":
            case "Alpha 4.1.0":
            case "Alpha 4.1.1":
            case "Alpha 4.2.0":
            case "Alpha 4.3.0":
                // attempt to add Sukrat to the party list
                if (!save.unlockedPartyIDs.Contains("sukrat"))
                {
                    save.unlockedPartyIDs.Add("sukrat");
                }

                save.version = VersionNumberScript.version;
                for (int i = 0; i < save.characters.Length; i++)
                {
                    if (save.characters[i].ID == "sukrat")
                    {
                        var party = save.characters[i].partyData = new WorldData.PartyData();
                        party.attackDialogue = "DESTRUCTION!";
                        party.defendDialogue = "Falling back!";
                        party.collectDialogue = "I'm on it.";
                        party.buildDialogue = "Building!";
                        party.followDialogue = "Following!";
                    }
                }

                File.WriteAllText(paths[indexToMigrate], JsonUtility.ToJson(save));
                SaveMenuIcon.LoadSaveByPath(paths[indexToMigrate], true);
                break;
            case "Alpha 1.0.0":
            case "Alpha 2.0.0":
            case "Alpha 2.1.0":
                save.version = VersionNumberScript.version;
                save.reputation = 0;
                migratedTimePlayed = save.timePlayed;
                save.timePlayed = 0;
                File.WriteAllText(paths[indexToMigrate], JsonUtility.ToJson(save));
                SaveMenuIcon.LoadSaveByPath(paths[indexToMigrate], true);
                break;
        }
    }

    public void DeleteSave()
    {
        DeleteSaveAtIndex(indexToDelete);
        deletePrompt.ToggleActive();
    }

    public void DeleteSaveAtIndex(int indexToDelete)
    {
        saves.RemoveAt(indexToDelete);
        File.Delete(paths[indexToDelete]);
        paths.RemoveAt(indexToDelete);
        Destroy(icons[indexToDelete].gameObject);
        icons.RemoveAt(indexToDelete);
        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].index = i;
        }
    }

    public void AddSave()
    {
        string name = inputField.text.Trim();
        if (name == "") 
        {
            inputFeedback.text = "The save name can't be empty.";
            return;
        }
        if (name == "TestSave")
        {
            inputFeedback.text = "The name \"TestSave\" is reserved for world creator functionality.";
            return;
        }
        if (name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) > -1)
        {
            inputFeedback.text = "The save name contains invalid characters.";
            return;
        }
        string path = System.IO.Path.Combine(Application.persistentDataPath, "Saves", name);
        if (paths.Contains(path))
        {
            inputFeedback.text = "The save name already exists.";
            return;
        }

        inputField.transform.parent.GetComponentInChildren<GUIWindowScripts>().ToggleActive();

        var save = CreateSave(name, null, this.resourcePath);

        if (save.resourcePath == "")
        {
            save.resourcePath = "main";
        }

        saves.Add(save);
        paths.Add(path);

        SaveMenuIcon icon = Instantiate(saveIconPrefab, contents).GetComponent<SaveMenuIcon>();
        icon.save = save;
        icon.path = path;
        icon.index = icons.Count;
        icon.handler = this;
        icons.Add(icon);
        icon.transform.SetAsFirstSibling();
    }

    public void BackupSave(PlayerSave save)
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "Saves", save.name + " - Backup ");
        int i = 1;
        while (File.Exists(path + i))
        {
            i++;
        }

        path = path + i;
        File.WriteAllText(path, JsonUtility.ToJson(save));

        saves.Add(save);
        paths.Add(path);

        SaveMenuIcon icon = Instantiate(saveIconPrefab, contents).GetComponent<SaveMenuIcon>();
        icon.save = save;
        icon.path = path;
        icon.index = icons.Count;
        icon.handler = this;
        icons.Add(icon);
        icon.transform.SetSiblingIndex(icon.transform.parent.childCount - 2);
    }

    public static PlayerSave CreateSave(string name, string checkpointName = null, string resourcePath = "")
    {
        string currentVersion = VersionNumberScript.version;
        PlayerSave save = new PlayerSave();
        save.name = name;
        save.timePlayed = 0;
        save.presetBlueprints = new string[5];
        save.currentHealths = new float[] { 1000, 250, 500 };
        save.partInventory = new List<EntityBlueprint.PartInfo>();
        save.sectorsSeen = new List<string>();
        save.missions = new List<Mission>();

        EntityBlueprint blueprint = SaveHandler.GetDefaultBlueprint();
        save.currentPlayerBlueprint = JsonUtility.ToJson(blueprint);
        save.abilityCaps = CoreUpgraderScript.minAbilityCap;
        save.shards = 0;
        save.version = currentVersion;
        save.resourcePath = resourcePath;
        save.abilityHotkeys = new AbilityHotkeyStruct();

        var savesDir = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        Directory.CreateDirectory(savesDir);
        File.WriteAllText(System.IO.Path.Combine(savesDir, name), JsonUtility.ToJson(save));

        return save;
    }
}
