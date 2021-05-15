using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ShipBuilderJSONButton : MonoBehaviour, IPointerClickHandler {

	public InputField field;
	public ShipBuilder builder;
	public GUIWindowScripts window;

    public void OnPointerClick(PointerEventData eventData)
    {
		if(window)
		{
			window.ToggleActive();
			field.text = builder.GetCurrentJSON();
		}
    }
}
