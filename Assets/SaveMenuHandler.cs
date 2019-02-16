using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveMenuHandler : MonoBehaviour, IWindow {

	// TODO: save timePlayed functionality
	List<PlayerSave> saves;
	List<string> paths;
	public Transform contents;
	public GameObject saveIconPrefab;
	List<SaveMenuIcon> icons;

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
		for(int i = 0; i < saves.Count; i++) {
			SaveMenuIcon icon = Instantiate(saveIconPrefab, contents).GetComponent<SaveMenuIcon>();
			icon.save = saves[i];
			icon.path = paths[i];
			icons.Add(icon);
			icon.transform.SetAsFirstSibling();
		}
	}

	public void OpenUI() {
		PlayerViewScript.SetCurrentWindow(this);
		gameObject.SetActive(true);
		Initialize();
	}
	public void CloseUI() {
		gameObject.SetActive(false);
		foreach(SaveMenuIcon icon in icons) {
			Destroy(icon.gameObject);
		}
		icons.Clear();
	}

	public void AddSave(string name) {
		string path = Application.persistentDataPath + "\\Saves" + "\\" + name;;
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
		blueprint.coreShellSpriteID = "core1_shell";
		save.currentPlayerBlueprint = JsonUtility.ToJson(blueprint);
		saves.Add(save);
		paths.Add(path);

		SaveMenuIcon icon = Instantiate(saveIconPrefab, contents).GetComponent<SaveMenuIcon>();
		icon.save = save;
		icon.path = path;
		icons.Add(icon);
		icon.transform.SetAsFirstSibling();
		File.WriteAllText(path, JsonUtility.ToJson(save));
	}
}
