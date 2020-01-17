using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPropertyDisplay : MonoBehaviour
{
    public RectTransform rectTransform;
    public Dropdown factionDropdown;
    public InputField jsonField;
    public InputField nameField;
    public Text assetName;
    public Text assetType;
    Item currentItem;

    void Start() 
    {
        if(!rectTransform) rectTransform = GetComponent<RectTransform>();
    }

    void Update() 
    {
        var pos = Camera.main.WorldToScreenPoint(currentItem.pos);
        pos += new Vector3(300, 0);
        rectTransform.anchoredPosition = pos;
    }
    public void DisplayProperties(Item item) 
    {
        currentItem = item;
        gameObject.SetActive(true);
        var pos = Camera.main.WorldToScreenPoint(currentItem.pos);
        pos += new Vector3(300, 0);
        rectTransform.anchoredPosition = pos;
        factionDropdown.value = item.faction;
        assetName.text = currentItem.assetID;
        assetType.text = currentItem.type + "";
        jsonField.text = currentItem.shellcoreJSON;
        nameField.text = currentItem.name;
    }

    public void UpdateFaction() 
    {
        currentItem.faction = factionDropdown.value;
        Debug.Log("updated faction: " + currentItem.faction );
    }

    public void UpdateBlueprint()
    {
        currentItem.shellcoreJSON = jsonField.text;
    }

    public void UpdateName()
    {
        currentItem.name = nameField.text;
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
