using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;



public class ShipBuilderInventoryScript : MonoBehaviour, IPointerDownHandler {
    public EntityBlueprint.PartInfo part;
    public GameObject SBPrefab;
    public ShipBuilderCursorScript cursor;
    public Text val;
    public BuilderMode mode;
    int count;
    Image image;

    void Start() {
        val = GetComponentInChildren<Text>();
        val.text = count + "";
        image = GetComponentsInChildren<Image>()[1];
        image.sprite = ResourceManager.GetAsset<Sprite>(part.partID + "_sprite");
        string shooterID = AbilityUtilities.GetShooterByID(part.abilityID);
        if(shooterID != null) {
            GetComponentsInChildren<Image>()[2].sprite = ResourceManager.GetAsset<Sprite>(shooterID);
            GetComponentsInChildren<Image>()[2].color = FactionColors.colors[0];
            GetComponentsInChildren<Image>()[2].rectTransform.sizeDelta = GetComponentsInChildren<Image>()[2].sprite.bounds.size * 100;
        } else GetComponentsInChildren<Image>()[2].enabled = false;
        image.color = FactionColors.colors[0];
        image.GetComponent<RectTransform>().sizeDelta = image.sprite.bounds.size * 100;
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
