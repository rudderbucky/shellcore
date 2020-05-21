using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;



public class ShipBuilderInventoryScript : ShipBuilderInventoryBase, IPointerDownHandler {

    public GameObject SBPrefab;
    public ShipBuilderCursorScript cursor;
    public Text val;
    public BuilderMode mode;
    public Image isShiny;
    int count;

    protected override void Start() {
        val = GetComponentInChildren<Text>();
        val.text = count + "";
        val.enabled = (mode == BuilderMode.Yard || mode == BuilderMode.Workshop);
        base.Start();
        isShiny.enabled = part.shiny;
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
        image.color = count > 0 ? activeColor : Color.gray; // gray gray gray gray gray USA USA USA USA USA
        if(GetComponentsInChildren<Image>().Length > 1) GetComponentsInChildren<Image>()[2].color = count > 0 ? activeColor : Color.gray;
    }
}
