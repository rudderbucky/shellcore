using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewScript : MonoBehaviour {

	public GameObject escapeMenu;
	// Update is called once per frame
	void Start() {
		escapeMenu.SetActive(false);
	}
	void Update () {
		if(Input.GetButtonUp("Cancel")) { // for some reason this is escape
			escapeMenu.SetActive(!escapeMenu.activeSelf); // toggle
			if(transform.Find("Settings")) transform.Find("Settings").gameObject.SetActive(false);
		}
	}
}
