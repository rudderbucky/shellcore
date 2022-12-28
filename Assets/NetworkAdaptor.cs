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
        DevConsoleScript.networkEnabled = true;
        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.OnClientConnectedCallback += (u) => {
        NetworkAdaptor.instance.CreateNetworkObjectServerRpc();};
#else
        DevConsoleScript.networkEnabled = true;
        NetworkManager.Singleton.StartServer();
#endif
    }

    public GameObject networkObj;

    [ServerRpc(RequireOwnership = false)]
    public void CreateNetworkObjectServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        Instantiate(networkObj).GetComponent<NetworkObject>().Spawn();
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            var client = NetworkManager.Singleton.ConnectedClients[clientId];
            // Do things for this client
        }
    }
}
