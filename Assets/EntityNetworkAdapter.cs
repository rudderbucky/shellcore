using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class EntityNetworkAdapter : NetworkBehaviour
{
    public struct ServerResponse : INetworkSerializable, IEquatable<ServerResponse>
    {
        public Vector3 position;
        public Vector3 velocity;
        public Quaternion rotation;
        public float time;
        public int faction;
        public float weaponGCDTimer;
        public float shell;
        public float core;
        public float energy;
        public ulong clientID;
        public ServerResponse(Vector3 position, Vector3 velocity, Quaternion rotation, ulong clientID, int faction, float weaponGCDTimer, float shell, float core, float energy)
        {
            this.position = position;
            this.velocity = velocity;
            this.clientID = clientID;
            this.time = Time.time;
            this.rotation = rotation;
            this.faction = faction;
            this.weaponGCDTimer = weaponGCDTimer;
            this.shell = shell;
            this.core = core;
            this.energy = energy;
        }

        public bool Equals(ServerResponse other)
        {
            return clientID == other.clientID && this.time == other.time;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref time);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref clientID);
            serializer.SerializeValue(ref faction);
            serializer.SerializeValue(ref weaponGCDTimer);
            serializer.SerializeValue(ref shell);
            serializer.SerializeValue(ref core);
            serializer.SerializeValue(ref energy);
        }
    }

    public string blueprintString;
    private EntityBlueprint blueprint;

    public struct PartStatusResponse : INetworkSerializable, IEquatable<PartStatusResponse>
    {
        public Vector2 location;
        public bool detached;

        public PartStatusResponse(Vector2 location, bool val)
        {
            this.location = location;
            detached = val;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref location);
            serializer.SerializeValue(ref detached);
        }

        public bool Equals(PartStatusResponse other)
        {
            return location == other.location;
        }
    }

    public NetworkList<PartStatusResponse> partStatuses;

    public struct ClientMessage : INetworkSerializable
    {
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            throw new System.NotImplementedException();
        }
    }


    public class TemporaryStateWrapper
    {
        public Vector3 position;
        public Vector3 directionalVector;
        public ulong clientID;

        public ServerResponse CreateResponse(EntityNetworkAdapter buf)
        {
            Rigidbody2D body = null;
            Entity core = null;
            body = buf.huskEntity.GetComponent<Rigidbody2D>();
            core = buf.huskEntity;
            return new ServerResponse(core.transform.position, body.velocity, core.transform.rotation, clientID, core.faction, core.GetWeaponGCDTimer(), core.CurrentHealth[0], core.CurrentHealth[1], core.CurrentHealth[2]);
        }
    }

    public TemporaryStateWrapper wrapper;

    public NetworkVariable<ServerResponse> state = new NetworkVariable<ServerResponse>();
    public NetworkVariable<bool> isPlayer = new NetworkVariable<bool>(false);

    [SerializeField]
    private Entity huskEntity;
    public int passedFaction = 0;

    void Awake()
    {
        if (partStatuses == null) partStatuses = new NetworkList<PartStatusResponse>();
        serverReady = new NetworkVariable<bool>(false);
    }
    void Start()
    {        
        if (NetworkManager.Singleton.IsServer)
        {
            if (passedFaction == 0) passedFaction = NetworkManager.Singleton.ConnectedClients == null ? 0 : NetworkManager.Singleton.ConnectedClients.Count - 1;
            if (IsOwner && isPlayer.Value) passedFaction = 0;
            state.Value = new ServerResponse(Vector3.zero, Vector3.zero, Quaternion.identity, OwnerClientId, passedFaction, 0, 1000, 250, 500);
            blueprint = SectorManager.TryGettingEntityBlueprint(blueprintString);
        }
        if (NetworkManager.Singleton.IsClient)
        {
            state.OnValueChanged += (x, y) =>
            {
                if (y.clientID == NetworkManager.Singleton.LocalClientId && isPlayer.Value)
                {
                    UpdatePlayerState(y);
                }
                else if (huskEntity)
                {
                    UpdateCoreState(huskEntity, y);
                }
            };
        }
    }

    public override void OnNetworkSpawn()
    {
        if (wrapper == null)
        {
            wrapper = new TemporaryStateWrapper();
            wrapper.clientID = OwnerClientId;
        }

        if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClientId == OwnerClientId && isPlayer.Value)
        {
            PlayerCore.Instance.networkAdapter = this;
        }

        

    }

    public override void OnNetworkDespawn()
    {
        if (huskEntity)
        {
            Destroy(huskEntity.gameObject);
        }
    }

    private void UpdatePlayerState(ServerResponse response)
    {
        UpdateCoreState(PlayerCore.Instance, response);
    }

    private void UpdateCoreState(Entity core, ServerResponse response)
    {
        core.transform.position = response.position;
        core.GetComponent<Rigidbody2D>().velocity = response.velocity;
        core.transform.rotation = response.rotation;
        core.dirty = false;
        core.SetWeaponGCDTimer(response.weaponGCDTimer);
        core.SyncHealth(response.shell, response.core, response.energy);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RequestDataStringsServerRpc(ServerRpcParams serverRpcParams = default)
    {
        GetDataStringsClientRpc(playerName, blueprintString);
    }

    [ClientRpc]
    public void GetDataStringsClientRpc(string name, string blueprint, ClientRpcParams clientRpcParams = default)
    {
        playerName = name;
        blueprintString = blueprint;
        this.blueprint = SectorManager.TryGettingEntityBlueprint(blueprint);
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestIDServerRpc(ServerRpcParams serverRpcParams = default)
    {
        GetIDClientRpc(idToUse);
    }


    [ClientRpc]
    public void GetIDClientRpc(string ID, ClientRpcParams clientRpcParams = default)
    {
        this.idToUse = ID;
        if (this.idToUse == "player") 
        {
            this.idToUse = "player-"+OwnerClientId;
        }
    }

    private string tractorID;
    private bool queuedTractor = false;

    public void SetTractorID(string ID)
    {
        this.tractorID = ID;
        UpdateTractorClientRpc(ID);
    }

    [ClientRpc]
    public void UpdateTractorClientRpc(string ID, ClientRpcParams clientRpcParams = default)
    {
        queuedTractor = true;
        tractorID = ID;
    }

    [ServerRpc(RequireOwnership = true)]
    public void ChangePositionServerRpc(Vector3 newPos, ServerRpcParams serverRpcParams = default)
    {
        if (wrapper == null) wrapper = new TemporaryStateWrapper();
        if (OwnerClientId == serverRpcParams.Receive.SenderClientId)
            wrapper.position = newPos;
    }



    [ServerRpc(RequireOwnership = true)]
    public void ChangeDirectionServerRpc(Vector3 directionalVector, ServerRpcParams serverRpcParams = default)
    {   
        if (OwnerClientId == serverRpcParams.Receive.SenderClientId)
        {
            wrapper.directionalVector = directionalVector;
        }
    }

    public static Ability GetAbilityFromLocation(Vector2 location, Entity core)
    {
        if (location == Vector2.zero)
        {
            return core.GetComponent<MainBullet>();
        }

        foreach (var part in core.NetworkGetParts())
        {            
            if (!part || part.info.location != location) continue;
            return part.GetComponent<Ability>();
        }
        return null;
    }


    [ServerRpc(RequireOwnership = true)]
    public void ExecuteAbilityServerRpc(Vector2 location, Vector3 victimPos, ServerRpcParams serverRpcParams = default)
    {   
        if (!huskEntity) return;
        var weapon = GetAbilityFromLocation(location, huskEntity);
        if (!weapon) return;
        weapon.Activate();
    }

    [ClientRpc]
    public void ExecuteAbilityCosmeticClientRpc(Vector2 location, Vector3 victimPos)
    {
        if (NetworkManager.Singleton.IsServer) return;
        var core = huskEntity ? huskEntity : PlayerCore.Instance;
        if (!core) return;
        var weapon = GetAbilityFromLocation(location, core);
        if (weapon) weapon.ActivationCosmetic(victimPos);
    }

    private static float POLL_RATE = 0.00F;
    private float lastPollTime;
    public bool clientReady;

    public void ServerDetachPart(ShellPart part)
    {
        for (int i = 0; i < partStatuses.Count; i++)
        {
            if (partStatuses[i].location != part.info.location) continue;
            partStatuses[i] = new PartStatusResponse(part.info.location, true);
            break;
        }
    }


    public NetworkVariable<bool> serverReady;


    public void ServerResetParts()
    {
        for (int i = 0; i < partStatuses.Count; i++)
        {
            partStatuses[i] = new PartStatusResponse(partStatuses[i].location, true);
        }
    }

    public void SetHusk(Entity husk)
    {
        huskEntity = husk;
    }

    public string playerName;
    public bool playerNameAdded;
    private bool stringsRequested;
    public string idToUse;
    void Update()
    {

        if (!blueprint)
        {
            if (!stringsRequested)
            {
                RequestDataStringsServerRpc();
                RequestIDServerRpc();
                stringsRequested = true;
            }
            return;
        }
        
        if (queuedTractor && (tractorID == null || AIData.entities.Find(e => e.ID == tractorID)) && huskEntity is ShellCore && MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client)
        {
            queuedTractor = false;
            var core = huskEntity as ShellCore;
            if (tractorID == null)
            {
                core.SetTractorTarget(null);
            }
            else
            {
                core.SetTractorTarget(AIData.entities.Find(e => e.ID == tractorID).GetComponentInChildren<Draggable>());
            } 
        }

        if ((!NetworkManager.IsClient || NetworkManager.Singleton.LocalClientId != OwnerClientId || (!isPlayer.Value && !string.IsNullOrEmpty(idToUse))) 
            && !huskEntity && SystemLoader.AllLoaded)
        {
            huskEntity = AIData.entities.Find(e => e.ID == idToUse);
            if (!huskEntity)
            {
                Sector.LevelEntity entity = new Sector.LevelEntity();
                var response = state;
                entity.faction = response.Value.faction;
                var print = Instantiate(blueprint);
                entity.ID = idToUse;
                var ent = SectorManager.instance.SpawnEntity(print, entity);
                if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Client)
                { 
                    GetIDClientRpc(entity.ID);
                }
                ent.husk = true;
                huskEntity = ent;
                huskEntity.blueprint = print;
                if (wrapper != null)
                {
                    huskEntity.spawnPoint = huskEntity.transform.position = wrapper.position;
                }
            }
            huskEntity.networkAdapter = this;
            clientReady = true;
        }
        else if (NetworkManager.IsClient && NetworkManager.Singleton.LocalClientId == OwnerClientId && !clientReady && (serverReady.Value || NetworkManager.Singleton.IsServer) && (isPlayer.Value && !huskEntity))
        {
            var response = state;
            if (state.Value.time > 0)
            {
                PlayerCore.Instance.faction = response.Value.faction;
                PlayerCore.Instance.blueprint = Instantiate(blueprint);
                if (!SystemLoader.AllLoaded && SystemLoader.InitializeCalled)
                {
                    SystemLoader.AllLoaded = true;
                    PlayerCore.Instance.StartWrapper();
                }
                else
                {    
                    PlayerCore.Instance.Rebuild();
                }
                PlayerCore.Instance.networkAdapter = this;
                idToUse = "player";
                huskEntity = PlayerCore.Instance;
                clientReady = true;
            }
        }
        else if (NetworkManager.IsHost || (huskEntity && !huskEntity.GetIsDead()))
        {
            clientReady = true;
            if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client)
            {
                var response = state;
                if (response.Value.faction != huskEntity.faction)
                {
                    huskEntity.faction = response.Value.faction;
                    huskEntity.Rebuild();
                }
            }
        }

        if (NetworkManager.Singleton.IsServer && Time.time - lastPollTime > POLL_RATE)
        {
            lastPollTime = Time.time;
            if (huskEntity)
                state.Value = wrapper.CreateResponse(this);
        }

        if (!playerNameAdded && !string.IsNullOrEmpty(playerName) && huskEntity && ProximityInteractScript.instance && isPlayer.Value)
        {
            playerNameAdded = true;
            ProximityInteractScript.instance.AddPlayerName(huskEntity as ShellCore, playerName);
        }
        if (huskEntity && huskEntity is Craft craft && craft.husk)
        {
            craft.MoveCraft(wrapper.directionalVector);
        }

        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            var core = huskEntity ? huskEntity : isPlayer.Value ? PlayerCore.Instance : null;
            if (!core || core.GetIsDead()) return;
            foreach (var part in partStatuses)
            {
                if (!part.detached) continue;
                var foundPart = core.NetworkGetParts().Find(p => p && p.info.location == part.location);
                if (!foundPart) continue;
                core.RemovePart(foundPart);
                break;
            }
        }
    }
}
