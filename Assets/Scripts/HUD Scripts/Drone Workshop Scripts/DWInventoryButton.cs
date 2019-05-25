using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Required when using Event data.

public class DWInventoryButton : ShipBuilderInventoryBase, IPointerEnterHandler
{
    public DWSelectionDisplayHandler handler;

    public void OnPointerEnter(PointerEventData eventData)
    {
        DroneSpawnData data = ScriptableObject.CreateInstance<DroneSpawnData>();
        JsonUtility.FromJsonOverwrite(part.secondaryData, data);
        EntityBlueprint blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
        JsonUtility.FromJsonOverwrite(data.drone, blueprint);
        handler.AssignDisplay(blueprint);
    }
}
