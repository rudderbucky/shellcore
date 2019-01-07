using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecCrCamera : MonoBehaviour {

	public SectorCreatorMouse mouse;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(!mouse.windowEnabled) {
			Vector3 mousePos = Input.mousePosition;
			mousePos.z += 10;
			mousePos = Camera.main.ScreenToWorldPoint(mousePos);
			Vector3 pos = Camera.main.WorldToViewportPoint(mousePos);
			if(pos.x < 0.05 && pos.y < 0.05) transform.position -= 0.8F * Mathf.Sqrt(2) * (Vector3)Vector2.one;
			else {
				if(pos.y < 0.05) transform.position += 0.8F * Vector3.down;
				if(pos.x < 0.05) transform.position += 0.8F * Vector3.left;
			}
			if(pos.x > 0.95 && pos.y > 0.95) transform.position += 0.8F * Mathf.Sqrt(2) * (Vector3)Vector2.one;
			else {
				if(0.95 < pos.x) transform.position += 0.8F * Vector3.right;
				if(0.95 < pos.y) transform.position += 0.8F * Vector3.up;
			}

			transform.position += Input.GetAxis("Horizontal") * Vector3.right;
			transform.position += Input.GetAxis("Vertical") * Vector3.up;

			if(Input.GetKey("space")) {
				Vector3 center = mouse.center;
				center.z -= 10;
				transform.position = center;
			}
		}
	}
}
