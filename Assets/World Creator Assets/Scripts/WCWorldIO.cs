using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.EventSystems;
using System.Linq;
using NodeEditorFramework.Standard;
using NodeEditorFramework.IO;

public class WCWorldIO : GUIWindowScripts
{
    public WCGeneratorHandler generatorHandler;
    [SerializeField]
    private SelectionDisplayHandler displayHandler;
    public ShipBuilder builder;
    public WaveBuilder waveBuilder;
    public RTNodeEditor nodeEditor;
    public GameObject buttonPrefab;
    public Transform content;
    public RectTransform IOContainer;
    public GameObject worldContents;
    public static bool active = false;

    private string originalReadPath = "";
    public MapMakerScript mapMakerScript;
    public GameObject window;
    public GameObject newWorldButton;
    public InputField field;
    public Text readButton;
    private bool rwFromEntityPlaceholder;

    string placeholderPath;    
    [SerializeField]
    private Button switchBPDirectoryButton;
    private bool readingFromPresetBPs;

    private List<Button> buttons = new List<Button>();
    public Text worldPathName;
    public InputField authors;
    public InputField description;
    public InputField defaultBlueprint;
    public InputField newWorldInputField;
    [SerializeField]
    private InputField fileNameInputField;
    UnityAction clearAction;

    enum IOMode
    {
        Read,
        Write,
        ReadShipJSON,
        WriteShipJSON,
        ReadWaveJSON,
        WriteWaveJSON,
        ReadVendingBlueprintJSON,
        WriteVendingBlueprintJSON,
        ReadCanvas,
        WriteCanvas
    }

    IOMode mode = IOMode.Read;
    public static string PRESET_DIRECTORY = null;

    public void Quit()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowWaveReadMode()
    {
        IOContainer.sizeDelta = new Vector2(330, IOContainer.sizeDelta.y);
        worldContents.SetActive(false);
        Show(IOMode.ReadWaveJSON);
    }

    public void ShowWaveWriteMode()
    {
        IOContainer.sizeDelta = new Vector2(330, IOContainer.sizeDelta.y);
        worldContents.SetActive(false);
        Show(IOMode.WriteWaveJSON);
    }

    public void ShowShipReadMode(bool rdbValidty = false)
    {
        IOContainer.sizeDelta = new Vector2(330, IOContainer.sizeDelta.y);
        worldContents.SetActive(false);
        Show(IOMode.ReadShipJSON, rdbValidty);
    }

    public void ShowShipWriteMode()
    {
        IOContainer.sizeDelta = new Vector2(330, IOContainer.sizeDelta.y);
        worldContents.SetActive(false);
        Show(IOMode.WriteShipJSON);
    }

    public void ShowCanvasReadMode()
    {
        IOContainer.sizeDelta = new Vector2(330, IOContainer.sizeDelta.y);
        nodeEditor.enabled = false;
        worldContents.SetActive(false);
        Show(IOMode.ReadCanvas);
    }

    public void ShowCanvasWriteMode()
    {
        IOContainer.sizeDelta = new Vector2(330, IOContainer.sizeDelta.y);
        nodeEditor.enabled = false;
        worldContents.SetActive(false);
        Show(IOMode.WriteCanvas);
    }

    public void ShowReadMode()
    {
        IOContainer.sizeDelta = new Vector2(900, IOContainer.sizeDelta.y);
        worldContents.SetActive(true);
        Show(IOMode.Read);
    }

    public void ShowWriteMode()
    {
        IOContainer.sizeDelta = new Vector2(900, IOContainer.sizeDelta.y);
        worldContents.SetActive(true);
        Show(IOMode.Write);
    }

    public SaveMenuHandler saveMenuHandler;

    public void PromptCurrentResourcePath()
    {
        if (originalReadPath == "")
        {
            return;
        }

        saveMenuHandler.Activate(originalReadPath);
        Hide();
    }

//#if UNITY_EDITOR
    public static bool instantTest = false;
//#endif

