using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPropertyDisplay : MonoBehaviour
{
    public RectTransform rectTransform;
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

    public void Hide() {
        gameObject.SetActive(false);
    }
}
