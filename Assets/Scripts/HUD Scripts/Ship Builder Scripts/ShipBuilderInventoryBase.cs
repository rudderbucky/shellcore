using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.UI;

public class ShipBuilderInventoryBase : MonoBehaviour
{
    public EntityBlueprint.PartInfo part;
    protected Image image;
    protected Color activeColor;

    protected virtual void Start() {
        image = GetComponentsInChildren<Image>()[1];
        image.sprite = ResourceManager.GetAsset<Sprite>(part.partID + "_sprite");

        image.color = activeColor = FactionColors.colors[0];
        if(part.shiny) 
        {
            activeColor += new Color32(0, 0, 150, 0);
            image.color = activeColor;
        }
        image.GetComponent<RectTransform>().sizeDelta = image.sprite.bounds.size * 100;
        // button border size is handled specifically by the grid layout components
    
        string shooterID = AbilityUtilities.GetShooterByID(part.abilityID);
        if(shooterID != null) {
            GetComponentsInChildren<Image>()[2].sprite = ResourceManager.GetAsset<Sprite>(shooterID);
            GetComponentsInChildren<Image>()[2].color = activeColor;
            GetComponentsInChildren<Image>()[2].rectTransform.sizeDelta = GetComponentsInChildren<Image>()[2].sprite.bounds.size * 100;
        } else GetComponentsInChildren<Image>()[2].enabled = false;
    }
}
