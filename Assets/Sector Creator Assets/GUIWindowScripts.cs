using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWindow {
	void CloseUI();
}
public class GUIWindowScripts : MonoBehaviour, IWindow {

	public void CloseUI() {
		gameObject.SetActive(false);
	}
	public void ToggleActive() {
		bool active = gameObject.activeSelf;
		gameObject.SetActive(!active);
	}
}
