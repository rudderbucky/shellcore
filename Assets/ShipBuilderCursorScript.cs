using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;


public class ShipBuilderCursorScript : MonoBehaviour {

	public List<ShipBuilderPart> parts = new List<ShipBuilderPart>();
	public RectTransform grid;
	ShipBuilderPart currentPart;
	public ShipBuilderPart lastPart;
	public ShipBuilder builder;
	public void GrabPart(ShipBuilderPart part) {
		lastPart = null;
		if(parts.Contains(part)) {
			parts.Remove(part);
			parts.Add(part);
			part.rectTransform.SetAsLastSibling();
		}
		currentPart = part;
	}
	void PlaceCurrentPart() {
		//builder.parts.Add(currentPart.info);
		if(!RectTransformUtility.RectangleContainsScreenPoint(grid, Input.mousePosition)) {
			builder.DispatchPart(currentPart);
		} else lastPart = currentPart;
		currentPart = null;
	}

	void ClearAllParts() {
		while(parts.Count > 0) {
			builder.DispatchPart(parts[0]);
		}
	}
	public bool rotateMode;
	public void RotateLastPart() {
		var x = Input.mousePosition - lastPart.transform.position;
		var y = new Vector3(0,0,(Mathf.Rad2Deg * Mathf.Atan(x.y/x.x) -(x.x >= 0 ? 90 : -90)));
		if(!float.IsNaN(y.z))
		{
			y.z = 15 * (Mathf.RoundToInt(y.z / 15));
			lastPart.info.rotation = y.z;
		}
		else lastPart.info.rotation = 0;
			return;
	}
	public void FlipLastPart() {
		lastPart.info.mirrored = !lastPart.info.mirrored;
	}
	void Update() {
		foreach(ShipBuilderPart part in parts) {
			part.isInChain = builder.IsInChain(part);
		}
		if(Input.GetKeyDown("c")) {
			ClearAllParts();
		}
		transform.position = new Vector3(10 * ((int)Input.mousePosition.x / 10), 10 * ((int)Input.mousePosition.y / 10), 0);
		if(rotateMode) {
			RotateLastPart();
			return;
		}
		if(currentPart) {
			currentPart.info.location = GetComponent<RectTransform>().anchoredPosition / 100;
			if(Input.GetMouseButtonUp(0)) {
				PlaceCurrentPart();
			}
		} else if(Input.GetMouseButtonDown(0)) {
			for(int i = parts.Count - 1; i >= 0; i--) {
				if(RectTransformUtility.RectangleContainsScreenPoint(parts[i].rectTransform, transform.position)) {
					GrabPart(parts[i]);
					break;
				}
			}
		}
	}
}

