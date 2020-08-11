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
    public InputField blueprintField;
    public InputField checkpointField;
    public static bool active = false;

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
        Show(IOMode.ReadWaveJSON);
    }

    public void ShowWaveWriteMode()
    {
        Show(IOMode.WriteWaveJSON);
    }

    public void ShowShipReadMode()
    {
        Show(IOMode.ReadShipJSON);
    }

    public void ShowShipWriteMode()
    {
        Show(IOMode.WriteShipJSON);
    }

    public void ShowReadMode()
    {
        Show(IOMode.Read);
    }

    #if UNITY_EDITOR
    private bool instantTest = false;
    #endif

    void Start()
    {
        if(SceneManager.GetActiveScene().name == "WorldCreator")
        {
            blueprintField.text = PlayerPrefs.GetString("WorldCreator_playerBlueprintField", "");
            checkpointField.text = PlayerPrefs.GetString("WorldCreator_playerCheckpointField", "");
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
        PlayerPrefs.SetString("WorldCreator_playerBlueprintField", blueprintField.text);
        PlayerPrefs.SetString("WorldCreator_playerCheckpointField", checkpointField.text);
        var path = Application.streamingAssetsPath + "\\Sectors\\TestWorld";
        var savePath = Application.persistentDataPath + "\\Saves\\TestSave";
        if (File.Exists(savePath))
            File.Delete(savePath);
        SaveMenuHandler.CreateSave("TestSave", checkpointField.text, blueprintField.text);
        SectorManager.testJsonPath = path;
        SaveMenuIcon.LoadSaveByPath(savePath, false);
    }


    public void ShowWriteMode()
    {
        Show(IOMode.Write);
    }

    public GameObject window;
    public GameObject newWorldStack;
    public InputField field;
    void Show(IOMode mode)
    {
        active = true;
        gameObject.SetActive(true);
        window.SetActive(true);
        newWorldStack.SetActive(mode == IOMode.Write || mode == IOMode.WriteShipJSON || mode == IOMode.WriteWaveJSON); 
        DestroyAllButtons();
        this.mode = mode;
        string[] directories = null;

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
                            generatorHandler.ReadWorld(dir);
                            break;
                        case IOMode.Write:
                            if(dir.Contains("main"))
                                generatorHandler.WriteWorld(System.IO.Path.GetDirectoryName(dir) + "\\main - " + VersionNumberScript.version);
                            else generatorHandler.WriteWorld(dir);
                            break;
                        case IOMode.ReadShipJSON:
                            builder.LoadBlueprint(System.IO.File.ReadAllText(dir));
                            break;
                        case IOMode.WriteShipJSON:
                            ShipBuilder.SaveBlueprint(null, dir, builder.GetCurrentJSON());
                            break;
                        case IOMode.ReadWaveJSON:
                            waveBuilder.ReadWaves(JsonUtility.FromJson<WaveSet>(System.IO.File.ReadAllText(dir)));
                            break;
                        case IOMode.WriteWaveJSON:
                            waveBuilder.ParseWaves(dir);
                            break;
                    }
                    Hide();
                }));
        }

    }

    void AddButton(string name, UnityAction action)
    {
        var button = Instantiate(buttonPrefab, content).GetComponent<Button>();
        button.onClick.AddListener(action);
        button.GetComponentInChildren<Text>().text = System.IO.Path.GetFileNameWithoutExtension(name);
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
                    generatorHandler.ReadWorld(path);
                    break;
                case IOMode.Write:
                    if(path.Contains("main"))
                        generatorHandler.WriteWorld(System.IO.Path.GetDirectoryName(path) + "\\main - " + VersionNumberScript.version);
                    else generatorHandler.WriteWorld(path);
                    break;
                case IOMode.ReadShipJSON:
                    builder.LoadBlueprint(System.IO.File.ReadAllText(path));
                    break;
                case IOMode.WriteShipJSON:
                    ShipBuilder.SaveBlueprint(null, path, builder.GetCurrentJSON());
                    break;
                case IOMode.ReadWaveJSON:
                    waveBuilder.ReadWaves(JsonUtility.FromJson<WaveSet>(System.IO.File.ReadAllText(path)));
                    break;
                case IOMode.WriteWaveJSON:
                    waveBuilder.ParseWaves(path);
                    break;
            }
            Hide();
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
