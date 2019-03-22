using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewScript : MonoBehaviour {

	private Stack<IWindow> currentWindow;
	private static PlayerViewScript instance;
	public static int currentLayer = 0;

	public static void SetCurrentWindow(IWindow window) {
		instance.currentWindow.Push(window);
	}
	public GameObject escapeMenu;
	Canvas escapeCanvas;
	// Update is called once per frame
	void Awake() {
		if(escapeMenu) escapeCanvas = escapeMenu.GetComponentInChildren<Canvas>();
		Debug.Log(Application.persistentDataPath);
		currentWindow = new Stack<IWindow>();
		instance = this;
		if(escapeMenu) escapeMenu.SetActive(false);
	}
	void Update () {
		if(escapeMenu) escapeCanvas.sortingOrder = currentLayer + 1;
		if(Input.GetButtonUp("Cancel")) { // for some reason this is escape
			while(currentWindow.Count > 0) {
				if(escapeMenu && escapeMenu.activeSelf) {
					escapeMenu.SetActive(false);
					return;
				}
				bool shouldReturn = currentWindow.Peek().Equals(null) ? false : currentWindow.Peek().GetActive();
				
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
