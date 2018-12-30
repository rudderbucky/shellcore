using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.

public class AbilityButtonScript : MonoBehaviour, IPointerDownHandler
{
    public bool clicked;

    public void OnPointerDown(PointerEventData eventData)
    {
        clicked = true;
    }
}
