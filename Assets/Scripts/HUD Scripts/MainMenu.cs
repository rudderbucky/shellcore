using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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
    private List<InputField> nameFields;
    [SerializeField]
    private List<InputField> blueprintFields;
    [SerializeField]
    private InputField worldField;

    private static string GATEWAY_IP = "34.125.253.226:8000";
    private bool queueNetworkRun = false;

    public void RunClientFromGateway(Task<HttpResponseMessage> message)
    {
        message.Result.Content.ReadAsStringAsync().ContinueWith((s) => 
        {
            Debug.Log("Connecting to: " + s.Result);
            var addressArray = s.Result.Split(":");
            Debug.LogWarning(addressArray[0] + " " + addressArray[1]);
            SetAddress(addressArray[0]);
            SetPort(addressArray[1]);
            queueNetworkRun = true;
        });
    }

    public void QueryGateway()
    {
        var client = new System.Net.Http.HttpClient();
        client.Timeout = new System.TimeSpan(0,0,5);
        var retval = client.GetAsync($"http://{GATEWAY_IP}/api").ContinueWith((request) => RunClientFromGateway(request));
    }

    private static string location;
    private static string password;

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

        if (args.TryGetValue("-location", out string loc))
        {
            location = loc;
        }

        if (args.TryGetValue("-password", out string pw))
        {
            password = pw;
        }

        blueprintFields.ForEach(x => x.text = PlayerPrefs.GetString("Network_blueprintName", "Ad Slayer"));
        worldField.text = PlayerPrefs.GetString("Network_worldName", "BattleZone Round Ringer");
        portField.text = PlayerPrefs.GetString("Network_port", "");
        addressField.text = PlayerPrefs.GetString("Network_address", "");
        nameFields.ForEach(x => x.text = PlayerPrefs.GetString("Network_name", "test_name"));
        MasterNetworkAdapter.address = addressField.text;
        MasterNetworkAdapter.port = portField.text;
        MasterNetworkAdapter.blueprint = blueprintFields[0].text;
        MasterNetworkAdapter.playerName = nameFields[0].text;

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

    private void Update()
    {
        if (queueNetworkRun)
        {
            MasterNetworkAdapter.world = "rudderbucky server";
            NetworkDuel("client");
            queueNetworkRun = false;
        }
    }

    public void StartSkirmishHelper(MasterNetworkAdapter.NetworkMode mode)
    {
        Debug.Log("Duelling. Port: " + MasterNetworkAdapter.port + " Address: " + MasterNetworkAdapter.address + " Blueprint: " + MasterNetworkAdapter.blueprint + " Player name: " + MasterNetworkAdapter.playerName);
        SectorManager.currentSectorIndex = 0;
        if (mode != MasterNetworkAdapter.NetworkMode.Client)
        {
            var world = MasterNetworkAdapter.world;
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
                MasterNetworkAdapter.instance.GetWorldNameClientRpc(world, SectorManager.currentSectorIndex, u);
            };
            WCWorldIO.LoadTestSave(path, true);
        }
        else
        {
            MasterNetworkAdapter.StartClient();
            SystemLoader.AllLoaded = false;
        }
    }

    private IEnumerator coroutine;
    public void SetAddress(string address)
    {
        MasterNetworkAdapter.address = address;
    }

    public void SetPort(string port)
    {
        MasterNetworkAdapter.port = port;
    }

    public void SetBlueprint(string blueprint)
    {
        MasterNetworkAdapter.blueprint = blueprint;
        if (!string.IsNullOrEmpty(blueprint))
            PlayerPrefs.SetString("Network_blueprintName", blueprint);
    }

    public void SetName(string name)
    {
        MasterNetworkAdapter.playerName = name;
        if (!string.IsNullOrEmpty(name))
            PlayerPrefs.SetString("Network_name", name);
    }

    public void SetWorld(string world)
    {
        MasterNetworkAdapter.world = world;
    }

    public void NetworkDuel(MasterNetworkAdapter.NetworkMode mode)
    {

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        if (!string.IsNullOrEmpty(worldField.text))
        {
            PlayerPrefs.SetString("Network_worldName", worldField.text);
        }
        
        if (!string.IsNullOrEmpty(addressField.text))
            PlayerPrefs.SetString("Network_address", addressField.text);

        if (!string.IsNullOrEmpty(portField.text))
            PlayerPrefs.SetString("Network_port", portField.text);

        if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }

        StartSkirmishHelper(mode);
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
