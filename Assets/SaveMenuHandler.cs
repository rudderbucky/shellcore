using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class SaveMenuHandler : MonoBehaviour, IWindow {

	// TODO: save timePlayed functionality
	List<PlayerSave> saves;
	List<string> paths;
	public Transform contents;
	public GameObject saveIconPrefab;
	List<SaveMenuIcon> icons;
	public InputField inputField;
	int indexToDelete;
	public GUIWindowScripts deletePrompt;

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
				try 
				{
					string savejson = File.ReadAllText(file);
					PlayerSave save = JsonUtility.FromJson<PlayerSave>(savejson);
					saves.Add(save);
					paths.Add(file);
					
				} catch(System.Exception) 
				{
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

	public void OpenUI() {
		PlayerViewScript.SetCurrentWindow(this);
		gameObject.SetActive(true);
		Initialize();
	}
	public void CloseUI() {
		ResourceManager.PlayClipByID("clip_back");
		gameObject.SetActive(false);
		foreach(SaveMenuIcon icon in icons) {
			Destroy(icon.gameObject);
		}
		icons.Clear();
	}

	public void OpenSavePrompt() {
		inputField.transform.parent.GetComponent<GUIWindowScripts>().ToggleActive();
	}

	public void PromptDelete(int index) {
		indexToDelete = index;
		deletePrompt.ToggleActive();
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
		string currentVersion = "Prototype 2.1.0";
		string name = inputField.text;
		string path = Application.persistentDataPath + "\\Saves" + "\\" + name;
		inputField.transform.parent.gameObject.SetActive(false);
		if(name == "" || paths.Contains(path)) return;
		PlayerSave save = new PlayerSave();
		save.name = name;
		save.timePlayed = 0;
		save.presetBlueprints = new string[5];
		save.currentHealths = new float[] {1000,250,500};
		save.partInventory = new List<EntityBlueprint.PartInfo>();

		EntityBlueprint blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
		blueprint.name = "Player Save Blueprint";
		blueprint.baseRegen = new float[] {60,0,60};
		blueprint.shellHealth = new float[] {1000,250,500};
		blueprint.parts = new List<EntityBlueprint.PartInfo>();
		blueprint.coreSpriteID = "core1_light";
		blueprint.coreShellSpriteID = "core3_shell";
		save.currentPlayerBlueprint = JsonUtility.ToJson(blueprint);
		save.version = currentVersion;
		saves.Add(save);
		paths.Add(path);

		SaveMenuIcon icon = Instantiate(saveIconPrefab, contents).GetComponent<SaveMenuIcon>();
		icon.save = save;
		icon.path = path;
		icon.index = icons.Count;
		icon.handler = this;
		icons.Add(icon);
		icon.transform.SetAsFirstSibling();
		File.WriteAllText(path, JsonUtility.ToJson(save));
	}

	public bool GetActive() {
		return gameObject.activeSelf;
	}
}
