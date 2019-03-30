using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusMenu : MonoBehaviour, IWindow {
	public PlayerCore player;
	public Text playerName;
	bool toggle = false;
	public void CloseUI()
	{
		ResourceManager.PlayClipByID("clip_back");
		for(int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.SetActive(false);
		}
		toggle = false;
	}
	void Initialize() {
		ResourceManager.PlayClipByID("clip_select");
		PlayerViewScript.SetCurrentWindow(this);
		GetComponent<Canvas>().sortingOrder = ++PlayerViewScript.currentLayer;
		playerName.text = "<color=yellow>" + player.name + "</color>";

	}
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.E) && !player.GetIsInteracting()) 
		{
			toggle = !toggle;
			if(toggle) 
			{
				Initialize();
			} else ResourceManager.PlayClipByID("clip_back");
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
