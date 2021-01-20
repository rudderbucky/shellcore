using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

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

    string originalReadPath = "";

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
        if(originalReadPath == "") return;
        saveMenuHandler.Activate(originalReadPath);
        Hide();
    }

    #if UNITY_EDITOR
    private bool instantTest = false;
    #endif

    void Start()
    {
        if(SceneManager.GetActiveScene().name == "WorldCreator")
        {
            var path = Application.streamingAssetsPath + "\\Sectors\\TestWorld";
            if(Directory.Exists(path)) 
            {
                generatorHandler.ReadWorld(path);
                #if UNITY_EDITOR
                if(instantTest) TestWorld();
                #endif
            }
            else
            {
                DeletePlaceholderDirectories();
            }
        }
    }

    public static void DeletePlaceholderDirectories()
    {
        if(System.IO.Directory.Exists(Application.streamingAssetsPath + "\\CanvasPlaceholder"))
        {
            foreach(var file in System.IO.Directory.GetFiles(Application.streamingAssetsPath + "\\CanvasPlaceholder"))
            {
                System.IO.File.Delete(file);
            }
            System.IO.Directory.Delete(Application.streamingAssetsPath + "\\CanvasPlaceholder");
        }

        if(System.IO.Directory.Exists(Application.streamingAssetsPath + "\\EntityPlaceholder"))
        {
            foreach(var file in System.IO.Directory.GetFiles(Application.streamingAssetsPath + "\\EntityPlaceholder"))
            {
                System.IO.File.Delete(file);
            }
            System.IO.Directory.Delete(Application.streamingAssetsPath + "\\EntityPlaceholder");
        }

        if(System.IO.Directory.Exists(Application.streamingAssetsPath + "\\WavePlaceholder"))
        {
            foreach(var file in System.IO.Directory.GetFiles(Application.streamingAssetsPath + "\\WavePlaceholder"))
            {
                System.IO.File.Delete(file);
            }
            System.IO.Directory.Delete(Application.streamingAssetsPath + "\\WavePlaceholder");
        }
    }

    public void WCReadCurrentPath()
    {
        if(mode == IOMode.Read)
        {
            WorldCreatorCursor.instance.Clear();
            generatorHandler.ReadWorld(originalReadPath);
        } 
        else if(mode == IOMode.Write)
        {
            if(originalReadPath.Contains("main"))
                generatorHandler.WriteWorld(System.IO.Path.GetDirectoryName(originalReadPath) + "\\main - " + VersionNumberScript.version);
            else generatorHandler.WriteWorld(originalReadPath);
        }
        Hide();
    }

    public void TestWorld()
    {
        // TODO: copy custom resources
        var path = Application.streamingAssetsPath + "\\Sectors\\TestWorld";
        if(generatorHandler.WriteWorld(path))
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
        var path = Application.streamingAssetsPath + "\\Sectors\\TestWorld";
        var savePath = Application.persistentDataPath + "\\Saves\\TestSave";
        if (File.Exists(savePath))
            File.Delete(savePath);
        SaveMenuHandler.CreateSave("TestSave");
        SectorManager.testJsonPath = path;
        SectorManager.testResourcePath = originalReadPath;
        SaveMenuIcon.LoadSaveByPath(savePath, false);
    }

    void SetWorldIndicators(string path)
    {
        worldPathName.text = "Currently selected: " + System.IO.Path.GetFileName(path);
        WorldData wdata = ScriptableObject.CreateInstance<WorldData>();
        try
        {
            JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(path + "\\world.worlddata"), wdata);
            authors.text = wdata.author;
            description.text = wdata.description;
            defaultBlueprint.text = wdata.defaultBlueprintJSON;
            Debug.Log(wdata.defaultBlueprintJSON);
        }
        catch(System.Exception e)
        {
            authors.text = 
            description.text = "";
            Debug.Log(e);
        }
        originalReadPath = path;
        foreach(var button in buttons)
        {
            button.image.color = new Color32(60,60,60,255);
        }
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

        switch(mode)
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
                directories = Directory.GetDirectories(Application.streamingAssetsPath + "\\Sectors");
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
                directories = Directory.GetDirectories(Application.streamingAssetsPath + "\\Sectors");
                break;
            case IOMode.ReadShipJSON:
            case IOMode.WriteShipJSON:
                directories = Directory.GetFiles(Application.streamingAssetsPath + "\\EntityPlaceholder");
                break;
            case IOMode.ReadWaveJSON:
            case IOMode.WriteWaveJSON:
                directories = Directory.GetFiles(Application.streamingAssetsPath + "\\WavePlaceholder");
                break;
        }

        foreach(var dir in directories)
        {
            if(!dir.Contains("TestWorld") && !dir.Contains("meta"))
                AddButton(dir, new UnityEngine.Events.UnityAction(() => {
                    switch(mode)
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

    public void OpenNewWorldPrompt() {
		newWorldInputField.transform.parent.GetComponentInChildren<GUIWindowScripts>().ToggleActive();
		newWorldInputField.transform.parent.Find("Background").GetComponentInChildren<Text>().text = "Name your World:\n" +
		"(Warning: the contents of the World Creator will immediately be written into the new folder.)";
		newWorldInputField.transform.parent.Find("Create Save").GetComponentInChildren<Text>().text = "Create World!";
	}

    public void AddButtonFromField()
    {
        if(field.text == "main") return;
        string path = null;

        switch(mode)
        {
            case IOMode.Read:
            case IOMode.Write:
                path = Application.streamingAssetsPath + "\\Sectors\\" + field.text;
                break;
            case IOMode.ReadShipJSON:
            case IOMode.WriteShipJSON:
                path = Application.streamingAssetsPath + "\\EntityPlaceholder\\" + field.text + ".json";
                break;
            case IOMode.ReadWaveJSON:
            case IOMode.WriteWaveJSON:
                path = Application.streamingAssetsPath + "\\WavePlaceholder\\" + field.text + ".json";
                break;
        }

        if(!Directory.Exists(path) && (mode == IOMode.Read || mode == IOMode.Write)) Directory.CreateDirectory(path);
        AddButton(path, new UnityEngine.Events.UnityAction(() => {
            switch(mode)
            {
                case IOMode.Read:
                case IOMode.Write:
                    SetWorldIndicators(path);
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
            }
        }));
}

    void DestroyAllButtons()
    {
        buttons.Clear();
        for(int i = 0; i < content.childCount; i++)
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
