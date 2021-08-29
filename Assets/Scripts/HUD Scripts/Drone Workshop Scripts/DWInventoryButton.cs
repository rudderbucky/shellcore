using UnityEngine;
using UnityEngine.EventSystems;



public class DWInventoryButton : ShipBuilderInventoryBase, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public DWSelectionDisplayHandler handler;
    public DroneWorkshop workshop;
    public EntityBlueprint blueprint;
    private DroneSpawnData data;

    protected override void Start()
    {
        data = DroneWorkshop.ParseDronePart(part);
        blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
        JsonUtility.FromJsonOverwrite(data.drone, blueprint);
        base.Start();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        handler.AssignDisplay(blueprint, data);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        handler.ClearDisplay();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        workshop.InitializeBuildPhase(blueprint, part, data);
    }
}
