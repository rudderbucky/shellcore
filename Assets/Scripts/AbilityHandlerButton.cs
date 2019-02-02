using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.

public class AbilityHandlerButton : MonoBehaviour, IPointerClickHandler {
	public AbilityHandler.AbilityTypes type;
	public AbilityHandler handler;
    public void OnPointerClick(PointerEventData eventData)
    {
        //foreach(AbilityHandlerButton button in transform.parent.GetComponentsInChildren<AbilityHandlerButton>()) {
        //    button.GetComponent<Image>().color = Color.black;
        //}
        //GetComponent<Image>().color = new Color32((byte)32,(byte)32,(byte)32,(byte)255);
        handler.SetCurrentVisible(type);
    }
    void Update() {
        if(handler.currentVisibles != type) {
            GetComponent<Image>().color = Color.black;
        } else GetComponent<Image>().color = new Color32((byte)32,(byte)32,(byte)32,(byte)255);
    }
}
