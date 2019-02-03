using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewScript : MonoBehaviour {

	private IWindow currentWindow;
	private static PlayerViewScript instance;

	public static void SetCurrentWindow(IWindow window) {
		instance.currentWindow = window;
	}
	public GameObject escapeMenu;
	// Update is called once per frame
	void Start() {
		instance = this;
		escapeMenu.SetActive(false);
	}
	void Update () {
		if(Input.GetButtonUp("Cancel")) { // for some reason this is escape
			if(currentWindow != null) {
				bool shouldReturn = (currentWindow as MonoBehaviour).gameObject.activeSelf;
				currentWindow.CloseUI();
				currentWindow = null;
				if(shouldReturn) return;
			}
			escapeMenu.SetActive(!escapeMenu.activeSelf); // toggle
			if(transform.Find("Settings")) transform.Find("Settings").gameObject.SetActive(false);
		}
	}
}
