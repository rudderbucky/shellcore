using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FusionStationInventoryScript : ShipBuilderInventoryBase
{
    private int count;
    public PartDisplayBase partDisplayBase;
    public override void OnPointerDown(PointerEventData eventData)
    {

    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        partDisplayBase.DisplayPartInfo(part);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        partDisplayBase.SetInactive();
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
