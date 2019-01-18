using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;


public class ShipBuilderInventoryScript : MonoBehaviour, IPointerDownHandler {
    public EntityBlueprint.PartInfo part;
    public ShipBuilderCursorScript cursor;

    void Start() {
        GetComponentsInChildren<Image>()[1].color = FactionColors.colors[0];
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        cursor.currentPart = part;
    }
}
