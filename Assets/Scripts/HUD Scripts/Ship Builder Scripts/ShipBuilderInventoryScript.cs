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
    int count;

    protected override void Start() {
        val = GetComponentInChildren<Text>();
        val.text = count + "";
        base.Start();
        // button border size is handled specifically by the grid layout components
    }
    public void OnPointerDown(PointerEventData eventData)
    {
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
        image.color = count > 0 ? FactionColors.colors[0] : Color.gray; // gray gray gray gray gray USA USA USA USA USA
        if(GetComponentsInChildren<Image>().Length > 1) GetComponentsInChildren<Image>()[2].color = count > 0 ? FactionColors.colors[0] : Color.gray;
    }
}
