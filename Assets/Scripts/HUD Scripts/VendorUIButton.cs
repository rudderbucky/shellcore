using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class VendorUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string text;
    public Text costInfo;

    public void OnPointerEnter(PointerEventData eventData)
    {
        costInfo.text = text;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        costInfo.text = "";
    }
}
