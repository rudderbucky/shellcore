using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemButtonScript : MonoBehaviour
{
    public Item item;
    public int itemIndex;
    public WorldCreatorCursor cursor;
    public Text text;

    Image image;

    void Start() {
        image = GetComponent<Image>();
        text.text = item.assetID;
    }

    public void SetCursorItem() {
        cursor.SetCurrent(itemIndex);
    }

    void Update() {
        if(cursor.currentIndex == itemIndex) image.color = new Color32(60, 120, 60, 255);
        else image.color = new Color32(60, 60, 60, 255);
    }
}
