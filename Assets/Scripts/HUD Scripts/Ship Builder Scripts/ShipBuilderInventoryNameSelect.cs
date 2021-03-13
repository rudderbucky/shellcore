using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.UI;

public class ShipBuilderInventoryNameSelect : ShipBuilderInventoryBase
{
    public InputField field;
    public ShipBuilder builder;

    protected override void Start()
    {
        base.Start();
        val.enabled = false;
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        field.text = part.partID;
        builder.SetSelectPartActive(false);
    }
}
