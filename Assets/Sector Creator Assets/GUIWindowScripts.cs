using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWindow {
	void CloseUI();
	bool GetActive();
}
public class GUIWindowScripts : MonoBehaviour, IWindow {

	public void CloseUI() {
		gameObject.SetActive(false);
	}

	public void ToggleActive() {
		bool active = gameObject.activeSelf;
		gameObject.SetActive(!active);
		if(gameObject.activeSelf) {
			GetComponent<Canvas>().sortingOrder = ++PlayerViewScript.currentLayer; // move window to top
			PlayerViewScript.SetCurrentWindow(this);
		}
	}

	public bool GetActive() {
		return gameObject ? gameObject.activeSelf : false;
	}
}
