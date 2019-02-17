using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SaveMenuIcon : MonoBehaviour {

	public SaveMenuHandler handler;
	public PlayerSave save;
	public string path;
	public Text saveName;
	public Text version;
	public Text timePlayed;
	public int index;

	void Start() {
		saveName.text = save.name;
		version.text = "Version: " + save.version;
		timePlayed.text = "Time Played: " + (((int)save.timePlayed / 60 > 0) ? (int)save.timePlayed / 60 + " hours " : "") + (int)save.timePlayed % 60 + " minutes";
	}

	public void LoadSave() {
		string current = Application.persistentDataPath + "\\CurrentSavePath";
		if(!File.Exists(current)) File.WriteAllText(current, path);
		else 
		{
			File.Delete(current);
			File.WriteAllText(current, path);
		}
		MainMenu.StartGame();
	}

	public void DeleteSave() {
		handler.PromptDelete(index);
	}
}
