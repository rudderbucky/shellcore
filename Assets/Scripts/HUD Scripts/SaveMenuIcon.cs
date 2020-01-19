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
	public Image shellImage;
	public Image coreImage;


	void Start() {
		EntityBlueprint print = ScriptableObject.CreateInstance<EntityBlueprint>();
		JsonUtility.FromJsonOverwrite(save.currentPlayerBlueprint, print);
		shellImage.sprite = ResourceManager.GetAsset<Sprite>(print.coreShellSpriteID);
		shellImage.rectTransform.sizeDelta = shellImage.sprite.bounds.size * 50;
		coreImage.material = ResourceManager.GetAsset<Material>("material_color_swap");
		coreImage.rectTransform.anchoredPosition = shellImage.sprite.pivot / 2 - shellImage.rectTransform.sizeDelta / 2;
		shellImage.color = coreImage.color = FactionColors.colors[0];
		saveName.text = save.name;
		version.text = "Version: " + save.version;
		if(save.version.Contains("Prototype") || save.version.Contains("Alpha 0.0.0"))
		{
			version.color = Color.red;
			version.text += " - Unsupported Version!";
		}
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
