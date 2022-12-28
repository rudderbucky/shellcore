using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkAdaptor : NetworkBehaviour
{

    public static NetworkAdaptor instance;
    void Start()
    {
        instance = this;
#if UNITY_EDITOR
    StartClient();
#else
    //StartServer();
#endif
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


    public GameObject networkObj;

    [ServerRpc(RequireOwnership = false)]
    public void CreateNetworkObjectServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        var obj = Instantiate(networkObj).GetComponent<NetworkObject>();
        obj.Spawn();
        NetworkManager.Singleton.OnClientDisconnectCallback += (u) =>
        {
            if (u == clientId) obj.Despawn();
        };
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            var client = NetworkManager.Singleton.ConnectedClients[clientId];
            // Do things for this client
        }
    }
}
