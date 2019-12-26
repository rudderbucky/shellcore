using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldCreatorCursor : MonoBehaviour
{
    public ItemHandler handler;
    Item current;
	public readonly float tileSize = 10F;
    
    public GameObject borderPrefab;
    SectorWCWrapper currentSector;
	public readonly Vector2 cursorOffset = new Vector2(5F, 5F);
    public int currentIndex;
    int maxIndex;
    public EventSystem system;
    public List<Item> placedItems = new List<Item>();
    public ItemPropertyDisplay propertyDisplay;
    public SectorPropertyDisplay sectorPropertyDisplay;
    
    public enum WCCursorMode {
        Item,
        Sector,
        Control
    }

    WCCursorMode mode = WCCursorMode.Item;
    public Text modeText;

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

        VisualizeMouseInSector();

        if(Input.GetKeyDown(KeyCode.Z)) 
        {
            mode = (WCCursorMode)(((int)mode + 1) % 3);
        }

        var modeColors = new Color[]
        {
            new Color32(28, 42, 63, 255),
            new Color32(63, 28, 42, 255),
            new Color32(42, 63, 28, 255),
        };

        switch(mode)
        {
            case WCCursorMode.Item:
                RemovePendingSector();
                current.obj.SetActive(true);
                modeText.text = "Item Mode";
                PollItems();
                break;
            case WCCursorMode.Sector:
                current.obj.SetActive(false);
                modeText.text = "Sector Mode";
                if(!system.IsPointerOverGameObject()) 
                    PollSectors();
                break;
            case WCCursorMode.Control:
                RemovePendingSector();
                current.obj.SetActive(false);
                modeText.text = "Control Mode";
                if(!system.IsPointerOverGameObject()) 
                    PollControls();
                break;
            default:
                break;
        }

        modeText.color = Camera.main.backgroundColor = modeColors[(int)mode];
        modeText.color += Color.gray;   
    }

    public GUIWindowScripts taskInterface;
    void PollControls()
    {
        if(Input.GetKeyUp(KeyCode.T))
            taskInterface.ToggleActive();
    }

    // revert or destroy pending sector if it exists        
    void RemovePendingSector()
    {
        if(currentSector != null)
        {
            if(lastSectorPos != null) {
                for(int i = 0; i < 4; i++) {
                    currentSector.renderer.SetPosition(i, lastSectorPos[i]);
                }
                SyncSectorCoords(currentSector);
                currentSector = null;
                lastSectorPos = null;
            } else 
            {
                sectors.Remove(currentSector);
                Destroy(currentSector.renderer.gameObject);
                currentSector = null;
            }
        }
    }
    void SyncSectorCoords(SectorWCWrapper wrapper) 
    {
        wrapper.sector.bounds.x = Mathf.Min((int)wrapper.renderer.GetPosition(0).x, (int)wrapper.renderer.GetPosition(2).x);
        wrapper.sector.bounds.y = Mathf.Min((int)wrapper.renderer.GetPosition(0).y, (int)wrapper.renderer.GetPosition(2).y);
        wrapper.sector.bounds.w = Mathf.Abs((int)wrapper.renderer.GetPosition(2).x - (int)wrapper.renderer.GetPosition(0).x);
        wrapper.sector.bounds.h = Mathf.Abs((int)wrapper.renderer.GetPosition(2).y - (int)wrapper.renderer.GetPosition(0).y);
        
    }

    // check if mouse is in a sector
    void VisualizeMouseInSector() {
        foreach(SectorWCWrapper sector in sectors) 
        {
            var renderer = sector.renderer;
            if(CheckMouseContainsSector(renderer)) 
            {
                if(renderer.sortingOrder < sortLayerNum) 
                    renderer.sortingOrder = ++sortLayerNum;
                renderer.startColor = renderer.endColor = Color.green;
            } else renderer.startColor = renderer.endColor = Color.white;
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

    public class SectorWCWrapper 
    {
        public LineRenderer renderer;
        public Sector sector;
    }

    public List<SectorWCWrapper> sectors = new List<SectorWCWrapper>();
    void PollSectors() 
    {
        if(Input.GetMouseButtonDown(0)) 
        {
            foreach(SectorWCWrapper sector in sectors)
            {
                LineRenderer renderer = sector.renderer;
                if(CheckMouseContainsSector(renderer)) 
                {
                    if(Input.GetKey(KeyCode.LeftShift)) {
                        sectorPropertyDisplay.DisplayProperties(sector.sector);
                        return;
                    }

                    origPos = renderer.GetPosition(0);
                    lastSectorPos = new Vector3[4];
                    for(int i = 0; i < 4; i++) 
                    {
                        lastSectorPos[i] = renderer.GetPosition(i);
                    }
                    renderer.SetPosition(0, origPos);
                    currentSector = sector;
                    break;
                }
            }
            if(currentSector == null) {
                currentSector = new SectorWCWrapper();
                currentSector.sector = ScriptableObject.CreateInstance<Sector>();
                var renderer = currentSector.renderer = Instantiate(borderPrefab).GetComponent<LineRenderer>();
                renderer.GetComponentInChildren<WorldCreatorSectorRepScript>().sector = currentSector.sector;
                lastSectorPos = null;
                origPos = CalcSectorPos();
                renderer.SetPosition(0, origPos);
                renderer.SetPosition(1, origPos);
                renderer.SetPosition(2, origPos);
                renderer.SetPosition(3, origPos);
                SyncSectorCoords(currentSector);
            }
        }
        else if(currentSector != null && Input.GetMouseButtonUp(0)) 
        {
            if(!CheckForSectorOverlap(currentSector.renderer) && CheckSectorSize(currentSector.renderer)) 
            {
                if(lastSectorPos == null) sectors.Add(currentSector);
            } else if(lastSectorPos != null) { // invalid position for current sector
                for(int i = 0; i < 4; i++) {
                    currentSector.renderer.SetPosition(i, lastSectorPos[i]);
                }
                SyncSectorCoords(currentSector);
                lastSectorPos = null;
            } else  // delete sector
            {
                Destroy(currentSector.renderer.gameObject);
                if(sectors.Contains(currentSector)) sectors.Remove(currentSector);
            }
            currentSector = null; // reset reference
        }
        else if(currentSector != null && Input.GetMouseButton(0))
        {
            var renderer = currentSector.renderer;
            renderer.SetPosition(1, new Vector3(origPos.x, CalcSectorPos().y, 0));
            renderer.SetPosition(2, CalcSectorPos());
            renderer.SetPosition(3, new Vector3(CalcSectorPos().x, origPos.y, 0));
            SyncSectorCoords(currentSector);
            // check for overlap
            renderer.startColor = renderer.endColor = Color.white;
            if(CheckForSectorOverlap(renderer)) 
            {
                if(renderer.sortingOrder < sortLayerNum)
                    renderer.sortingOrder = ++sortLayerNum;
                renderer.startColor = renderer.endColor = Color.red;
            }
        }
        else if(Input.GetMouseButtonUp(1)) 
        {
            foreach(SectorWCWrapper sector in sectors) 
            {
                var renderer = sector.renderer;
                if(CheckMouseContainsSector(renderer))
                {
                    Destroy(renderer.gameObject);
                    if(sectors.Contains(sector)) 
                    {
                        sectors.Remove(sector);
                        currentSector = null;
                        return;
                    }
                }
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

    public Vector2 GetSectorCenter() 
    {
        foreach(var sector in sectors)
        {
            if(CheckMouseContainsSector(sector.renderer)) return sector.renderer.bounds.center;
        }
        return Vector2.zero;
    }
    bool CheckForSectorOverlap(LineRenderer checkRenderer) 
    {
        foreach(SectorWCWrapper sector in sectors) 
        {
            var renderer = sector.renderer;
            if(renderer != checkRenderer) 
            {
                Bounds rendBounds = renderer.bounds;
                rendBounds.Expand(-cursorOffset);
                if(rendBounds.Intersects(checkRenderer.bounds)) 
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

    public static Vector2 GetMousePos() {
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
