using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Temporary main menu, will be redesigned later

public class MainMenu : MonoBehaviour
{
    public GameObject settings;
    public void StartSectorCreator() {
        SceneManager.LoadScene("SectorCreator");
    }
    public static void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OpenSettings()
    {
        if(settings) settings.GetComponentInChildren<GUIWindowScripts>().ToggleActive();
    }

    public void Quit() {
        Application.Quit();
    }
}
