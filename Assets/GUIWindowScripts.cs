using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIWindowScripts : MonoBehaviour {

	public void ToggleActive() {
		bool active = gameObject.activeSelf;
		gameObject.SetActive(!active);
	}
}
