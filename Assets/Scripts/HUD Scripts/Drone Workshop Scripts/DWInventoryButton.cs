using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Required when using Event data.

public class DWInventoryButton : ShipBuilderInventoryBase, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public DWSelectionDisplayHandler handler;
    public DroneWorkshop workshop;
    public EntityBlueprint blueprint;
    
    protected override void Start() {

        blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
        JsonUtility.FromJsonOverwrite(DroneWorkshop.ParseDronePart(part).drone, blueprint);
        base.Start();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        handler.AssignDisplay(blueprint);
    }
    public void OnPointerExit(PointerEventData eventData) {
        handler.ClearDisplay();
    }

    public void OnPointerClick(PointerEventData eventData) {
        workshop.InitializeBuildPhase(blueprint, part);
    }
}
