using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MasterNetworkAdapter : NetworkBehaviour
{
    public enum NetworkMode
    {
        Off,
        Client,
        Host
    }
    public static NetworkMode mode = NetworkMode.Off;
    public static MasterNetworkAdapter instance;
    public static string address;
    public static string port;
    public static string testName = "Test Name";

    public static bool lettingServerDecide;
    void Start()
    {
        instance = this;
    }

    public static void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        MasterNetworkAdapter.mode = MasterNetworkAdapter.NetworkMode.Client;
        ushort portVal = 0;
        if (!string.IsNullOrEmpty(port) && ushort.TryParse(port, out portVal))
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = portVal;
        }
        if (!string.IsNullOrEmpty(address))
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = address;
        }

        MasterNetworkAdapter.lettingServerDecide = true;
    }

    public static void StartServer()
    {
        PlayerCore.Instance.gameObject.SetActive(false);
    }

    public static void StartHost()
    {
        MasterNetworkAdapter.mode = MasterNetworkAdapter.NetworkMode.Host;
        NetworkManager.Singleton.StartHost();
    }

    [ClientRpc]
    public void GetWorldNameClientRpc(string worldName)
    {
        var path = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", worldName);
        if (!System.IO.Directory.Exists(path)) return;
        WCWorldIO.LoadTestSave(path, true);
    }


    public GameObject networkObj;

    [ServerRpc(RequireOwnership = false)]
    public void CreateNetworkObjectServerRpc(string name, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        var obj = Instantiate(networkObj).GetComponent<NetworkObject>();
        obj.GetComponent<EntityNetworkAdapter>().playerName = new NetworkVariable<FixedString64Bytes>(name);
        obj.SpawnWithOwnership(clientId);

        NetworkManager.Singleton.OnClientDisconnectCallback += (u) =>
        {
            if (u == clientId) obj.Despawn();
        };
    }
}
