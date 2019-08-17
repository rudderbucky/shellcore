using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class VendorUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string text;
    public Text nameInfo;
    public Text costInfo;
    public SelectionDisplayHandler handler;
    public EntityBlueprint blueprint;

    public void OnPointerEnter(PointerEventData eventData)
    {
        costInfo.text = text;
        nameInfo.text = blueprint.entityName.ToUpper();
        handler.AssignDisplay(blueprint, null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        costInfo.text = nameInfo.text = "";
        handler.ClearDisplay();
    }
}
