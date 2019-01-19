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
        if((int)type == 1) cursor.ToggleRotate();
    }

	public void OnPointerUp(PointerEventData eventData)
    {
        if((int)type == 1) {
			cursor.ToggleRotate();
		}
    }
    void Update() {
		if(cursor.mode == ShipBuilderCursorScript.CursorMode.FlipRotate) {
			GetComponent<Image>().enabled = true;
			var tmp = cursor.lastPart.builderImage.transform.position;
			tmp.x += ((int)type == 1 ? 15 : -15);
			tmp.y += 50;
			transform.position = tmp;
		} else GetComponent<Image>().enabled = false;
	}
}
