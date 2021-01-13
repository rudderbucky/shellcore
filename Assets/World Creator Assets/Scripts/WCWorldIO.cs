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

    string currentResourcePath = "";
    public SaveMenuHandler saveMenuHandler;

    public void PromptCurrentResourcePath()
    {
        if(currentResourcePath == "") return;
        saveMenuHandler.Activate(currentResourcePath);
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
        generatorHandler.ReadWorld(currentResourcePath);
        Hide();
    }

    public void TestWorld()
    {
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


    

    public GameObject window;
    public GameObject newWorldStack;
    public InputField field;
    public GameObject readButtons;
    void Show(IOMode mode)
    {
        active = true;
        gameObject.SetActive(true);
        window.SetActive(true);
        newWorldStack.SetActive(mode == IOMode.Write || mode == IOMode.WriteShipJSON || mode == IOMode.WriteWaveJSON); 
        DestroyAllButtons();
        this.mode = mode;
        string[] directories = null;

        readButtons.SetActive(mode == IOMode.Read);

        switch(mode)
        {
            case IOMode.Read:
            case IOMode.Write:
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
                            originalReadPath = dir;
                            worldPathName.text = System.IO.Path.GetFileName(dir);
                            WorldData wdata = ScriptableObject.CreateInstance<WorldData>();
                            JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(dir + "\\world.worlddata"), wdata);
                            authors.text = wdata.author;
                            currentResourcePath = dir;
                            description.text = wdata.description;
                            break;
                        case IOMode.Write:
                            if(dir.Contains("main"))
                                generatorHandler.WriteWorld(System.IO.Path.GetDirectoryName(dir) + "\\main - " + VersionNumberScript.version);
                            else generatorHandler.WriteWorld(dir);
                            Hide();
                            break;
                        case IOMode.ReadShipJSON:
                            builder.LoadBlueprint(System.IO.File.ReadAllText(dir));
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
    }
    
    public Text worldPathName;
    public InputField authors;
    public InputField description;

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
                    worldPathName.text = System.IO.Path.GetFileName(path);
                    WorldData wdata = ScriptableObject.CreateInstance<WorldData>();
                    JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(path + "\\world.worlddata"), wdata);
                    authors.text = wdata.author;
                    description.text = wdata.description;
                    currentResourcePath = path;
                    //generatorHandler.ReadWorld(path);
                    break;
                case IOMode.Write:
                    if(path.Contains("main"))
                        generatorHandler.WriteWorld(System.IO.Path.GetDirectoryName(path) + "\\main - " + VersionNumberScript.version);
                    else generatorHandler.WriteWorld(path);
                    Hide();
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
