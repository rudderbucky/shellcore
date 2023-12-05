using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FusionStationInventoryScript : ShipBuilderInventoryBase
{
    private int count;
    public PartDisplayBase partDisplayBase;
    public FusionStationScript fusionStationScript;
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }
        if (count > 0)
            fusionStationScript.SetSelectedPart(part);
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        partDisplayBase.DisplayPartInfo(part);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        partDisplayBase.SetInactive();
    }

    public int GetCount()
    {
        return count;
    }

    public void IncrementCount(bool obeyDroneCount = false)
    {
        count++;
        val.text = count.ToString();
    }

    public void DecrementCount(bool destroyIfZero = false, bool obeyDroneCount = false)
    {
        count--;
        val.text = count.ToString();
        if (destroyIfZero && count <= 0)
        {
            ShipBuilder.instance.RemoveKeyFromPartDict(part);
            Destroy(gameObject);
        }
    }
}
