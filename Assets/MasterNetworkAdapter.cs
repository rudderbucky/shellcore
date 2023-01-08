using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

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
    public static string testName = "Test";

    public static bool lettingServerDecide;
    void Start()
    {
        instance = this;
        switch (mode)
        {
            case NetworkMode.Client:
                StartClient();
                lettingServerDecide = true;
                break;
            case NetworkMode.Host:
                StartHost();
                break;
        }
    }

    public static void StartClient()
    {
        ushort portVal = 0;
        if (!string.IsNullOrEmpty(port) && ushort.TryParse(port, out portVal))
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = portVal;
        }
        if (!string.IsNullOrEmpty(address))
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = address;
        }


        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.OnClientConnectedCallback += (u) => {
        MasterNetworkAdapter.instance.CreateNetworkObjectServerRpc(testName);};
    }

    public static void StartServer()
    {
        PlayerCore.Instance.gameObject.SetActive(false);
        NetworkManager.Singleton.StartServer();
    }

    public static void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        MasterNetworkAdapter.instance.CreateNetworkObjectServerRpc(testName);
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
