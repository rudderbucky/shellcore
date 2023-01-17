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
    public static string playerName = "Test Name";
    public static string blueprint;

    public static bool lettingServerDecide;
    void Start()
    {
        instance = this;
        if (NetworkManager.Singleton.IsClient)
        {
            MasterNetworkAdapter.mode = MasterNetworkAdapter.NetworkMode.Client;
        }
        if (NetworkManager.Singleton.IsServer)
        {
            MasterNetworkAdapter.mode = MasterNetworkAdapter.NetworkMode.Host;
        }
    }

    public static void StartClient()
    {
        NetworkManager.Singleton.StartClient();
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
        NetworkManager.Singleton.StartHost();
    }

    [ClientRpc]
    public void GetWorldNameClientRpc(string worldName)
    {
        if (NetworkManager.Singleton.IsHost) return;
        Debug.LogWarning("loading world");
        var path = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", worldName);
        if (!System.IO.Directory.Exists(path)) return;
        WCWorldIO.LoadTestSave(path, true);
    }


    public GameObject networkObj;

    [ServerRpc(RequireOwnership = false)]
    public void CreateNetworkObjectServerRpc(string name, string blueprint, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        Debug.LogWarning(name);
        var obj = InternalEntitySpawnWrapper(blueprint, serverRpcParams);
        obj.GetComponent<EntityNetworkAdapter>().playerName = name;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnEntityServerRpc(string blueprint, ServerRpcParams serverRpcParams = default)
    {
        InternalEntitySpawnWrapper(blueprint, serverRpcParams);
    }

    private NetworkObject InternalEntitySpawnWrapper(string blueprint, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        var obj = Instantiate(networkObj).GetComponent<NetworkObject>();
        obj.SpawnWithOwnership(clientId);
        obj.GetComponent<EntityNetworkAdapter>().blueprintString = blueprint;

        NetworkManager.Singleton.OnClientDisconnectCallback += (u) =>
        {
            if (u == clientId) obj.Despawn();
        };
        
        return obj;
    }
}
