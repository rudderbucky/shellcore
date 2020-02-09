using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WCWorldIO : MonoBehaviour
{

    public WCGeneratorHandler generatorHandler;
    public GameObject buttonPrefab;
    public Transform content;

    enum IOMode
    {
        Read,
        Write
    }

    IOMode mode = IOMode.Read;

    public void ShowReadMode()
    {
        Show(IOMode.Read);
    }

    public void TestWorld()
    {
        var savePath = Application.persistentDataPath + "\\Saves\\TestSave";
        if(File.Exists(savePath)) 
            File.Delete(savePath);
        SaveMenuHandler.CreateSave("TestSave");
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

    public GameObject newWorldStack;
    public InputField field;
    void Show(IOMode mode)
    {
        gameObject.SetActive(true);
        newWorldStack.SetActive(mode == IOMode.Write); 
       DestroyAllButtons();
        this.mode = mode;
        foreach(var dir in Directory.GetDirectories(Application.streamingAssetsPath + "\\Sectors"))
        {
            if(!dir.Contains("TestWorld"))
                AddButton(dir);
        }

    }

    void AddButton(string name)
    {
        var button = Instantiate(buttonPrefab, content).GetComponent<Button>();
        button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
            if(mode == IOMode.Write)
                generatorHandler.WriteWorld(name);
            else generatorHandler.ReadWorld(name);
            Hide();
        }));
        var extraLength = (Application.streamingAssetsPath + "\\Sectors\\").Length;
        button.GetComponentInChildren<Text>().text = name.Substring(extraLength);
    }
    
    public void AddButtonFromField()
    {
        var path = Application.streamingAssetsPath + "\\Sectors\\" + field.text;
        if(!Directory.Exists(path)) Directory.CreateDirectory(path);
        AddButton(path);
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
    }
}
