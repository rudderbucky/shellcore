using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ShipBuilderJSONButton : MonoBehaviour, IPointerClickHandler {

	public InputField field;
	public GUIWindowScripts builder;
	public GUIWindowScripts window;

    public void OnPointerClick(PointerEventData eventData)
    {
		if(window)
		{
			window.ToggleActive();
			field.text = (builder as IBuilderInterface).GetCurrentJSON();
		}
    }
}
