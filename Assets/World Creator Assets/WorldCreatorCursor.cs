using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldCreatorCursor : MonoBehaviour
{
    public ItemHandler handler;
    Item current;
	private float tileSize = 10F;
    public LineRenderer testSectorBorder;
	Vector2 cursorOffset = new Vector2(5F, 5F);
    public int currentIndex;
    int maxIndex;
    public EventSystem system;
    List<Item> placedItems = new List<Item>();
    public ItemPropertyDisplay propertyDisplay;

    void Start() {
        maxIndex = handler.itemPack.items.Count;
    }
    // Update is called once per frame
    void Update() {
        if(Input.mouseScrollDelta.y < 0 && currentIndex < maxIndex - 1) SetCurrent(++currentIndex % maxIndex);
        else if(Input.mouseScrollDelta.y > 0 && currentIndex > 0) SetCurrent(--currentIndex % maxIndex);
		
        SetCurrent(0);
		current.pos = CalcPos(current);
        if(current.obj) {
            current.obj.transform.position = current.pos;
        }
        
        if(Input.GetKey(KeyCode.Z)) 
        {
            current.obj.SetActive(false);
            PollSectors();
        }
        else 
        {
            current.obj.SetActive(true);
            PollItems();
        }
        
    }

    void PollItems() {
        if(GetItemUnderCursor() != null) 
        {
            Item underCursor = new Item();
            underCursor = (Item)GetItemUnderCursor();
            if(Input.GetMouseButtonUp(0) && !system.IsPointerOverGameObject() && current.obj) 
            {
                if(((Item)underCursor).type == current.type)
                    propertyDisplay.DisplayProperties(underCursor);
                else Add(CopyCurrent());
            } 
            else if(Input.GetKeyUp(KeyCode.R) && !system.IsPointerOverGameObject()) 
            {
                if(underCursor.type == ItemType.Platform) 
                {
                    Rotate((Item)underCursor);
                }
            }
            else if(Input.GetMouseButtonUp(1) && !system.IsPointerOverGameObject()) 
            {
                Remove((Item)underCursor);
            }
        } 
        else 
        {
            if(Input.GetMouseButtonUp(0) && !system.IsPointerOverGameObject() && current.obj) 
            {
                Add(CopyCurrent());
            } 
        }
    }

    Vector3 origPos = new Vector3();
    void PollSectors() {
        if(Input.GetKeyDown(KeyCode.Z)) {
            origPos = CalcSectorPos();
            testSectorBorder.SetPosition(0, origPos);
            testSectorBorder.SetPosition(1, origPos);
            testSectorBorder.SetPosition(2, origPos);
            testSectorBorder.SetPosition(3, origPos);
        }
        else 
        {
            testSectorBorder.SetPosition(1, new Vector3(origPos.x, CalcSectorPos().y, 0));
            testSectorBorder.SetPosition(2, CalcSectorPos());
            testSectorBorder.SetPosition(3, new Vector3(CalcSectorPos().x, origPos.y, 0));
        }

    }

    Item? GetItemUnderCursor() {
        foreach(Item itemObj in placedItems) {
            if(itemObj.pos == current.pos) {
                return itemObj;
            }
        }
        return null;
    }

    void Remove(Item item) {
        placedItems.Remove(item);
        Destroy(item.obj);
        propertyDisplay.Hide();
    }

    void Add(Item item) {
        placedItems.Insert(0, item);
        propertyDisplay.Hide();
    }

    void Rotate(Item item) {
        item.obj.transform.Rotate(0, 0, 90);
    }
    public Vector2 CalcPos(Item item) {
        Vector3 mousePos = Input.mousePosition;
		mousePos.z -= Camera.main.transform.position.z;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        if(item.type == ItemType.Platform) {
			mousePos.x = cursorOffset.x + tileSize * (int)((mousePos.x - cursorOffset.x) / tileSize + (mousePos.x / 2> 0 ? 0.5F : -0.5F));
			mousePos.y = cursorOffset.y + tileSize * (int)((mousePos.y - cursorOffset.y) / tileSize + (mousePos.y / 2> 0 ? 0.5F : -0.5F));
		} else {
			mousePos.x = 0.5F * tileSize * Mathf.RoundToInt((mousePos.x) / (0.5F * tileSize));
			mousePos.y = 0.5F * tileSize * Mathf.RoundToInt((mousePos.y) / (0.5F * tileSize));
		}
        return mousePos;
    }

    public Vector2 CalcSectorPos() {
                Vector3 mousePos = Input.mousePosition;
		mousePos.z -= Camera.main.transform.position.z;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        mousePos.x = tileSize * (int)((mousePos.x) / tileSize + (mousePos.x / 2> 0 ? 0.5F : -0.5F));
        mousePos.y = tileSize * (int)((mousePos.y) / tileSize + (mousePos.y / 2> 0 ? 0.5F : -0.5F));
        return mousePos;
    }

    public Item CopyCurrent() {
        var copy = handler.CopyItem(current);
        copy.obj.transform.position = copy.pos;
        return copy;
    }
    public void SetCurrent(int index) {
        if(current.obj) Destroy(current.obj);
        currentIndex = index;
        current = handler.GetItemByIndex(index);
        current.pos = CalcPos(current);
        current.obj.transform.position = current.pos;
    }
}
