using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveHandler : MonoBehaviour {

	public PlayerCore player;
	PlayerSave save;
	// Use this for initialization
	void Awake() {
		if(File.Exists(Application.persistentDataPath + "\\TestSave")) {
			string json = File.ReadAllText(Application.persistentDataPath + "\\TestSave");
			save = JsonUtility.FromJson<PlayerSave>(json);
			Debug.Log(save.currentPlayerBlueprint);
			if(save.currentPlayerBlueprint != null && save.currentPlayerBlueprint != "") JsonUtility.FromJsonOverwrite(save.currentPlayerBlueprint, player.blueprint);
			player.cursave = save;
		} else save = new PlayerSave();
	}
	
	public void Save() {
		save.position = player.transform.position;
		save.currentHealths = player.currentHealth;
		if(player.currentHealth[1] <= 0) save.currentHealths = player.GetMaxHealth();
		save.name = "Test Save";
		save.currentPlayerBlueprint = JsonUtility.ToJson(player.blueprint);
		string saveJson = JsonUtility.ToJson(save);
		File.WriteAllText(Application.persistentDataPath + "\\TestSave", saveJson);
	}
}
