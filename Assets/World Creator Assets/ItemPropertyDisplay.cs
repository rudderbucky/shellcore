using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPropertyDisplay : MonoBehaviour
{
    public RectTransform rectTransform;
    public Dropdown factionDropdown;
    Item currentItem;

    void Start() {
        if(!rectTransform) rectTransform = GetComponent<RectTransform>();
        Hide();
    }
    void Update() {
        var pos = Camera.main.WorldToScreenPoint(currentItem.pos);
        pos += new Vector3(300, 0);
        rectTransform.anchoredPosition = pos;
    }
    public void DisplayProperties(Item item) {
        currentItem = item;
        gameObject.SetActive(true);
        var pos = Camera.main.WorldToScreenPoint(currentItem.pos);
        pos += new Vector3(300, 0);
        rectTransform.anchoredPosition = pos;
    }

    public void UpdateFaction() {
        currentItem.faction = factionDropdown.value;
        Debug.Log("updated faction: " + currentItem.faction );
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
