using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.UI;

public class ShipBuilderInventoryBase : MonoBehaviour
{
    public EntityBlueprint.PartInfo part;
    protected Image image;
    protected Image shooter;
    protected Color activeColor;
    public Text val;
    public Image isShiny;

    protected virtual void Start() {
        image = GetComponentsInChildren<Image>()[1];
        shooter = GetComponentsInChildren<Image>()[2];
        val = GetComponentInChildren<Text>();
        image.sprite = ResourceManager.GetAsset<Sprite>(part.partID + "_sprite");
        isShiny.enabled = part.shiny;

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
            shooter.sprite = ResourceManager.GetAsset<Sprite>(shooterID);
            shooter.color = activeColor;
            shooter.rectTransform.sizeDelta = shooter.sprite.bounds.size * 100;
        } else shooter.enabled = false;
    }
}
