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
        Host,
        Server
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
        if (!NetworkManager.Singleton) return;
        
        if (NetworkManager.Singleton.IsServer)
        {
            if (!NetworkManager.Singleton.IsClient)
                PlayerCore.Instance.gameObject.SetActive(false);
            MasterNetworkAdapter.mode = MasterNetworkAdapter.NetworkMode.Server;
        }
        if (NetworkManager.Singleton.IsClient)
        {
            MasterNetworkAdapter.mode = MasterNetworkAdapter.NetworkMode.Client;
        }
        if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsServer)
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
        NetworkManager.Singleton.StartServer();
    }

    public static void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    [ClientRpc]
    public void GetWorldNameClientRpc(string worldName, ulong clientID)
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.LocalClientId != clientID) return;
        var path = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", worldName);
        if (!System.IO.Directory.Exists(path)) return;
        WCWorldIO.LoadTestSave(path, true);
    }


    public GameObject networkObj;

    public void CreateNetworkObjectWrapper(string name, string blueprint, string idToGrab, bool isPlayer, int faction, Vector3 pos, ServerRpcParams serverRpcParams = default)
    {
        var obj = InternalEntitySpawnWrapper(blueprint, idToGrab, isPlayer, faction, pos, serverRpcParams);
        if (isPlayer) obj.GetComponent<EntityNetworkAdapter>().playerName = name;
    }

    public Dictionary<ulong, bool> playerSpawned = new Dictionary<ulong, bool>();

    [ServerRpc(RequireOwnership = false)]
    public void CreatePlayerServerRpc(string name, string blueprint, int faction, ServerRpcParams serverRpcParams = default)
    {
        if (!playerSpawned.ContainsKey(serverRpcParams.Receive.SenderClientId))
            playerSpawned.Add(serverRpcParams.Receive.SenderClientId, false);
        if (playerSpawned[serverRpcParams.Receive.SenderClientId]) return;
        CreateNetworkObjectWrapper(name, blueprint, "player-"+serverRpcParams.Receive.SenderClientId, true, faction, Vector3.zero, serverRpcParams);
        playerSpawned[serverRpcParams.Receive.SenderClientId] = true;
    }

    private NetworkObject InternalEntitySpawnWrapper(string blueprint, string idToGrab, bool isPlayer, int faction, Vector3 pos, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        var obj = Instantiate(networkObj).GetComponent<NetworkObject>();
        obj.SpawnWithOwnership(clientId);
        obj.GetComponent<EntityNetworkAdapter>().blueprintString = blueprint;
        obj.GetComponent<EntityNetworkAdapter>().passedFaction = faction;
        obj.GetComponent<EntityNetworkAdapter>().isPlayer.Value = isPlayer;
        obj.GetComponent<EntityNetworkAdapter>().idToUse = idToGrab;
        if (pos != Vector3.zero)
            obj.GetComponent<EntityNetworkAdapter>().ChangePositionWrapper(pos);
        NetworkManager.Singleton.OnClientDisconnectCallback += (u) =>
        {
            if (u == clientId) obj.Despawn();
        };
        
        return obj;
    }
}
