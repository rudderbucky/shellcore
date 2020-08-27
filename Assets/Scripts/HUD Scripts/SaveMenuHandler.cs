using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class SaveMenuHandler : GUIWindowScripts {

	List<PlayerSave> saves;
	List<string> paths;
	public Transform contents;
	public GameObject saveIconPrefab;
	List<SaveMenuIcon> icons;
	public InputField inputField;
	int indexToDelete;
	int indexToMigrate;
	public GUIWindowScripts deletePrompt;
	public GUIWindowScripts migratePrompt;
	public static float? migratedTimePlayed = null;
	public static List<string> migrationVersions = new List<string>() 
	{
		"Alpha 1.0.0",
		"Alpha 2.0.0",
		"Alpha 2.1.0"
	};
	void Awake() {
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

	void Initialize() {
		string curpath = null;
		if(File.Exists(Application.persistentDataPath + "\\CurrentSavePath")) 
			curpath = File.ReadAllText(Application.persistentDataPath + "\\CurrentSavePath");
		for(int i = 0; i < saves.Count; i++) {
			SaveMenuIcon icon = Instantiate(saveIconPrefab, contents).GetComponent<SaveMenuIcon>();
			icon.save = saves[i];
			icon.index = i;
			icon.handler = this;
			icon.path = paths[i];
			icons.Add(icon);
			if(icon.path == curpath || i == 0) icon.transform.SetAsFirstSibling();
			else icon.transform.SetSiblingIndex(1);
		}
	}

	public override void Activate() {
		base.Activate();
		Initialize();
	}

	public override void CloseUI() {
		foreach(SaveMenuIcon icon in icons) {
			Destroy(icon.gameObject);
		}
		icons.Clear();
		base.CloseUI();
	}

	public void OpenSavePrompt() {
		inputField.transform.parent.GetComponentInChildren<GUIWindowScripts>().ToggleActive();
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
				migratePrompt.ToggleActive();
				break;
			default:
				break;
		}
	}

	public void Migrate()
	{
		var save = saves[indexToMigrate];
		save.version = VersionNumberScript.version;
		save.reputation = 0;
		migratedTimePlayed = save.timePlayed;
		save.timePlayed = 0;
		File.WriteAllText(paths[indexToMigrate], JsonUtility.ToJson(save));
		SaveMenuIcon.LoadSaveByPath(paths[indexToMigrate], true);
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

		var save = CreateSave(name);

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


	public static PlayerSave CreateSave(string name, string checkpointName = null)
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
		File.WriteAllText(Application.persistentDataPath + "\\Saves" + "\\" + name, JsonUtility.ToJson(save));
		return save;
	} 
}
