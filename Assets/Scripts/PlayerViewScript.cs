using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewScript : MonoBehaviour {

	private Stack<IWindow> currentWindow;
	private static PlayerViewScript instance;

	public static void SetCurrentWindow(IWindow window) {
		instance.currentWindow.Push(window);
	}
	public GameObject escapeMenu;
	// Update is called once per frame
	void Awake() {
		currentWindow = new Stack<IWindow>();
		instance = this;
		if(escapeMenu) escapeMenu.SetActive(false);
	}
	void Update () {
		if(Input.GetButtonUp("Cancel")) { // for some reason this is escape
			while(currentWindow.Count > 0) {
				bool shouldReturn = (currentWindow.Peek() as MonoBehaviour).gameObject.activeSelf;
				
				if(shouldReturn) {
					currentWindow.Pop().CloseUI();
					return;
				} else currentWindow.Pop();
			}
			if(escapeMenu) escapeMenu.SetActive(!escapeMenu.activeSelf); // toggle
			if(transform.Find("Settings")) transform.Find("Settings").gameObject.SetActive(false);
		}
	}
}
