using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WCWorldIO : MonoBehaviour
{
    public WCGeneratorHandler generatorHandler;
    public ShipBuilder builder;
    public WaveBuilder waveBuilder;
    public GameObject buttonPrefab;
    public Transform content;
    public RectTransform IOContainer;
    public GameObject worldContents;
    public static bool active = false;

    private string originalReadPath = "";

    enum IOMode
    {
        Read,
        Write,
        ReadShipJSON,
        WriteShipJSON,
        ReadWaveJSON,
        WriteWaveJSON
    }

    IOMode mode = IOMode.Read;

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

    public void ShowShipReadMode()
    {
        IOContainer.sizeDelta = new Vector2(330, IOContainer.sizeDelta.y);
        worldContents.SetActive(false);
        Show(IOMode.ReadShipJSON);
    }

    public void ShowShipWriteMode()
    {
        IOContainer.sizeDelta = new Vector2(330, IOContainer.sizeDelta.y);
        worldContents.SetActive(false);
        Show(IOMode.WriteShipJSON);
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

#if UNITY_EDITOR
    private bool instantTest = false;
#endif

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
#if UNITY_EDITOR
                if (instantTest)
                {
                    TestWorld();
                }
#endif
            }
        }
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
            LoadTestSave();
        }
        else
        {
            Debug.LogWarning("Something went wrong, testing aborted.");
        }
    }

    void LoadTestSave()
    {
        var path = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", "TestWorld");
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

    void SetWorldIndicators(string path)
    {
        StopAllCoroutines();
        StartCoroutine(ReadAllSectors(path));
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

    public MapMakerScript mapMakerScript;

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
        var skippedFiles = new List<string> { ".meta", ".worlddata", ".taskdata", ".dialoguedata", ".sectordata", "ResourceData.txt" };
        List<Sector> sectors = new List<Sector>();
        foreach (var str in System.IO.Directory.GetFiles(path))
        {
            if (skippedFiles.Exists(s => str.Contains(s)))
            {
                continue;
            }

            string sectorjson = System.IO.File.ReadAllText(str);
            SectorCreatorMouse.SectorData data = JsonUtility.FromJson<SectorCreatorMouse.SectorData>(sectorjson);
            // Debug.Log("Platform JSON: " + data.platformjson);
            // Debug.Log("Sector JSON: " + data.sectorjson);
            Sector curSect = ScriptableObject.CreateInstance<Sector>();
            JsonUtility.FromJsonOverwrite(data.sectorjson, curSect);
            sectors.Add(curSect);
            loadingText.text = GetLoadingString();
            yield return null;
        }

        MapMakerScript.Redraw(sectors);
        loadingText.gameObject.SetActive(false);
    }

    public GameObject window;
    public GameObject newWorldStack;
    public InputField field;
    public Text readButton;

    void Show(IOMode mode)
    {
        buttons.Clear();
        active = true;
        gameObject.SetActive(true);
        window.SetActive(true);
        newWorldStack.SetActive(mode == IOMode.Write || mode == IOMode.WriteShipJSON || mode == IOMode.WriteWaveJSON);
        DestroyAllButtons();
        this.mode = mode;
        string[] directories = null;

        readButton.gameObject.SetActive(mode == IOMode.Read || mode == IOMode.Write);

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
                directories = Directory.GetFiles(System.IO.Path.Combine(Application.streamingAssetsPath, "EntityPlaceholder"));
                break;
            case IOMode.ReadWaveJSON:
            case IOMode.WriteWaveJSON:
                directories = Directory.GetFiles(System.IO.Path.Combine(Application.streamingAssetsPath, "WavePlaceholder"));
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
                            builder.LoadBlueprint(System.IO.File.ReadAllText(dir));
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
                    }
                }));
            }
        }
    }

    void AddButton(string name, UnityAction action)
    {
        var button = Instantiate(buttonPrefab, content).GetComponent<Button>();
        button.onClick.AddListener(action);
        button.GetComponentInChildren<Text>().text = System.IO.Path.GetFileName(name);
        buttons.Add(button);
    }

    private List<Button> buttons = new List<Button>();

    public Text worldPathName;
    public InputField authors;
    public InputField description;
    public InputField defaultBlueprint;
    public InputField newWorldInputField;

    public void OpenNewWorldPrompt()
    {
        newWorldInputField.transform.parent.GetComponentInChildren<GUIWindowScripts>().ToggleActive();
        newWorldInputField.transform.parent.Find("Background").GetComponentInChildren<Text>().text = "Name your World:\n" +
                                                                                                     "(Warning: the contents of the World Creator will immediately be written into the new folder.)";
        newWorldInputField.transform.parent.Find("Create Save").GetComponentInChildren<Text>().text = "Create World!";
    }

    public void AddButtonFromField()
    {
        if (field.text == "main")
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
                path = System.IO.Path.Combine(Application.streamingAssetsPath, "EntityPlaceholder", field.text + ".json");
                break;
            case IOMode.ReadWaveJSON:
            case IOMode.WriteWaveJSON:
                path = System.IO.Path.Combine(Application.streamingAssetsPath, "WavePlaceholder", field.text + ".json");
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
                builder.LoadBlueprint(System.IO.File.ReadAllText(path));
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
            default:
                break;
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

    public void Hide()
    {
        active = false;
        DestroyAllButtons();
        gameObject.SetActive(false);
        window.SetActive(false);
    }
}
