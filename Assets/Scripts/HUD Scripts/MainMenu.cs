using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject settings;

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

    [SerializeField]
    private InputField addressField;
    
    [SerializeField]
    private InputField portField;

    public void NetworkDuel(bool hostMode)
    {
        if (hostMode)
        {
            var world = "test";
            var path = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", world);
            if (!System.IO.Directory.Exists(path)) return;
            MasterNetworkAdapter.StartHost();
            NetworkManager.Singleton.OnClientConnectedCallback += (u) => 
            { 
                MasterNetworkAdapter.instance.GetWorldNameClientRpc(world);
                WCWorldIO.LoadTestSave(path, true);
                NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
                MasterNetworkAdapter.instance.CreateNetworkObjectServerRpc("Test Name");
            };
        }
        else
        {
            MasterNetworkAdapter.StartClient();
        }
    }

    public void OpenSettings()
    {
        if (settings)
        {
            settings.GetComponentInChildren<GUIWindowScripts>().ToggleActive(true);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/TXaenta");
    }

    public void OpenTwitter()
    {
        Application.OpenURL("https://twitter.com/rudderbucky");
    }

    public void StartWorldCreator()
    {
        WCGeneratorHandler.DeleteTestWorld();
        SceneManager.LoadScene("WorldCreator");
    }
}
