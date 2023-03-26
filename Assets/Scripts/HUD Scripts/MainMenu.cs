using System.Collections.Generic;
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
    [SerializeField]
    private InputField nameField;
    [SerializeField]
    private InputField blueprintField;
    [SerializeField]
    private InputField worldField;

    public void Start()
    {
       var args = GetCommandlineArgs();
        if (args.TryGetValue("-world", out string world))
        {
            PlayerPrefs.SetString("Network_worldName", world);

        }

        if (args.TryGetValue("-address", out string address))
        {
            PlayerPrefs.SetString("Network_address", address);

        }

        blueprintField.text = PlayerPrefs.GetString("Network_blueprintName", "Ad Slayer");
        worldField.text = PlayerPrefs.GetString("Network_worldName", "BattleZone Round Ringer");
        portField.text = PlayerPrefs.GetString("Network_port", "");
        addressField.text = PlayerPrefs.GetString("Network_address", "");
        nameField.text = PlayerPrefs.GetString("Network_name", "test_name");

       if (args.TryGetValue("-mode", out string mode))
        {
            switch (mode)
            {
                case "server":
                    NetworkDuel(MasterNetworkAdapter.NetworkMode.Server);
                    break;
            }
        }
    }

    private Dictionary<string, string> GetCommandlineArgs()
    {
        Dictionary<string, string> argDictionary = new Dictionary<string, string>();

        var args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; ++i)
        {
            var arg = args[i].ToLower();
            if (arg.StartsWith("-"))
            {
                var value = i < args.Length - 1 ? args[i + 1] : null;
                value = (value?.StartsWith("-") ?? false) ? null : value;

                argDictionary.Add(arg, value);
            }
        }
        return argDictionary;
    }

    public void NetworkDuel(string mode)
    {
        switch (mode)
        {
            case "server":
                NetworkDuel(MasterNetworkAdapter.NetworkMode.Server);
                break;
            case "client":
                NetworkDuel(MasterNetworkAdapter.NetworkMode.Client);
                break;
            case "host":
                if (Input.GetKey(KeyCode.LeftShift))
                    NetworkDuel(MasterNetworkAdapter.NetworkMode.Server);
                else
                    NetworkDuel(MasterNetworkAdapter.NetworkMode.Host);
                break;
        }
    }

    public void NetworkDuel(MasterNetworkAdapter.NetworkMode mode)
    {
        MasterNetworkAdapter.port = portField.text;
        MasterNetworkAdapter.address = addressField.text;
        MasterNetworkAdapter.blueprint = blueprintField.text;
        MasterNetworkAdapter.playerName = nameField.text;
        
        PlayerPrefs.SetString("Network_blueprintName", blueprintField.text);
        PlayerPrefs.SetString("Network_worldName", worldField.text);
        PlayerPrefs.SetString("Network_address", addressField.text);
        PlayerPrefs.SetString("Network_name", nameField.text);
        PlayerPrefs.SetString("Network_port", portField.text);
        Debug.Log("Duelling. Port: " + MasterNetworkAdapter.port + " Address: " + MasterNetworkAdapter.address + " Blueprint: " + MasterNetworkAdapter.blueprint + " Player name: " + MasterNetworkAdapter.playerName);
        if (mode != MasterNetworkAdapter.NetworkMode.Client)
        {
            var world = worldField.text;
            if (string.IsNullOrEmpty(world))
            {
                Debug.LogError("Invalid world name.");
                return;
            }
            var path = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", world);
            if (!System.IO.Directory.Exists(path)) 
            {
                Debug.LogError("World " + world + " does not exist.");
                return;
            }
            if (mode == MasterNetworkAdapter.NetworkMode.Host)
                MasterNetworkAdapter.StartHost();
            else MasterNetworkAdapter.StartServer();
            NetworkManager.Singleton.OnClientConnectedCallback += (u) => 
            { 
                MasterNetworkAdapter.instance.GetWorldNameClientRpc(world, u);
            };
            WCWorldIO.LoadTestSave(path, true);
        }
        else
        {
            MasterNetworkAdapter.StartClient();
            SystemLoader.AllLoaded = false;
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
