using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SaveMenuIcon : MonoBehaviour {

	public PlayerSave save;
	public string path;
	public Text saveName;
	public Text version;
	public Text timePlayed;

	void Start() {
		saveName.text = save.name;
		version.text = "Current Version: Prototype v2.0.0";
		timePlayed.text = "Time Played: " + save.timePlayed;
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
}
