using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusMenu : MonoBehaviour, IWindow {
	public PlayerCore player;
	bool toggle = false;
	public void CloseUI()
	{
	for(int i = 0; i < transform.childCount; i++)
	{
		transform.GetChild(i).gameObject.SetActive(false);
	}
	toggle = false;
	}

	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.E) && !player.GetIsInteracting()) 
		{
			toggle = !toggle;
			if(toggle) 
			{
				PlayerViewScript.SetCurrentWindow(this);
				GetComponent<Canvas>().sortingOrder = ++PlayerViewScript.currentLayer;
			}
			for(int i = 0; i < transform.childCount; i++)
			{
				transform.GetChild(i).gameObject.SetActive(toggle);
			}
		}
	}

	public bool GetActive() {
		return toggle;
	}
}
