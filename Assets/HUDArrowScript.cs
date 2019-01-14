using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HUDArrowScript : MonoBehaviour {

	public PlayerCore player;
	private SpriteRenderer spr;
	public void Initialize(PlayerCore player) {
		spr = GetComponent<SpriteRenderer>();
		this.player = player;
	}
	
	// Update is called once per frame
	void Update () {
		if(player) {
			if(player.GetTargetingSystem().GetTarget()) {
				Vector3 targpos = player.GetTargetingSystem().GetTarget().position;
				Vector3 viewpos = Camera.main.WorldToViewportPoint(targpos);
				if(viewpos.x > 1 || viewpos.x < 0 || viewpos.y < 0 || viewpos.y > 1) {
					spr.enabled = true;
					spr.sortingOrder = player.GetComponent<SpriteRenderer>().sortingOrder;
					var x = (-player.transform.position + targpos);
					x.z = 0;
					float magcheck = Mathf.Max(viewpos.x, 1- viewpos.x, viewpos.y, 1- viewpos.y);
					transform.localScale = new Vector3(1 / magcheck,1 / magcheck,1);
					if(player.IsMoving()) {
						transform.position = player.transform.position + x.normalized * 10;
						transform.eulerAngles = new Vector3(0,0,(Mathf.Rad2Deg * Mathf.Atan(x.y/x.x) -(x.x > 0 ? 90 : -90)));
					}
				} else spr.enabled = false;
			} else spr.enabled = false;
		}
	}
}
