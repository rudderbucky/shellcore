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
        handler.SetCurrentVisible(type);
    }
}
