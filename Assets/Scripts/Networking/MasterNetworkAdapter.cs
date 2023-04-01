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
    public static string world;
    public static int POP_IN_DISTANCE = 500;

    public static bool lettingServerDecide;
    void Start()
    {
        Debug.Log("MNA starting...");
        if (EntityNetworkAdapter.playerFactions != null)
            EntityNetworkAdapter.playerFactions.Clear();
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

    public override void OnNetworkDespawn()
    {
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client)
        {
            MasterNetworkAdapter.mode = NetworkMode.Off;
            MasterNetworkAdapter.lettingServerDecide = false;
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MainMenu");
            SceneManager.sceneLoaded += UnloadMessage;
        }
    }

    private void UnloadMessage(Scene s1, LoadSceneMode s2)
    {
        Debug.LogWarning("Server disconnected.");
        DevConsoleScript.Instance.SetActive();
        SceneManager.sceneLoaded -= UnloadMessage;
        return;
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
        MasterNetworkAdapter.lettingServerDecide = true;
    }

    public static void StartServer()
    {
        Debug.Log("Starting server...");
        MasterNetworkAdapter.lettingServerDecide = false;
        NetworkManager.Singleton.StartServer();
    }

    public static void StartHost()
    {

        MasterNetworkAdapter.lettingServerDecide = false;
        NetworkManager.Singleton.StartHost();
    }

    [ClientRpc]
    public void GetWorldNameClientRpc(string worldName, int currentSector, ulong clientID)
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.LocalClientId != clientID) return;
        var path = System.IO.Path.Combine(Application.streamingAssetsPath, "Sectors", worldName);
        if (!System.IO.Directory.Exists(path)) return;
        SectorManager.currentSectorIndex = currentSector;
        WCWorldIO.LoadTestSave(path, true);
        SectorManager.instance.ReloadSector(SectorManager.currentSectorIndex);
    }


    [ClientRpc]
    public void ReloadSectorClientRpc(int sectorToChange, ClientRpcParams clientRpcParams = default)
    {
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Host) return;
        SectorManager.instance.ReloadSector(sectorToChange);
    }

    [ClientRpc]
    public void NotifyInvalidBlueprintClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (clientRpcParams.Send.TargetClientIds != null &&
            !System.Linq.Enumerable.Contains<ulong>(clientRpcParams.Send.TargetClientIds, NetworkManager.Singleton.LocalClientId))
        {
            return;
        }
        Debug.LogWarning("Your passed blueprint is invalid. Use command loadbp <blueprint JSON> to pass another one and join the game.");
        DevConsoleScript.Instance.SetActive();
    }


    public GameObject networkObj;

    public void CreateNetworkObjectWrapper(string name, string blueprint, string idToGrab, bool isPlayer, int faction, Vector3 pos, ServerRpcParams serverRpcParams = default)
    {
        var obj = InternalEntitySpawnWrapper(blueprint, idToGrab, isPlayer, faction, pos, serverRpcParams);
        var networkAdapter = obj.GetComponent<EntityNetworkAdapter>();
        networkAdapter.blueprint = SectorManager.TryGettingEntityBlueprint(blueprint);
        if (isPlayer) networkAdapter.playerName = name;
        else
        {
            networkAdapter.passedFaction = faction;
            networkAdapter.SetUpHuskEntity();
        }
    }

    public Dictionary<ulong, bool> playerSpawned = new Dictionary<ulong, bool>();



    [ServerRpc(RequireOwnership = false)]
    public void RequestVoteServerRpc(int i, ServerRpcParams serverRpcParams = default)
    {
        if (!DialogueSystem.Instance.IsVoting()) return;
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        int voteToAdd = -1;
        int voteToSub = -1;
        if (!DialogueSystem.Instance.votesById.ContainsKey(clientId))
        {
            DialogueSystem.Instance.votesById.Add(clientId, i);
        }
        else
        {
            voteToSub = DialogueSystem.Instance.votesById[clientId];
            DialogueSystem.Instance.voteNumbers[voteToSub]--;
            DialogueSystem.Instance.votesById[clientId] = i;
        }
        voteToAdd = i;
        DialogueSystem.Instance.voteNumbers[i]++;
        RequestRefreshVoteClientRpc(voteToAdd, voteToSub);
    }

    [ClientRpc]
    public void RequestRefreshVoteClientRpc(int newVoteToAdd, int newVoteToSub, ClientRpcParams clientRpcParams = default)
    {
        if (MasterNetworkAdapter.mode == NetworkMode.Host)
        {
            DialogueSystem.Instance.RefreshButtons();
            return;
        }
        if (newVoteToAdd >= 0 && DialogueSystem.Instance.voteNumbers.Count > newVoteToAdd)
            DialogueSystem.Instance.voteNumbers[newVoteToAdd]++;
        if (newVoteToSub >= 0 && DialogueSystem.Instance.voteNumbers.Count > newVoteToSub)
            DialogueSystem.Instance.voteNumbers[newVoteToSub]--;
        DialogueSystem.Instance.RefreshButtons();
    }


    [ServerRpc(RequireOwnership = false)]
    public void CreatePlayerServerRpc(string name, string blueprint, int faction, ServerRpcParams serverRpcParams = default)
    {
        if (!playerSpawned.ContainsKey(serverRpcParams.Receive.SenderClientId))
            playerSpawned.Add(serverRpcParams.Receive.SenderClientId, false);
        if (playerSpawned[serverRpcParams.Receive.SenderClientId]) return;
        if (!ValidatePlayerBlueprint(blueprint))
        {
            NotifyInvalidBlueprintClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] {serverRpcParams.Receive.SenderClientId}
                }
            });
            return;
        }
        CreateNetworkObjectWrapper(name, blueprint, "player-"+serverRpcParams.Receive.SenderClientId, true, faction, Vector3.zero, serverRpcParams);
        playerSpawned[serverRpcParams.Receive.SenderClientId] = true;
    }

    private bool ValidatePlayerBlueprint(string blueprint)
    {
        if (blueprint.Length > 25000) // Blueprint too large. We can't have the server do too much work here or else it will chug everyone.
        {
            return false;
        }
        var print = ScriptableObject.CreateInstance<EntityBlueprint>();
        try
        {
            print = SectorManager.TryGettingEntityBlueprint(blueprint);
        }
        catch // invalid blueprint
        {
            return false;
        }
        if (print.intendedType != EntityBlueprint.IntendedType.ShellCore) return false; // print is of incorrect type

        var invalidAbilities = new List<AbilityID>() {AbilityID.MainBullet, AbilityID.Harvester, AbilityID.EnergyAura, AbilityID.SpeedAura, AbilityID.HealAura, AbilityID.Rocket, AbilityID.SpeederBullet, AbilityID.SiegeBullet};
        foreach (var part in print.parts)
        {
            if (invalidAbilities.Contains((AbilityID)part.abilityID)) return false;
        }
        return true;
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
