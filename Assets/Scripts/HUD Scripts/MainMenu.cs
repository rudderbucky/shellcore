using UnityEngine;
using UnityEngine.SceneManagement;

//Temporary main menu, will be redesigned later
public class MainMenu : MonoBehaviour
{
    public GameObject settings;
    public GameObject discordPopup;

    public void StartSectorCreator()
    {
        SceneManager.LoadScene("SectorCreator");
    }

    public static void StartGame(bool nullifyTestJsonPath = false)
    {
        if (nullifyTestJsonPath)
        {
            SectorManager.testJsonPath = null;
            SectorManager.testResourcePath = null;
        }

        SceneManager.LoadScene("SampleScene");
    }

    public void OpenSettings()
    {
        if (settings)
        {
            settings.GetComponentInChildren<GUIWindowScripts>().ToggleActive();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OpenCredits()
    {
        DialogueSystem.ShowPopup(
@"Programming by Ormanus and rudderbucky
Art by rudderbucky
Music by Avocato, FlightWish and Mr Spastic
Story by Flashbacker and rudderbucky
Skirmish Minisode by Vansten
Playtesting by YOU!
Special thanks to Flashbacker"
        );
    }

    public void OpenCommunityPopup()
    {
        if (discordPopup)
        {
            discordPopup.GetComponentInChildren<GUIWindowScripts>().ToggleActive();
        }
    }

    public void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/TXaenta");
    }

    public void StartWorldCreator()
    {
        WCGeneratorHandler.DeleteTestWorld();
        SceneManager.LoadScene("WorldCreator");
    }
}
