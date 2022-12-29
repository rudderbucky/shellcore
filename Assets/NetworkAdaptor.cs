using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
    void Start()
    {
        instance = this;
        switch (mode)
        {
            case NetworkMode.Client:
                StartClient();
                break;
            case NetworkMode.Host:
                StartHost();
                break;
        }
    }

    public static void StartClient()
    {
        DevConsoleScript.networkEnabled = true;
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
