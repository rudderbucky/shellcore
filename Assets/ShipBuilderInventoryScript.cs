using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;



public class ShipBuilderInventoryScript : MonoBehaviour, IPointerDownHandler {
    public EntityBlueprint.PartInfo part;
    public ShipBuilderCursorScript cursor;

    public static string GetShooterID(Ability.AbilityType type) {
        switch(type) {
            case Ability.AbilityType.None:
            case Ability.AbilityType.Speed:
                return null;
            case Ability.AbilityType.Beam:
                return "beamshooter_sprite";
            case Ability.AbilityType.Bullet:
                return "bulletshooter_sprite";
            case Ability.AbilityType.Cannon:
                return "cannonshooter_sprite";
            case Ability.AbilityType.Missile:
                return "missileshooter_sprite";
            case Ability.AbilityType.Torpedo:
                return "torpedoshooter_sprite";
            default:
                return "ability_indicator";
        }
    }
    void Start() {
        var image = GetComponentsInChildren<Image>()[1];
        image.sprite = ResourceManager.GetAsset<Sprite>(part.partID + "_sprite");
        string shooterID = GetShooterID(part.abilityType);
        if(shooterID != null) {
            GetComponentsInChildren<Image>()[2].sprite = ResourceManager.GetAsset<Sprite>(shooterID);
            GetComponentsInChildren<Image>()[2].color = FactionColors.colors[0];
            GetComponentsInChildren<Image>()[2].rectTransform.sizeDelta = GetComponentsInChildren<Image>()[2].sprite.bounds.size * 100;
        }
        image.color = FactionColors.colors[0];
        image.GetComponent<RectTransform>().sizeDelta = image.sprite.bounds.size * 100;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        cursor.currentPart = part;
    }
}
