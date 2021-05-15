using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class SaveMenuHandler : GUIWindowScripts {

	List<PlayerSave> saves;
	List<string> paths;
	public Transform worldContents;
	public Transform contents;
	public GameObject worldIconPrefab;
	public GameObject saveIconPrefab;
	List<Button> worldButtons;
	List<SaveMenuIcon> icons;
	public InputField inputField;
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
	};

	// Use to check before world load if the format is correct. Helps prevent loading incorrectly installed worlds
	private bool CheckWorldValidity(string dir)
	{
		// check the 3 main folders are in the world
		bool foundEntities = false;
		bool foundCanvases = false;
		bool foundWaves = false;

		foreach(var childDir in System.IO.Directory.GetDirectories(dir))
		{
			if(childDir.Contains("Entities")) foundEntities = true;
			if(childDir.Contains("Canvases")) foundCanvases = true;
			if(childDir.Contains("Waves")) foundWaves = true;
		}

		return foundCanvases && foundEntities && foundWaves;
	}

	void SetUpWorlds()
	{
		worldButtons = new List<Button>();
		selectIndicatorText.text = "SELECT WORLD";
		string[] directories = Directory.GetDirectories(Application.streamingAssetsPath + "\\Sectors");
		foreach(var dir in directories)
        {
			if(dir.Contains("main") && !dir.Contains(VersionNumberScript.mapVersion))
				continue;
			string xdir = dir;
			Button worldButton = Instantiate(worldIconPrefab, worldContents).GetComponent<Button>();
			worldButtons.Add(worldButton);
			worldButton.GetComponentInChildren<Text>().text = dir.Contains("main") ? "Main world" : System.IO.Path.GetFileName(dir);

			if(!CheckWorldValidity(dir))
			{
				worldButton.GetComponentInChildren<Text>().color = Color.red;
				worldButton.GetComponentInChildren<Text>().text += " - World may be incorrectly installed";
			}

			worldButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
				this.resourcePath = dir.Contains("main") ? "" : xdir;
				worldView.SetActive(false);
				InitializeSaves();
			}));
			if(dir.Contains("main"))
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
		string path = Application.persistentDataPath + "\\Saves";
		if(!Directory.Exists(path)) Directory.CreateDirectory(path);
		else 
		{
			string[] files = Directory.GetFiles(path);
			foreach(string file in files) 
			{
				if(file.Contains("TestSave")) continue;
				try 
				{
					string savejson = File.ReadAllText(file);
					PlayerSave save = JsonUtility.FromJson<PlayerSave>(savejson);
					saves.Add(save);
					paths.Add(file);
					
				} catch(System.Exception e) 
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
		
		if(File.Exists(Application.persistentDataPath + "\\CurrentSavePath")) 
			curpath = File.ReadAllText(Application.persistentDataPath + "\\CurrentSavePath");
		for(int i = 0; i < saves.Count; i++) {
			if(saves[i] == null) 
				continue;
			SaveMenuIcon icon = Instantiate(saveIconPrefab, contents).GetComponent<SaveMenuIcon>();
			icon.save = saves[i];
			icon.index = i;
			icon.handler = this;
			icon.path = paths[i];
			icons.Add(icon);
			if(resourcePath == "")
			{
				if(icon.path == curpath || i == 0) icon.transform.SetAsFirstSibling();
				else icon.transform.SetSiblingIndex(1);
			}
			else icon.transform.SetSiblingIndex(0);
			if((resourcePath == "" && saves[i].resourcePath != null && !saves[i].resourcePath.Contains("main") && saves[i].resourcePath != "") 
				|| (resourcePath != "" && saves[i].resourcePath != resourcePath)) icon.gameObject.SetActive(false);
		}
	}

	void Awake() {
		instance = this;
	}

	void Initialize(string resourcePath = "") {
		this.resourcePath = resourcePath;
		worldView.SetActive(true);
		saveView.SetActive(false);
		SetUpWorlds();
		SetUpSaves();
		
	}

	public override void Activate() {
		base.Activate();
		Initialize();
	}

	public void Activate(string resourcePath = "") {
		base.Activate();
		Initialize(resourcePath);
	}

	public override void CloseUI() {
		foreach(SaveMenuIcon icon in icons) {
			Destroy(icon.gameObject);
		}
		foreach(var worldButton in worldButtons)
		{
			Destroy(worldButton.gameObject);
		}
		icons.Clear();
		worldButtons.Clear();
		base.CloseUI();
	}

	public void OpenSavePrompt() {
		inputField.transform.parent.GetComponentInChildren<GUIWindowScripts>().ToggleActive();
		inputField.transform.parent.Find("Background").GetComponentInChildren<Text>().text = "Name your ShellCore:\n" +
		"(Warning: closing this box, entering nothing, or entering a name already in use by another save will terminate this process.)";
		inputField.transform.parent.Find("Create Save").GetComponentInChildren<Text>().text = "Create ShellCore!";
	}

	public void PromptDelete(int index) {
		indexToDelete = index;
		deletePrompt.ToggleActive();
	}

	public void PromptMigrate(int index) {
		switch(saves[index].version)
		{
			case "Alpha 2.1.0":
				indexToMigrate = index;
				migratePrompt.transform.Find("Background").GetComponentInChildren<Text>().text = "This will reset your task progress, reputation and place you in the "
					+ "Spawning Grounds. Backup first! (Below save icon delete button)";
				migratePrompt.ToggleActive();
				break;
			default:
				indexToMigrate = index;
				migratePrompt.transform.Find("Background").GetComponentInChildren<Text>().text = "This will simply add Sukrat to your party list. Regardless, backup first! (Below save icon delete button)";
				migratePrompt.ToggleActive();
				break;
		}
	}

	public void Migrate()
	{
		var save = saves[indexToMigrate];
		switch(save.version)
		{
			case "Alpha 4.0.0":
			case "Alpha 4.1.0":
			case "Alpha 4.1.1":
			case "Alpha 4.2.0":
			case "Alpha 4.3.0":
				// attempt to add Sukrat to the party list
				if(!save.unlockedPartyIDs.Contains("sukrat"))
					save.unlockedPartyIDs.Add("sukrat");
				save.version = VersionNumberScript.version;
				for(int i = 0; i < save.characters.Length; i++)
				{
					if(save.characters[i].ID == "sukrat")
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
		saves.RemoveAt(indexToDelete);
		File.Delete(paths[indexToDelete]);
		paths.RemoveAt(indexToDelete);
		Destroy(icons[indexToDelete].gameObject);
		icons.RemoveAt(indexToDelete);
		deletePrompt.ToggleActive();
		for(int i = 0; i < icons.Count; i++) {
			icons[i].index = i;
		}
	}

	public void AddSave() {
		string name = inputField.text.Trim();
		string path = Application.persistentDataPath + "\\Saves" + "\\" + name;
		inputField.transform.parent.GetComponentInChildren<GUIWindowScripts>().ToggleActive();
		if(name == "" || name == "TestSave" ||
			paths.Contains(path) || name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) > -1) return;

		Debug.Log(this.resourcePath);
		var save = CreateSave(name, null, this.resourcePath);

		if(save.resourcePath == "") save.resourcePath = "main";
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
		string origPath = Application.persistentDataPath + "\\Saves\\" + save.name + " - Backup";
		string path = origPath + " ";
		int i = 1;
		while(File.Exists(path + i))
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
		save.currentHealths = new float[] {1000,250,500};
		save.partInventory = new List<EntityBlueprint.PartInfo>();
		save.sectorsSeen = new List<string>();
		save.missions = new List<Mission>();

		// this section contains default information for a new save. Edit this to change how the default save
		// is created.
		EntityBlueprint blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
		blueprint.name = "Player Save Blueprint";
		blueprint.baseRegen = new float[] {60,0,60};
		blueprint.shellHealth = new float[] {1000,250,500};
		blueprint.parts = new List<EntityBlueprint.PartInfo>();
		blueprint.coreSpriteID = "core1_light";
		blueprint.coreShellSpriteID = "core1_shell";
		save.currentPlayerBlueprint = JsonUtility.ToJson(blueprint);
		save.abilityCaps = new int[] {10, 3, 10, 10};
		save.shards = 0;
		save.version = currentVersion;
		save.resourcePath = resourcePath;
		save.abilityHotkeys = new AbilityHotkeyStruct();
		File.WriteAllText(Application.persistentDataPath + "\\Saves" + "\\" + name, JsonUtility.ToJson(save));
		return save;
	} 
}
