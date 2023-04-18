using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
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

    public static string GATEWAY_IP = "34.125.253.226:8000";
    private bool queueNetworkRun = false;

    public void RunClientFromGateway(string result)
    {
        Debug.Log("Connecting to: " + result);
        var addressArray = result.Split(":");
        SetAddress(addressArray[0]);
        SetPort(addressArray[1]);
        queueNetworkRun = true;
    }

    public static System.Net.Http.HttpClient client;

    public static string location = null;
    public static string RDB_SERVER_PASSWORD = "test_password";
    [SerializeField]
    private Dropdown rdbServerLocation;
    [SerializeField]
    private Text playersConnectedText;
    private string playersConnected = "";

    public IEnumerator UpdatePlayersConnected()
    {
        if (!playersConnectedText) yield return null;
        UnityWebRequest www = UnityWebRequest.Get($"http://{GATEWAY_IP}/playercount/{location}");
        www.timeout = 3;
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            playersConnected = www.downloadHandler.text;
        }
    }
    public void QueryGateway()
    {
        StartCoroutine(QueryGatewayHelper());
    }
    public IEnumerator QueryGatewayHelper()
    {
        UnityWebRequest www = UnityWebRequest.Get($"http://{GATEWAY_IP}/seekip/{location}");
        www.timeout = 3;
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            RunClientFromGateway(www.downloadHandler.text);
        }
    }


    public void UpdateLocation(int loc)
    {
        if (playersConnectedText)
        {
            playersConnected = "";
        }
        switch(loc)
        {
            case 0:
                location = "na";
                break;
            case 1:
                location = "eu";
                break;
            case 2:
                location = "apac";
                break;
            default:
                break;
        }

        try
        {
            StartCoroutine(UpdatePlayersConnected());
        }
        catch
        {
            Debug.Log("Connection failure."  );
        }
    }


    public void Start()
    {
        location = "";
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
            RDB_SERVER_PASSWORD = pw;
        }

        if (args.TryGetValue("-port", out string pt))
        {
            PlayerPrefs.SetString("Network_port", pt);
        }

        if (client == null)
        {
            client = new System.Net.Http.HttpClient();
            client.Timeout = new System.TimeSpan(0,0,3);
        }

        if (string.IsNullOrEmpty(location))
            UpdateLocation(0);

        blueprintFields.ForEach(x => x.text = PlayerPrefs.GetString("Network_blueprintName", "Advanced Scout"));
        worldField.text = PlayerPrefs.GetString("Network_worldName", "BattleZone Round Ringer");
        if (rdbServerLocation && string.IsNullOrEmpty(RDB_SERVER_PASSWORD))
        {
            try
            {
                rdbServerLocation.value = int.Parse(PlayerPrefs.GetString("Network_location", "0"));
            }
            catch {}
            UpdateLocation(rdbServerLocation.value);
        }

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
                    NetworkDuel("server");
                else
                    NetworkDuel(MasterNetworkAdapter.NetworkMode.Host);
                break;
        }
    }

    private void Update()
    {
        if (queueNetworkRun)
        {
            MasterNetworkAdapter.world = VersionNumberScript.rdbMap;
            NetworkDuel("client");
            queueNetworkRun = false;
        }

        if (playersConnectedText)
        {
            if (string.IsNullOrEmpty(playersConnected)) 
                playersConnectedText.text = $"Checking how many players are online...";
            else
            {
                var serverAndPlayerCounts = playersConnected.Split(":");
                var sCount = $"There {(serverAndPlayerCounts[0] == "1" ? "is" : "are")} {serverAndPlayerCounts[0]} server{(serverAndPlayerCounts[0] == "1" ? "" : "s")} running in this location.";
                var pCount = $"There {(serverAndPlayerCounts[1] == "1" ? "is" : "are")} {serverAndPlayerCounts[1]} player{(serverAndPlayerCounts[1] == "1" ? "" : "s")} connected to this location.";
                playersConnectedText.text = $"{sCount}\n{pCount}";
            }
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
        
        PlayerPrefs.SetString("Network_address", addressField.text);

        PlayerPrefs.SetString("Network_port", portField.text);

        PlayerPrefs.SetString("Network_location", rdbServerLocation.value.ToString());

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
