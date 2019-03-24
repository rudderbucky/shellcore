using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWindow {
	void CloseUI();
	bool GetActive();
}
public class GUIWindowScripts : MonoBehaviour, IWindow {

	public void CloseUI() {
		ResourceManager.PlayClipByID("clip_back");
		gameObject.SetActive(false);
	}

	public void ToggleActive() {
		bool active = gameObject.activeSelf;
		if(active) CloseUI();
		else {
			gameObject.SetActive(true);
			GetComponent<Canvas>().sortingOrder = ++PlayerViewScript.currentLayer; // move window to top
			PlayerViewScript.SetCurrentWindow(this);
		}
	}

	public bool GetActive() {
		return gameObject ? gameObject.activeSelf : false;
	}
}
