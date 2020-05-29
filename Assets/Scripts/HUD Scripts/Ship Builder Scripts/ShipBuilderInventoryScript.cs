using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;



public class ShipBuilderInventoryScript : ShipBuilderInventoryBase, IPointerDownHandler {

    public GameObject SBPrefab;
    public ShipBuilderCursorScript cursor;
    public BuilderMode mode;
    
    int count;

    protected override void Start() {
        base.Start();
        val.text = count + "";
        val.enabled = (mode == BuilderMode.Yard || mode == BuilderMode.Workshop);
        // button border size is handled specifically by the grid layout components
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if(Input.GetKey(KeyCode.LeftShift)) 
        {
            #if UNITY_EDITOR
            Debug.Log(part.secondaryData);
            #endif
        }

        if(count > 0) {
            var builderPart = Instantiate(SBPrefab, cursor.transform.parent).GetComponent<ShipBuilderPart>();
            builderPart.info = part;
            builderPart.cursorScript = cursor;
            builderPart.mode = mode;
            cursor.parts.Add(builderPart);
            cursor.GrabPart(builderPart);
            count--;
            cursor.buildValue += EntityBlueprint.GetPartValue(part);
            if(mode == BuilderMode.Trader) cursor.buildCost += EntityBlueprint.GetPartValue(part);

            if(Input.GetKey(KeyCode.LeftShift)) 
            {
                if(mode == BuilderMode.Yard && cursor.builder.GetMode() == BuilderMode.Trader) cursor.builder.DispatchPart(builderPart, ShipBuilder.TransferMode.Sell);
                else if(mode == BuilderMode.Trader) cursor.builder.DispatchPart(builderPart, ShipBuilder.TransferMode.Buy);
            }
        }
    }
    public void IncrementCount() {
        count++;
    }

    public void DecrementCount() {
        count--;
    }
    public int GetCount() {
        return count;
    }
    void Update() {
        val.text = count + "";
        image.color = count > 0 ? activeColor : Color.gray;
        if(shooter) shooter.color = count > 0 ? activeColor : Color.gray;
    }
}
