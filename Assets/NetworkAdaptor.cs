using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkAdaptor : NetworkBehaviour
{
    public enum NetworkMode
    {
        Off,
        Client,
        Host
    }

    public static NetworkMode mode = NetworkMode.Off;
    public static NetworkAdaptor instance;
    public static string address;
    public static string port;

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
        DevConsoleScript.networkEnabled = true;
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
        NetworkAdaptor.instance.CreateNetworkObjectServerRpc();};
    }

    public static void StartServer()
    {
        PlayerCore.Instance.gameObject.SetActive(false);
        DevConsoleScript.networkEnabled = true;
        NetworkManager.Singleton.StartServer();
    }

    public static void StartHost()
    {
        DevConsoleScript.networkEnabled = true;
        NetworkManager.Singleton.StartHost();
        NetworkAdaptor.instance.CreateNetworkObjectServerRpc();
    }


    public GameObject networkObj;

    [ServerRpc(RequireOwnership = false)]
    public void CreateNetworkObjectServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        var obj = Instantiate(networkObj).GetComponent<NetworkObject>();
        obj.SpawnWithOwnership(clientId);
        NetworkManager.Singleton.OnClientDisconnectCallback += (u) =>
        {
            if (u == clientId) obj.Despawn();
        };
    }
}
