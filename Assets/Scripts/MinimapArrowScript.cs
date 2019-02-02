using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapArrowScript : MonoBehaviour {

	public Camera minicam;
	PlayerCore player;
	public void Initialize(PlayerCore player) {
		this.player = player;
	}

	void Update() {
		if(player) {
			if(player.GetTargetingSystem().GetTarget()) {
				Transform target = player.GetTargetingSystem().GetTarget();
				Vector3 pos = minicam.WorldToViewportPoint(target.transform.position);
				Vector3 arrowpos = new Vector3(0,0,0);
				bool xlim = false;
				bool ylim = false;
				if(pos.x > 1) {
					arrowpos.x = minicam.ViewportToWorldPoint(new Vector3(1,0,0)).x;
					transform.eulerAngles = new Vector3(0,0,-90);
					xlim = true;
				}
				else if(pos.x < 0) {
					arrowpos.x = minicam.ViewportToWorldPoint(new Vector3(0,0,0)).x;
					transform.eulerAngles = new Vector3(0,0,90);
					xlim = true;
				} else arrowpos.x = target.transform.position.x;
				if(pos.y > 1) {
					arrowpos.y = minicam.ViewportToWorldPoint(new Vector3(0,1,0)).y;
					transform.eulerAngles = new Vector3(0,0,0);
					ylim = true;
				}
				else if(pos.y < 0) {
					arrowpos.y = minicam.ViewportToWorldPoint(new Vector3(0,0,0)).y;
					transform.eulerAngles = new Vector3(0,0,180);
					ylim = true;
				} else arrowpos.y = target.transform.position.y;
				
				if(xlim || ylim) {
					GetComponent<SpriteRenderer>().enabled = true;
					transform.position = arrowpos;
					return;
				}
			}
		}
		GetComponent<SpriteRenderer>().enabled = false;
	}
}
