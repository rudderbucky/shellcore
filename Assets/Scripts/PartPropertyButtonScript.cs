using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;


public class PartPropertyButtonScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
	public ShipBuilderCursorScript cursor;
	public enum ButtonType {
		Flip,
		Rotate
	}

	public ButtonType type;

    public void OnPointerDown(PointerEventData eventData)
    {
		if((int)type == 0) cursor.FlipLastPart();
        if((int)type == 1) cursor.rotateMode = true;
    }

	public void OnPointerUp(PointerEventData eventData)
    {
        if((int)type == 1) {
			cursor.rotateMode = false;
		}
    }
    void Update() {
		if(cursor.GetLastInfo() != null) {
			GetComponent<Image>().enabled = true;
			var tmp = ((EntityBlueprint.PartInfo)cursor.GetLastInfo()).location * 100;
			tmp.x += ((int)type == 1 ? 25 : -25);
			tmp.y += 100;
			((RectTransform)transform).anchoredPosition = tmp;
		} else GetComponent<Image>().enabled = false;
	}
}
