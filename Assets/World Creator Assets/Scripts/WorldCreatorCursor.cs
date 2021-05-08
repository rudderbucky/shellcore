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

    public delegate void SelectEntityDelegate(string EntityID);
    public static SelectEntityDelegate selectEntity;

    public delegate void FinishPathDelegate(NodeEditorFramework.Standard.PathData path);
    public static FinishPathDelegate finishPath;
    public static WorldCreatorCursor instance;
    public ShipBuilder shipBuilder;
    public WaveBuilder waveBuilder;
    public WCCharacterHandler characterHandler;
    WCPathCreator pathCreator;
    int cursorModeCount;
    public static WCCursorMode originalCursorMode;

    public enum WCCursorMode
    {
        Item,
        Sector,
        Control,
        SelectEntity,
        DrawPath
    }

    public readonly Color[] modeColors = new Color[]
        {
            new Color32(28, 42, 63, 255),
            new Color32(63, 28, 42, 255),
            new Color32(42, 63, 28, 255),
            new Color32(42, 64, 64, 255),
            new Color32(42, 64, 64, 255),
        };

    WCCursorMode mode = WCCursorMode.Item;
    public Text modeText;
    public List<WorldData.CharacterData> characters = new List<WorldData.CharacterData>();

    public WCCursorMode GetMode()
    {
        return mode;
    }

    private void Awake()
    {
        if (instance != null)
            Debug.LogError("Too many WorldCreatorCursor instances!");
        instance = this;
        cursorModeCount = System.Enum.GetValues(typeof(WCCursorMode)).Length;
    }

    public void ShiftMode(int bump)
    {
        SetMode((WCCursorMode)(((int)mode + bump) % 3));
    }

    void Start() {
        SetCurrent(0);
        maxIndex = handler.itemPack.items.Count;
        pathCreator = gameObject.AddComponent<WCPathCreator>();
        AudioManager.StopMusic();
    }
    // Update is called once per frame
    static int sortLayerNum = 1;
    public GUIWindowScripts manual;

    public List<string> music;
    private float musicTimer;
    private static readonly float musicTimerThreshold = 10;

    void UpdateMusic()
    {
        // time music so that it does not immediately start (5 seconds for now)
        // play a random song (might make it so that it doesn't play the same song twice)
        if(music.Count == 0 || !AudioManager.instance) return;
        if(!AudioManager.instance.playerMusicSource.isPlaying)
            musicTimer += Time.deltaTime;

        // TODO: Add null logic to AudioManager when the song is done playing
        if(musicTimer >= musicTimerThreshold)
        {
            musicTimer = 0;
            var track = music[Random.Range(0,music.Count)];
            AudioManager.PlayMusic(track, false);
        }
    }

    void Update() {
		current.pos = CalcPos(current.type);
        if(current.obj) {
            current.obj.transform.position = current.pos;
        }

        UpdateEntityAppearances();
        UpdateMusic();

        VisualizeMouseInSector();

        if(Input.GetKeyDown(KeyCode.Z) && (int)mode < 3)
        {
            ShiftMode(1);
        }

        if(Input.GetKeyDown(KeyCode.M) && !system.IsPointerOverGameObject())
        {
            manual.ToggleActive();
        }

        switch (mode)
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
                if (!system.IsPointerOverGameObject())
                    PollSectors();
                break;
            case WCCursorMode.Control:
                RemovePendingSector();
                current.obj.SetActive(false);
                modeText.text = "Control Mode";
                if (!system.IsPointerOverGameObject())
                    PollControls();
                break;
            case WCCursorMode.SelectEntity:
                modeText.text = "Select Entity Mode"; // change only when mode changes?
                current.obj.SetActive(false); // same
                PollEntitySelection();
                break;
            case WCCursorMode.DrawPath:
                modeText.text = "Draw Path Mode";
                current.obj.SetActive(false);
                pathCreator.PollPathDrawing();
                break;
            default:
                break;
        }

        modeText.color = Camera.main.backgroundColor = modeColors[(int)mode];
        modeText.color += Color.gray;
        modeText.text = modeText.text.ToUpper();
    }

    void UpdateEntityAppearances()
    {
        foreach(var item in placedItems)
        {
            if(item.type == ItemType.Other || item.assetID == "core_gate" || item.assetID == "broken_core_gate")
            {
                foreach(var rend in item.obj.GetComponentsInChildren<SpriteRenderer>())
                {
                    rend.color = FactionManager.GetFactionColor(item.faction);
                }
            }
        }

        if(current.type == ItemType.Other || current.assetID == "core_gate" || current.assetID == "broken_core_gate")
        {
            foreach(var rend in current.obj.GetComponentsInChildren<SpriteRenderer>())
            {
                rend.color = FactionManager.GetFactionColor(current.faction);
            }
        }
    }

    public void UpdateCurrentAppearanceToDefault()
    {
        if(current == null) return;
        current.faction = PlayerPrefs.GetInt("WCItemPropertyDisplay_defaultFaction", 0);
        current.shellcoreJSON =  PlayerPrefs.GetString("WCItemPropertyDisplay_defaultJSON", "");
        if(current.type == ItemType.Other || current.assetID == "core_gate" || current.assetID == "broken_core_gate")
        {
            foreach(var rend in current.obj.GetComponentsInChildren<SpriteRenderer>())
            {
                rend.color = FactionManager.GetFactionColor(current.faction);
            }
        }
    }

    public Transform spawnPoint;
    bool changingSpawnPoint = false;
    public GUIWindowScripts taskInterface;
    void PollControls()
    {
        if(Input.GetMouseButtonUp(0))
        {
            if(changingSpawnPoint) changingSpawnPoint = false;
            else if(spawnPoint.GetComponent<SpriteRenderer>().bounds.Contains(GetMousePos()))
            {
                changingSpawnPoint = true;
            }
        }

        if(changingSpawnPoint)
        {
            spawnPoint.position = CalcSpawnPos();
        }

        if(Input.GetKeyUp(KeyCode.T))
        {
            if (taskInterface.GetActive())
                taskInterface.GetComponentInChildren<NodeEditorFramework.Standard.RTNodeEditor>().AutoSave();
            taskInterface.ToggleActive();
        }

        if(Input.GetKeyDown(KeyCode.C) && !system.IsPointerOverGameObject())
        {
            ActivateCharacterHandler();
        }
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
        wrapper.sector.bounds.y = Mathf.Max((int)wrapper.renderer.GetPosition(0).y, (int)wrapper.renderer.GetPosition(2).y);
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
                if(translatingSectors.Contains(sector))
                    renderer.startColor = renderer.endColor = Color.yellow;
                else renderer.startColor = renderer.endColor = Color.green;
            } 
            else if(translatingSectors.Contains(sector))
                renderer.startColor = renderer.endColor = Color.yellow;
            else renderer.startColor = renderer.endColor = Color.white;
        }
    }

    public void ActivateShipBuilder()
    {
        shipBuilder.Initialize(BuilderMode.Yard);
        shipBuilder.Activate();
    }

    public void ActivateWaveBuilder()
    {
        waveBuilder.ToggleActive();
    }

    public void ActivateCharacterHandler()
    {
        characterHandler.ToggleActive();
    }

    public int flagID = 0;
    void PollItems() {
        if(Input.GetKeyDown(KeyCode.B) && !system.IsPointerOverGameObject())
        {
            ActivateShipBuilder();
        }
        if(Input.GetKeyDown(KeyCode.V) && !system.IsPointerOverGameObject())
        {
            ActivateWaveBuilder();
        }

        if(!Input.GetKey(KeyCode.LeftControl) && !system.IsPointerOverGameObject())
        {
            if(Input.mouseScrollDelta.y < 0 && currentIndex > 0) SetCurrent(--currentIndex % maxIndex);
            else if(Input.mouseScrollDelta.y > 0 && currentIndex < maxIndex - 1) SetCurrent(++currentIndex % maxIndex);
        }
        
		
        if(GetItemUnderCursor(current.type) != null) 
        {
            Item underCursor;
            underCursor = (Item)GetItemUnderCursor(current.type);
            if(Input.GetMouseButtonDown(0) && !system.IsPointerOverGameObject() && current.obj) 
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
            else if((Input.GetMouseButtonUp(1) || (Input.GetMouseButton(1) && underCursor.type == ItemType.Platform)) && !system.IsPointerOverGameObject()) 
            {
                Remove((Item)underCursor);
            }
        } 
        else 
        {
            if(Input.GetKeyUp(KeyCode.R) && current.type == ItemType.Platform)
            {
                Rotate(current);
            }
            if(!system.IsPointerOverGameObject() && current.obj) 
            {
                if(Input.GetMouseButtonUp(0) || (Input.GetMouseButton(0) && current.type == ItemType.Platform))
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
        public Vector2[] originalRendererPos = new Vector2[4];
    }

    public List<SectorWCWrapper> sectors = new List<SectorWCWrapper>();

    public void Clear()
    {
        WCGeneratorHandler.DeleteTestWorld();
        while(placedItems.Count > 0)
        {
            Remove(placedItems[0]);
        }
        while(sectors.Count > 0)
        {
            RemoveSector(sectors[0]);
        }
        characters.Clear();
        characterHandler.ReflectButtonData();
    }

    void RemoveSector(SectorWCWrapper sector)
    {
        Destroy(sector.renderer.gameObject);
        if(sectors.Contains(sector)) sectors.Remove(sector);
    }

    Vector2 sectorStoredMousePos;

    Vector2 GetSectorOriginalPosition(LineRenderer renderer)
    {
        List<Vector2> origins = new List<Vector2>()
        {
            renderer.GetPosition(0),
            renderer.GetPosition(1),
            renderer.GetPosition(2),
            renderer.GetPosition(3),
        };

        var vec = GetMousePos();
        var finalOrigin = (Vector2)renderer.GetPosition(0);
        foreach(var origin in origins)
        {
            if((origin - vec).sqrMagnitude > (finalOrigin - vec).sqrMagnitude)
                finalOrigin = origin;
        }

        return finalOrigin;
    }

    List<SectorWCWrapper> translatingSectors = new List<SectorWCWrapper>();
    Vector2 sectorTranslationStoredPos;
    float doubleClickTimer;

    void PollSectors() 
    {
        if(!Input.GetKey(KeyCode.LeftShift) && translatingSectors.Count > 0)
        {
            FinalizeTranslation();
            translatingSectors.Clear();
            return;
        }
            
        if(Input.GetMouseButtonDown(0)) 
        {
            foreach(SectorWCWrapper sector in sectors)
            {
                LineRenderer renderer = sector.renderer;
                if(CheckMouseContainsSector(renderer)) 
                {
                    origPos = GetSectorOriginalPosition(renderer);
                    lastSectorPos = new Vector3[4];
                    for(int i = 0; i < 4; i++) 
                    {
                        lastSectorPos[i] = renderer.GetPosition(i);
                    }
                    //renderer.SetPosition(0, origPos);
                    if(Input.GetKey(KeyCode.LeftShift) && Time.time - doubleClickTimer < 0.2F)
                    {
                        if(!translatingSectors.Contains(sector))
                            translatingSectors.Add(sector);
                        else
                            translatingSectors.Remove(sector);
                        doubleClickTimer = 0;
                    }
                    else    
                        currentSector = sector;
                    doubleClickTimer = Time.time;
                    break;
                    
                }
            }

            sectorStoredMousePos = Input.mousePosition;
            if(Input.GetKey(KeyCode.LeftShift))
            {
                foreach(var sector in translatingSectors)
                {
                    for(int i = 0; i < 4; i++)
                        sector.originalRendererPos[i] = sector.renderer.GetPosition(i);
                }
                sectorTranslationStoredPos = CalcSectorPos();
            }
            else
            {
                
                
                if(currentSector == null) {
                    currentSector = new SectorWCWrapper();
                    currentSector.sector = ScriptableObject.CreateInstance<Sector>();
                    currentSector.sector.backgroundSpawns = new Sector.BackgroundSpawn[0];
                    currentSector.sector.hasMusic = true; // sectors have music by default in WC
                    currentSector.sector.backgroundColor = GetDefaultColor((Sector.SectorType)0);
                    currentSector.sector.rectangleEffectSkin = (RectangleEffectSkin)
                        PlayerPrefs.GetInt("WCSectorPropertyDisplay_defaultParticles", 0);
                    currentSector.sector.backgroundTileSkin = (BackgroundTileSkin)
                        PlayerPrefs.GetInt("WCSectorPropertyDisplay_defaultTiles", 0);
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
            
        }
        else if(Input.GetMouseButtonUp(0)) 
        {
            if(Input.GetKey(KeyCode.LeftShift))
            {
                FinalizeTranslation();
            }

            if((Vector2)Input.mousePosition == sectorStoredMousePos)
            {
                
                foreach(SectorWCWrapper sector in sectors)
                {
                    LineRenderer renderer = sector.renderer;
                    if(CheckMouseContainsSector(renderer) && !Input.GetKey(KeyCode.LeftShift)) 
                    {
                        sectorPropertyDisplay.DisplayProperties(sector.sector);
                        currentSector = null;
                        return;
                    }
                }

            }
            if(currentSector != null)
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
                    RemoveSector(currentSector);
                }
                currentSector = null; // reset reference
            }
            
        }
        else if(Input.GetMouseButton(0))
        {
            if(Input.GetKey(KeyCode.LeftShift))
            {
                foreach(var sector in translatingSectors)
                {
                    TranslateSector(sector, CalcSectorPos() - sectorTranslationStoredPos);
                    if(CheckForSectorOverlap(sector.renderer)) 
                    {
                        if(sector.renderer.sortingOrder < sortLayerNum)
                            sector.renderer.sortingOrder = ++sortLayerNum;
                        sector.renderer.startColor = sector.renderer.endColor = Color.red;
                    }
                }
            }
            else if(currentSector != null && (Vector2)Input.mousePosition != sectorStoredMousePos)
            {
                var renderer = currentSector.renderer;
                renderer.SetPosition(0, origPos);
                renderer.SetPosition(1, new Vector3(origPos.x, CalcSectorPos().y, 0));
                renderer.SetPosition(2, CalcSectorPos());
                renderer.SetPosition(3, new Vector3(CalcSectorPos().x, origPos.y, 0));
                SyncSectorCoords(currentSector);
                // check for overlap
                if(CheckForSectorOverlap(renderer)) 
                {
                    if(renderer.sortingOrder < sortLayerNum)
                        renderer.sortingOrder = ++sortLayerNum;
                    renderer.startColor = renderer.endColor = Color.red;
                }
            }
        }
        else if(Input.GetMouseButtonUp(1) && !Input.GetKey(KeyCode.LeftShift)) 
        {
            foreach(SectorWCWrapper sector in sectors) 
            {
                var renderer = sector.renderer;
                renderer.startColor = renderer.endColor = Color.white;
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

    void FinalizeTranslation()
    {
        foreach(var sector in translatingSectors)
        {
            if(CheckForSectorOverlap(sector.renderer))
            {
                foreach(var sector2 in translatingSectors)
                {
                    for(int i = 0; i < 4; i++)
                    {
                        sector2.renderer.SetPosition(i, sector2.originalRendererPos[i]);
                    }
                }
                break;
            }
        }
    }

    void TranslateSector(SectorWCWrapper sector, Vector2 offset)
    {
        for(int i = 0; i < 4; i++)
        {
            sector.renderer.SetPosition(i, sector.originalRendererPos[i] + offset);
        }
        SyncSectorCoords(sector);
    }

    public static Color GetDefaultColor(Sector.SectorType type)
    {
        return new Color(
            PlayerPrefs.GetFloat($"WCSectorPropertyDisplay_defaultR{(int)type}", 
                SectorColors.colors[(int)type].r),
            PlayerPrefs.GetFloat($"WCSectorPropertyDisplay_defaultG{(int)type}", 
                SectorColors.colors[(int)type].g),
            PlayerPrefs.GetFloat($"WCSectorPropertyDisplay_defaultB{(int)type}", 
                SectorColors.colors[(int)type].b)
        );
    }

    public void EntitySelection()
    {
        taskInterface.CloseUI();
        SetMode(WCCursorMode.SelectEntity);
    }

    void PollEntitySelection()
    {
        if (Input.GetMouseButtonUp(0) && !system.IsPointerOverGameObject())
        {
            var item = GetItemUnderCursor();
            if (item != null)
            {
                Item underCursor = new Item();
                underCursor = item;

                Debug.Log("under cursor: " + item);
                Debug.Log("under cursor name: " + item.name);
                Debug.Log("under cursor ID: " + item.ID);
                Debug.Log("under cursor assetID: " + item.assetID);

                // TODO: Specify which type of item you are scanning for
                if (underCursor.type == ItemType.Other || underCursor.type == ItemType.Decoration)
                {
                    taskInterface.Activate();
                    SetMode(originalCursorMode);
                    selectEntity.Invoke(underCursor.ID);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            taskInterface.Activate();
            SetMode(originalCursorMode);
            selectEntity.Invoke("");
        }
    }

    public void pathDrawing(WorldCreatorCursor.WCCursorMode originalMode, NodeEditorFramework.Standard.PathData path = null)
    {
        pathCreator.Clear();
        pathCreator.SetPath(path);

        taskInterface.CloseUI();
        originalCursorMode = originalMode;
        SetMode(WCCursorMode.DrawPath);
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

    // iterate through all sectors till you find the one that contains pos
    public SectorWCWrapper GetWrapperByPos(Vector2 pos)
    {
        foreach(var sector in sectors)
        {
            if(sector.sector.bounds.contains(pos)) return sector;
        }
        return null;
    }

    public SectorWCWrapper GetWrapperByPos(Item item)
    {
        return GetWrapperByPos(item.pos);
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

    Item GetItemUnderCursor(ItemType? type = null) {
        foreach(Item itemObj in placedItems) {
            if(itemObj.pos == current.pos && (type == null || type == itemObj.type)) {
                return itemObj;
            }
        }
        return null;
    }

    public void Remove(Item item) {
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

    public Vector2 CalcPos(ItemType type) {
        Vector3 mousePos = GetMousePos();
        if(type == ItemType.Platform) {
			mousePos.x = cursorOffset.x + tileSize * (int)((mousePos.x - cursorOffset.x) / tileSize + (mousePos.x / 2> 0 ? 0.5F : -0.5F));
			mousePos.y = cursorOffset.y + tileSize * (int)((mousePos.y - cursorOffset.y) / tileSize + (mousePos.y / 2> 0 ? 0.5F : -0.5F));
		} else {
			mousePos.x = 0.25F * tileSize * Mathf.RoundToInt((mousePos.x) / (0.25F * tileSize));
			mousePos.y = 0.25F * tileSize * Mathf.RoundToInt((mousePos.y) / (0.25F * tileSize));
		}
        return mousePos;
    }

    public static Vector2 GetMousePos() {
        Vector3 mousePos = Input.mousePosition;
		mousePos.z -= Camera.main.transform.position.z;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        return mousePos;
    }
    public Vector2 CalcSectorPos() 
    {
        Vector3 mousePos = GetMousePos();
        mousePos.x = tileSize * (int)((mousePos.x) / tileSize + (mousePos.x / 2> 0 ? 0.5F : -0.5F));
        mousePos.y = tileSize * (int)((mousePos.y) / tileSize + (mousePos.y / 2> 0 ? 0.5F : -0.5F));
        return mousePos;
    }

    public Vector2 CalcSpawnPos()
    {
        Vector3 mousePos = GetMousePos();
        mousePos.x = 0.5F * tileSize * Mathf.RoundToInt((mousePos.x) / (0.5F * tileSize));
        mousePos.y = 0.5F * tileSize * Mathf.RoundToInt((mousePos.y) / (0.5F * tileSize));
        return mousePos;
    }

    public Item CopyCurrent() 
    {
        var copy = handler.CopyItem(current);
        copy.obj.transform.position = copy.pos;

        return copy;
    }

    public void BumpCurrent(int val)
    {
        if(currentIndex + val < 0 || currentIndex + val >= handler.items.Count)
            return;
        SetCurrent(currentIndex + val);
    }

    public void SetCurrent(int index) 
    {
        if(current != null && current.obj) Destroy(current.obj);
        currentIndex = index;
        current = handler.GetItemByIndex(index);
        current.pos = CalcPos(current.type);
        current.obj.transform.position = current.pos;

        // item defaults go here
        current.faction = PlayerPrefs.GetInt("WCItemPropertyDisplay_defaultFaction", 0);
        current.shellcoreJSON = PlayerPrefs.GetString("WCItemPropertyDisplay_defaultJSON", "");
    }

    public void SetCurrent(Item item)
    {
        if(current != null && current.obj) Destroy(current.obj);
        current = item;
        current.pos = CalcPos(current.type);
        current.obj.transform.position = current.pos;
    }

    public void SetMode(WCCursorMode mode)
    {
        this.mode = mode;
        WCBetterBarHandler.UpdateActiveButtons();
        translatingSectors.Clear();
    }
}