    [SerializeField]
    private Text loadingText;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "WorldCreator")
        {
            var path = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", "TestWorld");
            DeletePlaceholderDirectories();
            if (Directory.Exists(path))
            {
                generatorHandler.ReadWorld(path);
//#if UNITY_EDITOR
                if (instantTest)
                {
                    TestWorld();
                }
//#endif
            }
        }
    }

    public override bool GetActive() {
        return gameObject.activeSelf;
    }

    public static void DeletePlaceholderDirectories()
    {
        var CanvasPlaceholder = System.IO.Path.Combine(Application.streamingAssetsPath, "CanvasPlaceholder");
        if (System.IO.Directory.Exists(CanvasPlaceholder))
        {
            foreach (var file in System.IO.Directory.GetFiles(CanvasPlaceholder))
            {
                System.IO.File.Delete(file);
            }

            System.IO.Directory.Delete(CanvasPlaceholder);
        }

        var EntityPlaceholder = System.IO.Path.Combine(Application.streamingAssetsPath, "EntityPlaceholder");
        if (System.IO.Directory.Exists(EntityPlaceholder))
        {
            foreach (var file in System.IO.Directory.GetFiles(EntityPlaceholder))
            {
                System.IO.File.Delete(file);
            }

            System.IO.Directory.Delete(EntityPlaceholder);
        }

        var WavePlaceholder = System.IO.Path.Combine(Application.streamingAssetsPath, "WavePlaceholder");
        if (System.IO.Directory.Exists(WavePlaceholder))
        {
            foreach (var file in System.IO.Directory.GetFiles(WavePlaceholder))
            {
                System.IO.File.Delete(file);
            }

            System.IO.Directory.Delete(WavePlaceholder);
        }

        var FactionPlaceholder = System.IO.Path.Combine(Application.streamingAssetsPath, "FactionPlaceholder");
        if (System.IO.Directory.Exists(FactionPlaceholder))
        {
            foreach (var file in System.IO.Directory.GetFiles(FactionPlaceholder))
            {
                System.IO.File.Delete(file);
            }

            System.IO.Directory.Delete(FactionPlaceholder);
        }

        var ResourcePlaceholder = System.IO.Path.Combine(Application.streamingAssetsPath, "ResourcePlaceholder");
        if (System.IO.Directory.Exists(ResourcePlaceholder))
        {
            foreach (var file in System.IO.Directory.GetFiles(ResourcePlaceholder))
            {
                System.IO.File.Delete(file);
            }

            System.IO.Directory.Delete(ResourcePlaceholder);
        }

        if (System.IO.File.Exists(System.IO.Path.Combine(Application.streamingAssetsPath, "ResourceDataPlaceholder.txt")))
        {
            File.Delete(System.IO.Path.Combine(Application.streamingAssetsPath, "ResourceDataPlaceholder.txt"));
        }
    }

    public void WCReadCurrentPath()
    {
        if (mode == IOMode.Read)
        {
            WorldCreatorCursor.instance.Clear();
            generatorHandler.ReadWorld(originalReadPath);
            WorldCreatorCursor.instance.propertyDisplay.LoadFactions();
        }
        else if (mode == IOMode.Write)
        {
            if (originalReadPath.Contains("main"))
            {
                generatorHandler.WriteWorld(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(originalReadPath), "main - " + VersionNumberScript.version));
            }
            else
            {
                generatorHandler.WriteWorld(originalReadPath);
            }
        }

        Hide();
    }

    public void TestWorld()
    {
        // TODO: copy custom resources
        var path = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", "TestWorld");
        if (generatorHandler.WriteWorld(path))
        {
            generatorHandler.OnSectorSaved.AddListener(OnWorldSaved);
        }
    }

    void OnWorldSaved() // ...and they lived happily ever after. Or did they?
    {
        if (generatorHandler.saveState == 2)
        {
            LoadTestSave(originalReadPath);
        }
        else
        {
            Debug.LogWarning("Something went wrong, testing aborted.");
        }
    }

    public static void LoadTestSave(string originalReadPath, bool usePassedPathForWorld = false)
    {
        var path = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", "TestWorld");
        if (usePassedPathForWorld) path = originalReadPath;
        var savePath = System.IO.Path.Combine(Application.persistentDataPath, "Saves", "TestSave");
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }

        SaveMenuHandler.CreateSave("TestSave");
        SectorManager.testJsonPath = path;
        SectorManager.testResourcePath = originalReadPath;
        SaveMenuIcon.LoadSaveByPath(savePath, false);
    }

    IEnumerator coroutine;

    void SetWorldIndicators(string path)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = ReadAllSectors(path);
        StartCoroutine(coroutine);
        worldPathName.text = "Currently selected: " + System.IO.Path.GetFileName(path);
        WorldData wdata = ScriptableObject.CreateInstance<WorldData>();
        try
        {
            JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(System.IO.Path.Combine(path, "world.worlddata")), wdata);
            authors.text = wdata.author;
            description.text = wdata.description;
            defaultBlueprint.text = wdata.defaultBlueprintJSON;
            Debug.Log(wdata.defaultBlueprintJSON);
        }
        catch (System.Exception e)
        {
            authors.text = description.text = "";
            Debug.Log(e);
        }

        originalReadPath = path;
        foreach (var button in buttons)
        {
            button.image.color = new Color32(60, 60, 60, 255);
        }
    }


    private string GetLoadingString()
    {
        float dots = (Time.time * 2) % 4;
        var text = "Loading map";
        for (int i = 0; i < (int)dots; i++)
        {
            text += ".";
        }

        return text;
    }

    IEnumerator ReadAllSectors(string path)
    {
        loadingText.gameObject.SetActive(true);
        loadingText.text = GetLoadingString();
        var skippedFiles = new List<string> { ".meta", ".worlddata", ".taskdata", ".dialoguedata", ".sectordata", "ResourceData.txt" , ".DS_Store"};
        List<Sector> sectors = new List<Sector>();
        foreach (var str in System.IO.Directory.GetFiles(path))
        {
            if (skippedFiles.Exists(s => str.Contains(s)))
            {
                continue;
            }

            string sectorjson = System.IO.File.ReadAllText(str);
            Sector.SectorData data = JsonUtility.FromJson<Sector.SectorData>(sectorjson);
            Sector curSect = ScriptableObject.CreateInstance<Sector>();
            JsonUtility.FromJsonOverwrite(data.sectorjson, curSect);
            sectors.Add(curSect);
            loadingText.text = GetLoadingString();
            yield return null;
        }

        MapMakerScript.Redraw(sectors);
        loadingText.gameObject.SetActive(false);
    }


    void Show(IOMode mode)
    {
        Show(mode, false);
    }


    void Show(IOMode mode, bool displayRdbValidity = false)
    {
        clearAction = () => {
            if (displayHandler && builder && builder.currentPartHandler)
            {
                displayHandler.ClearDisplay();
                builder.currentPartHandler.SetActive(true);
            }

            if (nodeEditor && !nodeEditor.enabled) nodeEditor.enabled = true;
        };
        rwFromEntityPlaceholder = SceneManager.GetActiveScene().name != "SampleScene";
        placeholderPath = System.IO.Path.Combine(Application.streamingAssetsPath, "EntityPlaceholder");
        var isInMainMenu = SceneManager.GetActiveScene().name == "MainMenu";
        if (isInMainMenu)
        {
            var serverPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", VersionNumberScript.rdbMap, "Entities");
            if (Directory.Exists(serverPath))
            {
                placeholderPath = serverPath;
            }
            else placeholderPath = "";
        }
        PRESET_DIRECTORY = System.IO.Path.Combine(Application.persistentDataPath, "PresetBlueprints");
        if ((mode != IOMode.Read && mode != IOMode.Write) && 
            !Directory.Exists(placeholderPath) && rwFromEntityPlaceholder)
        {
            rwFromEntityPlaceholder = false;
        }
        buttons.Clear();
        active = true;
        gameObject.SetActive(true);
        window.SetActive(true);
        bool writing = mode == IOMode.Write || mode == IOMode.WriteShipJSON || mode == IOMode.WriteWaveJSON || mode == IOMode.WriteCanvas;
        DestroyAllButtons();
        this.mode = mode;
        string[] directories = null;

        readButton.gameObject.SetActive(mode == IOMode.Read || mode == IOMode.Write);

        PlayerViewScript.SetCurrentWindow(this);
        switchBPDirectoryButton.gameObject.SetActive(false);
        switchBPDirectoryButton.onClick.RemoveAllListeners();
        fileNameInputField.gameObject.SetActive(writing);
        newWorldButton.gameObject.SetActive(writing);
        GetComponent<RectTransform>().anchoredPosition = GetComponentsInChildren<RectTransform>()[1].anchoredPosition = Vector2.zero;
        if (!Directory.Exists(PRESET_DIRECTORY))
        {
            Directory.CreateDirectory(PRESET_DIRECTORY);
        }

        var wrongReadingFromBPS = (readingFromPresetBPs == rwFromEntityPlaceholder);
        if (wrongReadingFromBPS) SwitchBPDirectory();
        if (loadingText)
        {
            loadingText.text = "If you select a world, its map will appear here.";
            loadingText.gameObject.SetActive(true);
        } 
        switch (mode)
        {
            case IOMode.Read:
                readButton.text = "Read world";
                worldPathName.text = "If you select a world, its name will appear here.";
                authors.text = "";
                description.text = "";
                defaultBlueprint.text = "";
                authors.placeholder.GetComponent<Text>().text = "World authors appear here";
                description.placeholder.GetComponent<Text>().text = "World description appears here";
                defaultBlueprint.placeholder.GetComponent<Text>().text = "Default blueprint appears here";
                directories = Directory.GetDirectories(System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors"));
                break;
            case IOMode.Write:
                readButton.text = "Write world";
                worldPathName.text = "If you select a world, its name will appear here.";
                authors.text = "";
                description.text = "";
                defaultBlueprint.text = "";
                authors.placeholder.GetComponent<Text>().text = "Enter world authors here";
                description.placeholder.GetComponent<Text>().text = "Enter world description here";
                defaultBlueprint.placeholder.GetComponent<Text>().text = "Enter default blueprint here";
                directories = Directory.GetDirectories(System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors"));
                break;
            case IOMode.ReadShipJSON:
            case IOMode.WriteShipJSON:
                switchBPDirectoryButton.gameObject.SetActive(rwFromEntityPlaceholder);
                List<string> files = new List<string>();
                if (rwFromEntityPlaceholder && !String.IsNullOrEmpty(placeholderPath)) files.AddRange(Directory.GetFiles(placeholderPath));
                files.AddRange(Directory.GetFiles(PRESET_DIRECTORY));
                directories = files.ToArray();
                break;
            case IOMode.ReadWaveJSON:
            case IOMode.WriteWaveJSON:
                var path = System.IO.Path.Combine(Application.streamingAssetsPath, "WavePlaceholder");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                directories = Directory.GetFiles(path);
                break;
            case IOMode.ReadCanvas:
            case IOMode.WriteCanvas:
                path = System.IO.Path.Combine(Application.streamingAssetsPath, "CanvasPlaceholder");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                directories = Directory.GetFiles(path);
                break;

        }

        foreach (var dir in directories)
        {
            if (!dir.Contains("TestWorld") && !dir.Contains("meta"))
            {
                AddButton(dir, new UnityEngine.Events.UnityAction(() =>
                {
                    switch (mode)
                    {
                        case IOMode.Read:
                        case IOMode.Write:
                            SetWorldIndicators(dir);
                            break;
                        case IOMode.ReadShipJSON:
                            ReadShipJSON(dir);
                            Hide();
                            break;
                        case IOMode.WriteShipJSON:
                            ShipBuilder.SaveBlueprint(null, dir, builder.GetCurrentJSON());
                            Hide();
                            break;
                        case IOMode.ReadWaveJSON:
                            waveBuilder.ReadWaves(JsonUtility.FromJson<WaveSet>(System.IO.File.ReadAllText(dir)));
                            Hide();
                            break;
                        case IOMode.WriteWaveJSON:
                            waveBuilder.ParseWaves(dir);
                            Hide();
                            break;
                        case IOMode.ReadCanvas:
                            nodeEditor.enabled = true;
                            var intf = nodeEditor.GetEditorInterface();
                            intf.canvasCache.SetCanvas(ImportExportManager.ImportCanvas(intf.GetImportExportFormat(), new object[] {dir}));
                            NodeEditorInterface.forceUpdateCanvasUI = true;
                            Hide();
                            break;
                        case IOMode.WriteCanvas:
                            nodeEditor.enabled = true;
                            intf = nodeEditor.GetEditorInterface();
                            ImportExportManager.ExportCanvas(intf.canvasCache.nodeCanvas, intf.GetImportExportFormat(), dir);
                            Hide();
                            break;
                    }
                }), displayRdbValidity);
            }
        }
        GetComponentInParent<Canvas>().sortingOrder = ++PlayerViewScript.currentLayer; // move window to top
        GetComponentsInChildren<SubcanvasSortingOrder>(true).ToList().ForEach(x => x.Initialize());
    }

    public void SwitchBPDirectory()
    {
        readingFromPresetBPs = !readingFromPresetBPs;
        switchBPDirectoryButton.GetComponentInChildren<Text>().text = readingFromPresetBPs ? "Preset BPs" : "World BPs";
    }
    void AddButton(string name, UnityAction action, bool displayRdbValidity = false)
    {
        var rdbValid = false;
        if (displayRdbValidity)
        {
            var output = "";
            rdbValid = MasterNetworkAdapter.ValidateBluperintOnServer(SectorManager.TryGettingEntityBlueprint(File.ReadAllText(name)), out output);
        }
        var isPreset = name.Contains("PresetBlueprints");
        var isInMainMenu = (SceneManager.GetActiveScene().name == "MainMenu");

        if (!rdbValid && isInMainMenu && !isPreset && displayRdbValidity) return;
        if (rdbValid && isInMainMenu && isPreset && !readingFromPresetBPs) switchBPDirectoryButton.onClick.Invoke();


        var button = Instantiate(buttonPrefab, content).GetComponent<Button>();
        button.onClick.AddListener(
            () =>
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    File.Delete(name);
                    Destroy(button.gameObject);
                    return;
                }
                action.Invoke();
            });
        button.onClick.AddListener(clearAction);
        if (isPreset)
        {
            var assignTrigger = new EventTrigger.TriggerEvent();
            var print = SectorManager.TryGettingEntityBlueprint(File.ReadAllText(name));
            assignTrigger.AddListener((e) => {
                
                if (displayHandler && print)
                {
                    displayHandler.AssignDisplay(print, null);
                    builder.currentPartHandler.SetActive(false);
                }
            });
            var unassignTrigger = new EventTrigger.TriggerEvent();
            unassignTrigger.AddListener((e) => {
                clearAction.Invoke();
            });

            var eventTrigger = button.gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerEnter,
                callback = assignTrigger
            });

            eventTrigger.triggers.Add(new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerExit,
                callback = unassignTrigger
            });
        }
        
        var nameText = System.IO.Path.GetFileName(name);
        if (mode != IOMode.Read && mode != IOMode.ReadCanvas && mode != IOMode.WriteCanvas)
            nameText = System.IO.Path.GetFileNameWithoutExtension(name);
        button.GetComponentInChildren<Text>().text = nameText;
        if (PlayerCore.Instance)
        {
            var print = SectorManager.TryGettingEntityBlueprint(File.ReadAllText(name));
            if (print && print.parts != null && !builder.ContainsParts(print.parts))
            {
                button.GetComponentInChildren<Text>().text = System.IO.Path.GetFileNameWithoutExtension(name) + " (Inadequate parts)";
                button.GetComponentInChildren<Text>().color = Color.red;
            }
            if (!ShipBuilder.ValidateBlueprint(print, false, PlayerCore.Instance.blueprint.coreShellSpriteID, true, PlayerCore.Instance.abilityCaps))
            {
                button.GetComponentInChildren<Text>().text = System.IO.Path.GetFileNameWithoutExtension(name) + " (Invalid)";
                button.GetComponentInChildren<Text>().color = Color.red;
            }
        }
        buttons.Add(button);
        if (displayRdbValidity)
        {
            if (!rdbValid) 
            {

                button.GetComponentInChildren<Text>().color = Color.red;
                button.GetComponentInChildren<Text>().text = System.IO.Path.GetFileNameWithoutExtension(name) + " (rdb server invalid)";
            }
        }


        if (mode == IOMode.ReadShipJSON || mode == IOMode.WriteShipJSON)
        {
            button.gameObject.SetActive(readingFromPresetBPs == name.Contains("PresetBlueprints"));
            switchBPDirectoryButton.onClick.AddListener(() => {
                button.gameObject.SetActive(readingFromPresetBPs == name.Contains("PresetBlueprints"));
            });
        }
    }

    public void OpenNewWorldPrompt()
    {
        newWorldInputField.transform.parent.GetComponentInChildren<GUIWindowScripts>().ToggleActive();
        newWorldInputField.transform.parent.Find("Background").GetComponentInChildren<Text>().text = "Name your World:\n" +
                                                                                                     "(Warning: the contents of the World Creator will immediately be written into the new folder.)";
        newWorldInputField.transform.parent.Find("Create Save").GetComponentInChildren<Text>().text = "Create World!";
    }

    public void AddButtonFromField()
    {
        if (String.IsNullOrEmpty(field.text) || field.text == "main")
        {
            return;
        }

        string path = null;

        switch (mode)
        {
            case IOMode.Read:
            case IOMode.Write:
                path = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", field.text);
                break;
            case IOMode.ReadShipJSON:
            case IOMode.WriteShipJSON:
                path = readingFromPresetBPs ? System.IO.Path.Combine(Application.persistentDataPath, "PresetBlueprints", field.text + ".json") :
                    System.IO.Path.Combine(Application.streamingAssetsPath, "EntityPlaceholder", field.text + ".json");
                break;
            case IOMode.ReadWaveJSON:
            case IOMode.WriteWaveJSON:
                path = System.IO.Path.Combine(Application.streamingAssetsPath, "WavePlaceholder", field.text + ".json");
                break;
            case IOMode.WriteCanvas:
                path = System.IO.Path.Combine(Application.streamingAssetsPath, "CanvasPlaceholder", field.text + ImportExportFormat.GetCanvasExtension());
                break;
        }

        if (!Directory.Exists(path) && (mode == IOMode.Read || mode == IOMode.Write))
        {
            Directory.CreateDirectory(path);
        }

        switch (mode)
        {
            case IOMode.Write:
                originalReadPath = path;
                WCReadCurrentPath();
                break;
            case IOMode.ReadShipJSON:
                ReadShipJSON(path);
                Hide();
                break;
            case IOMode.WriteShipJSON:
                ShipBuilder.SaveBlueprint(null, path, builder.GetCurrentJSON());
                Hide();
                break;
            case IOMode.ReadWaveJSON:
                waveBuilder.ReadWaves(JsonUtility.FromJson<WaveSet>(System.IO.File.ReadAllText(path)));
                Hide();
                break;
            case IOMode.WriteWaveJSON:
                waveBuilder.ParseWaves(path);
                Hide();
                break;
            case IOMode.WriteCanvas:
                nodeEditor.enabled = true;
                var intf = nodeEditor.GetEditorInterface();
                ImportExportManager.ExportCanvas(intf.canvasCache.nodeCanvas, intf.GetImportExportFormat(), path);
                break;
            default:
                break;
        }
    }

    [SerializeField]
    private InputField[] blueprintFields;

    private void ReadShipJSON(string path)
    {
        rwFromEntityPlaceholder = SceneManager.GetActiveScene().name != "SampleScene";
        foreach (var field in blueprintFields)
        {
            field.text = System.IO.Path.GetFileNameWithoutExtension(path);
        }
        if (rwFromEntityPlaceholder && builder)
            builder.LoadBlueprint(System.IO.File.ReadAllText(path));
        else if (PlayerCore.Instance)
        {
            var print = SectorManager.TryGettingEntityBlueprint(File.ReadAllText(path));
            var validPreset = print && ShipBuilder.ValidateBlueprint(print, false, PlayerCore.Instance.blueprint.coreShellSpriteID, false, PlayerCore.Instance.abilityCaps) && builder.ContainsParts(print.parts);

            if (validPreset || DevConsoleScript.godModeEnabled)
                LoadPreset(print);
        }
    }
    void DestroyAllButtons()
    {
        buttons.Clear();
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    private void LoadPreset(EntityBlueprint blueprint)
    {
        if (!blueprint) return;
        if (blueprint.parts == null) return;
        if (builder && builder.cursorScript) 
        {
            builder.cursorScript.ClearAllParts();
            foreach (EntityBlueprint.PartInfo info in blueprint.parts)
            {
                if (!builder.DecrementPartButton(ShipBuilder.CullSpatialValues(info)))
                {
                    builder.CloseUI(false);
                    return;
                }
            }
        }

        var x = new EntityBlueprint.PartInfo[blueprint.parts.Count];
        blueprint.parts.CopyTo(x);
        PlayerCore.Instance.blueprint.parts = new List<EntityBlueprint.PartInfo>(x);
        builder.CloseUI(true);
        PlayerCore.Instance.Rebuild();
    }

    public void Hide()
    {
        active = false;
        DestroyAllButtons();
        gameObject.SetActive(false);
        window.SetActive(false);
        if (clearAction != null) clearAction.Invoke();
    }

    public override void CloseUI()
    {
        Hide();            
        if (playSoundOnClose)
        {
            AudioManager.PlayClipByID("clip_back", true);
        }
    }
}
