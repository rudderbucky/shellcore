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
    public GameObject buttonPrefab;
    public Transform content;
    public InputField blueprintField;
    public InputField checkpointField;
    

    enum IOMode
    {
        Read,
        Write,
        ReadShipJSON,
        WriteShipJSON
    }

    IOMode mode = IOMode.Read;

    public void Quit()
    {
        SceneManager.LoadScene("MainMenu");
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

    void Start()
    {
        if(SceneManager.GetActiveScene().name == "WorldCreator")
        {
            blueprintField.text = PlayerPrefs.GetString("WorldCreator_playerBlueprintField", "");
            checkpointField.text = PlayerPrefs.GetString("WorldCreator_playerCheckpointField", "");
            var path = Application.streamingAssetsPath + "\\Sectors\\TestWorld";
            if(Directory.Exists(path)) 
                generatorHandler.ReadWorld(path);
        }
    }

    public void TestWorld()
    {
        var savePath = Application.persistentDataPath + "\\Saves\\TestSave";
        if(File.Exists(savePath)) 
            File.Delete(savePath);
        PlayerPrefs.SetString("WorldCreator_playerBlueprintField", blueprintField.text);
        PlayerPrefs.SetString("WorldCreator_playerCheckpointField", checkpointField.text);
        SaveMenuHandler.CreateSave("TestSave", checkpointField.text, blueprintField.text);
        var path = Application.streamingAssetsPath + "\\Sectors\\TestWorld";
        if(generatorHandler.WriteWorld(path))
        {
            SectorManager.testJsonPath = path;
            SaveMenuIcon.LoadSaveByPath(savePath, false);
        }
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
        gameObject.SetActive(true);
        window.SetActive(true);
        newWorldStack.SetActive(mode == IOMode.Write || mode == IOMode.WriteShipJSON); 
        DestroyAllButtons();
        this.mode = mode;
        var directories = mode == IOMode.Read || mode == IOMode.Write ? Directory.GetDirectories(Application.streamingAssetsPath + "\\Sectors")
            : Directory.GetFiles(Application.streamingAssetsPath + "\\Entities");
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
                            generatorHandler.WriteWorld(dir);
                            break;
                        case IOMode.ReadShipJSON:
                            builder.LoadBlueprint(System.IO.File.ReadAllText(dir));
                            break;
                        case IOMode.WriteShipJSON:
                            ShipBuilder.SaveBlueprint(null, dir, builder.GetCurrentJSON());
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
        var path = mode == IOMode.Read || mode == IOMode.Write ? Application.streamingAssetsPath + "\\Sectors\\" + field.text
            : Application.streamingAssetsPath + "\\Entities\\" + field.text;
        if(!Directory.Exists(path) && (mode == IOMode.Read || mode == IOMode.Write)) Directory.CreateDirectory(path);
        AddButton(path, new UnityEngine.Events.UnityAction(() => {
            switch(mode)
            {
                case IOMode.Read:
                    generatorHandler.ReadWorld(path);
                    break;
                case IOMode.Write:
                    generatorHandler.WriteWorld(path);
                    break;
                case IOMode.ReadShipJSON:
                    builder.LoadBlueprint(System.IO.File.ReadAllText(path));
                    break;
                case IOMode.WriteShipJSON:
                    ShipBuilder.SaveBlueprint(null, path, builder.GetCurrentJSON());
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
        DestroyAllButtons();
        gameObject.SetActive(false);
        window.SetActive(false);
    }
}
