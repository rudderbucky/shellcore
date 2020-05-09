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
    public static void StartGame(bool nullifyTestJsonPath = false)
    {
        if(nullifyTestJsonPath) SectorManager.testJsonPath = null;
        SceneManager.LoadScene("SampleScene");
        Debug.LogWarning(Time.timeSinceLevelLoad);
    }

    public void OpenSettings()
    {
        if(settings) settings.GetComponentInChildren<GUIWindowScripts>().ToggleActive();
    }

    public void Quit() {
        Application.Quit();
    }

    public void OpenCredits()
    {
        DialogueSystem.ShowPopup(
            "Programming by Ormanus and rudderbucky\n"
            + "Art by rudderbucky\n"
            + "Music by Avocato, FlightWish and Mr Spastic\n"
            + "Story by Flashbacker and rudderbucky\n"
            + "Playtesting by YOU!\n"
            + "Special thanks to Flashbacker"
        );
    }

    public void StartWorldCreator()
    {
        WCGeneratorHandler.DeleteTestWorld();
        SceneManager.LoadScene("WorldCreator");
    }
}
