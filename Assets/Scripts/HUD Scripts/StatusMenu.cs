using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusMenu : GUIWindowScripts {
	public PlayerCore player;
	public Text playerName;
	bool toggle = false;
	public static Task[] taskInfo = new Task[0];
	public override void CloseUI()
	{
		ResourceManager.PlayClipByID("clip_back");
		for(int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(0).gameObject.SetActive(false);
		}
		toggle = false;
	}
	void Initialize() {
		ResourceManager.PlayClipByID("clip_select");
		PlayerViewScript.SetCurrentWindow(this);
		GetComponentInParent<Canvas>().sortingOrder = ++PlayerViewScript.currentLayer;
		playerName.text = "<color=yellow>" + player.name + "</color>";
		base.Activate();

	}
	// Update is called once per frame
	protected override void Update () 
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
				transform.GetChild(0).gameObject.SetActive(toggle);
			}
		}
		base.Update();
	}

	public override bool GetActive() {
		return toggle;
	}
}
