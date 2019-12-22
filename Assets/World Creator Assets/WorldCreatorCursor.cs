using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldCreatorCursor : MonoBehaviour
{
    public ItemHandler handler;
    Item current;
	private float tileSize = 10F;
    
    public GameObject borderPrefab;
    LineRenderer testSectorBorder;
	Vector2 cursorOffset = new Vector2(5F, 5F);
    public int currentIndex;
    int maxIndex;
    public EventSystem system;
    List<Item> placedItems = new List<Item>();
    public ItemPropertyDisplay propertyDisplay;

    void Start() {
        SetCurrent(0);
        maxIndex = handler.itemPack.items.Count;
    }
    // Update is called once per frame
    static int sortLayerNum = 1;
    void Update() {
        if(Input.mouseScrollDelta.y < 0 && currentIndex < maxIndex - 1) SetCurrent(++currentIndex % maxIndex);
        else if(Input.mouseScrollDelta.y > 0 && currentIndex > 0) SetCurrent(--currentIndex % maxIndex);
		
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
            // check if mouse is in a sector
            foreach(LineRenderer renderer in renderers) 
            {
                if(CheckMouseContainsSector(renderer)) 
                {
                    if(renderer.sortingOrder < sortLayerNum) 
                        renderer.sortingOrder = ++sortLayerNum;
                    renderer.startColor = renderer.endColor = Color.green;
                } else renderer.startColor = renderer.endColor = Color.white;
            }

            // revert or destroy pending sector if it exists
            if(testSectorBorder)
                if(lastSectorPos != null) {
                    for(int i = 0; i < 4; i++) {
                        testSectorBorder.SetPosition(i, lastSectorPos[i]);
                    }
                    testSectorBorder = null;
                    lastSectorPos = null;
                } else Destroy(testSectorBorder.gameObject);

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
    Vector3[] lastSectorPos = null;
    List<LineRenderer> renderers = new List<LineRenderer>();
    void PollSectors() 
    {
        if(Input.GetMouseButtonDown(0)) 
        {
            foreach(LineRenderer renderer in renderers)
            {
                if(CheckMouseContainsSector(renderer)) 
                {
                    origPos = renderer.GetPosition(0);
                    lastSectorPos = new Vector3[4];
                    for(int i = 0; i < 4; i++) {
                        lastSectorPos[i] = renderer.GetPosition(i);
                    }
                    testSectorBorder = renderer;
                }
            }
            if(!testSectorBorder) {
                testSectorBorder = Instantiate(borderPrefab).GetComponent<LineRenderer>();
                lastSectorPos = null;
                origPos = CalcSectorPos();
                testSectorBorder.SetPosition(0, origPos);
                testSectorBorder.SetPosition(1, origPos);
                testSectorBorder.SetPosition(2, origPos);
                testSectorBorder.SetPosition(3, origPos);
            }
        }
        else if(testSectorBorder && Input.GetMouseButtonUp(0)) 
        {
            if(!CheckForSectorOverlap(testSectorBorder) && CheckSectorSize(testSectorBorder)) 
            {
                renderers.Add(testSectorBorder);
            } else if(lastSectorPos != null) {
                for(int i = 0; i < 4; i++) {
                    testSectorBorder.SetPosition(i, lastSectorPos[i]);
                }
                lastSectorPos = null;
            } else Destroy(testSectorBorder.gameObject);
            testSectorBorder = null; // reset reference
        }
        else if(testSectorBorder && Input.GetMouseButton(0))
        {
            testSectorBorder.SetPosition(1, new Vector3(origPos.x, CalcSectorPos().y, 0));
            testSectorBorder.SetPosition(2, CalcSectorPos());
            testSectorBorder.SetPosition(3, new Vector3(CalcSectorPos().x, origPos.y, 0));

            // check for overlap
            testSectorBorder.startColor = testSectorBorder.endColor = Color.white;
            if(CheckForSectorOverlap(testSectorBorder)) 
            {
                if(testSectorBorder.sortingOrder < sortLayerNum)
                    testSectorBorder.sortingOrder = ++sortLayerNum;
                testSectorBorder.startColor = testSectorBorder.endColor = Color.red;
            }
        }
    }

    ///
    /// Returns true if the sector being checked is not a line
    ///
    bool CheckSectorSize(LineRenderer sector)
    {
        var pos1 = sector.GetPosition(0);
        var pos2 = sector.GetPosition(2);
        return !((pos1.x == pos2.x) || (pos1.y == pos2.y));
    }

    bool CheckMouseContainsSector(LineRenderer renderer) {
        return renderer.bounds.Contains(GetMousePos()); 
    }

    bool CheckForSectorOverlap(LineRenderer testSectorBorder) 
    {
        foreach(LineRenderer renderer in renderers) 
        {
            if(renderer != testSectorBorder) 
            {
                Bounds rendBounds = renderer.bounds;
                rendBounds.Expand(-cursorOffset);
                if(rendBounds.Intersects(testSectorBorder.bounds)) 
                {
                    return true;
                }
            } 
        }
        return false;
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
        Vector3 mousePos = GetMousePos();
        if(item.type == ItemType.Platform) {
			mousePos.x = cursorOffset.x + tileSize * (int)((mousePos.x - cursorOffset.x) / tileSize + (mousePos.x / 2> 0 ? 0.5F : -0.5F));
			mousePos.y = cursorOffset.y + tileSize * (int)((mousePos.y - cursorOffset.y) / tileSize + (mousePos.y / 2> 0 ? 0.5F : -0.5F));
		} else {
			mousePos.x = 0.5F * tileSize * Mathf.RoundToInt((mousePos.x) / (0.5F * tileSize));
			mousePos.y = 0.5F * tileSize * Mathf.RoundToInt((mousePos.y) / (0.5F * tileSize));
		}
        return mousePos;
    }

    public Vector2 GetMousePos() {
        Vector3 mousePos = Input.mousePosition;
		mousePos.z -= Camera.main.transform.position.z;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        return mousePos;
    }
    public Vector2 CalcSectorPos() {
        Vector3 mousePos = GetMousePos();
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
