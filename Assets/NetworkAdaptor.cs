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
    }

    public GameObject networkObj;


    [ServerRpc(RequireOwnership = false)]
    public void CreateNetworkObjectServerRpc(ServerRpcParams serverRpcParams = default)
    {
        //networkObj.GetComponent<NetworkObject>().Spawn();
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            var client = NetworkManager.Singleton.ConnectedClients[clientId];
            // Do things for this client
        }
    }
}
