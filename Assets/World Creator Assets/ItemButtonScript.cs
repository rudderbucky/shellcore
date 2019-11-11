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

    void Start() {
        text.text = item.assetID;
    }

    public void SetCursorItem() {
        cursor.SetCurrent(item);
    }
}
