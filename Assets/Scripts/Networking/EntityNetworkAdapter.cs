using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
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
        public int faction;
        public float weaponGCDTimer;
        public float shell;
        public float core;
        public float energy;
        public int power;
        public ulong clientID;
        public ServerResponse(Vector3 position, Vector3 velocity, Quaternion rotation, ulong clientID, int faction, float weaponGCDTimer, int power, float shell, float core, float energy)
        {
            this.position = position;
            this.velocity = velocity;
            this.clientID = clientID;
            this.rotation = rotation;
            this.faction = faction;
            this.weaponGCDTimer = weaponGCDTimer;
            this.power = power;
            this.shell = shell;
            this.core = core;
            this.energy = energy;
        }

        public bool Equals(ServerResponse other)
        {
            return (clientID == other.clientID &&
                (this.position - other.position).sqrMagnitude > 1 &&
                (this.velocity - other.velocity).sqrMagnitude > 1 &&
                (this.rotation.eulerAngles - other.rotation.eulerAngles).sqrMagnitude > 1 &&
                this.faction == other.faction &&
                Mathf.Abs(this.weaponGCDTimer - other.weaponGCDTimer) > 0.1F &&
                this.power == other.power &&
                Mathf.Abs(this.shell - other.shell) > 0.5F &&
                Mathf.Abs(this.core - other.core) > 0.5F &&
                Mathf.Abs(this.energy - other.energy) > 0.5F);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref clientID);
            serializer.SerializeValue(ref faction);
            serializer.SerializeValue(ref weaponGCDTimer);
            serializer.SerializeValue(ref power);
            serializer.SerializeValue(ref shell);
            serializer.SerializeValue(ref core);
            serializer.SerializeValue(ref energy);
        }
    }

    public string blueprintString;
    public EntityBlueprint blueprint;

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
            body = buf?.huskEntity?.GetComponent<Rigidbody2D>();
            core = buf?.huskEntity;
            return new ServerResponse(
            core ? core.transform.position : Vector3.zero, 
            body ? body.velocity : Vector3.zero, 
            core? core.transform.rotation : Quaternion.identity, 
            clientID, 
            core ? core.faction : buf.passedFaction, 
            core ? core.GetWeaponGCDTimer() : 0,
            core as ShellCore ? (core as ShellCore).GetPower() : 0,
            core ? core.CurrentHealth[0] : 1, 
            core ? core.CurrentHealth[1] : 1, 
            core ? core.CurrentHealth[2] : 1);
        }
    }

    public TemporaryStateWrapper wrapper;
    public NetworkVariable<bool> isPlayer = new NetworkVariable<bool>(false);
    public Vector3 pos;

    [SerializeField]
    private Entity huskEntity;
    public int passedFaction = 0;
    public static Dictionary<int, int> playerFactions;

    void Awake()
    {
        serverReady = new NetworkVariable<bool>(false);
    }


    void Start()
    {        
        if (NetworkManager.Singleton.IsServer)
        {    
            if (isPlayer.Value)
            {
                if (playerFactions == null || playerFactions.Count == 0)
                {
                    playerFactions = new Dictionary<int, int>();
                    for (int i = 0; i < SectorManager.instance.GetFactionCount(); i++)
                    {
                        playerFactions.Add(i, 0);
                    }
                }
                int minFac = 0;
                for (int i = 0; i < SectorManager.instance.GetFactionCount(); i++)
                {
                    if (!playerFactions.ContainsKey(i)) continue;
                    if (playerFactions[i] < playerFactions[minFac])
                    {
                        minFac = i;
                    }
                }

                if (passedFaction == 0 || isPlayer.Value) passedFaction = minFac;
                playerFactions[minFac]++;
                if (IsOwner && isPlayer.Value) passedFaction = 0;

                foreach(var kvp in HUDScript.scores)
                {
                    MasterNetworkAdapter.instance.SetScoreClientRpc(kvp.Key, kvp.Value);
                }

            }        
            
            UpdateStateClientRpc(wrapper.CreateResponse(this), passedFaction);


        }
        if (NetworkManager.Singleton.IsClient)
        {
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
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Server || MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Host && isPlayer.Value)
        {
            HUDScript.RemoveScore(playerName);
            MasterNetworkAdapter.instance.playerSpawned[OwnerClientId] = false;
        }
        ProximityInteractScript.instance.RemovePlayerName(huskEntity);
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && isPlayer.Value && huskEntity is IOwner owner)
        {
            foreach (var unit in owner.GetUnitsCommanding())
            {
                Destroy((unit as Entity).gameObject);
            }
        }


        if (huskEntity && !(huskEntity as PlayerCore))
        {
            Destroy(huskEntity.gameObject);
        }


        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && isPlayer.Value && playerFactions != null && playerFactions.ContainsKey(passedFaction))
        {
            playerFactions[passedFaction]--;
        }
    }

    private void UpdatePlayerState(ServerResponse response)
    {
        UpdateCoreState(PlayerCore.Instance, response);
        CameraScript.instance.Focus(PlayerCore.Instance.transform.position);
    }

    private void UpdateCoreState(Entity core, ServerResponse response)
    {
        if (isPlayer.Value && response.core > 0 && huskEntity && huskEntity.GetIsDead())
        {
            huskEntity.CancelDeath();
            (huskEntity as ShellCore).Respawn();
        }

        core.transform.position = response.position;
        core.GetComponent<Rigidbody2D>().velocity = response.velocity;
        core.transform.rotation = response.rotation;
        core.dirty = false;
        core.SetWeaponGCDTimer(response.weaponGCDTimer);
        core.SyncHealth(response.shell, response.core, response.energy);
        if (core as ShellCore) (core as ShellCore).SyncPower(response.power);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RequestDataServerRpc(ServerRpcParams serverRpcParams = default)
    {
        GetDataClientRpc(playerName, blueprintString, huskEntity is IOwnable ownable && (ownable.GetOwner() != null) ? (ownable.GetOwner() as Entity).networkAdapter.NetworkObjectId : ulong.MaxValue, passedFaction);
    }

    ulong ownerId = ulong.MaxValue;
    [ClientRpc]
    public void GetDataClientRpc(string name, string blueprint, ulong owner, int faction, ClientRpcParams clientRpcParams = default)
    {
        playerName = name;
        blueprintString = blueprint;
        ownerId = owner;
        this.passedFaction = faction;
        this.blueprint = SectorManager.TryGettingEntityBlueprint(blueprint);
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestIDServerRpc(ServerRpcParams serverRpcParams = default)
    {
        GetIDClientRpc(idToUse);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CommandMovementServerRpc(Vector3 pos, ServerRpcParams serverRpcParams = default)
    {
        if (huskEntity && huskEntity is Drone drone)
        {
            drone.CommandMovement(pos);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void CommandFollowOwnerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (huskEntity && huskEntity is Drone drone)
        {
            drone.CommandFollowOwner();
        }
    }


    [ClientRpc]
    public void GetIDClientRpc(string ID, ClientRpcParams clientRpcParams = default)
    {
        this.idToUse = ID;
        if (this.idToUse == "player" || isPlayer.Value) 
        {
            this.idToUse = "player-"+OwnerClientId;
        }
    }

    private ulong? tractorID;
    private bool queuedTractor = false;
    private bool dirty;
    public Dictionary<int, bool> weaponActivationStates = new Dictionary<int, bool>();

    [ServerRpc(RequireOwnership = false)]
    public void ForceNetworkVarUpdateServerRpc(ServerRpcParams serverRpcParams = default)
    {
        dirty = true;
    }
    public void SetTractorID(ulong? ID)
    {
        this.tractorID = ID;
        UpdateTractorClientRpc(ID.HasValue ? ID.Value : 0, !ID.HasValue);
    }

    [ServerRpc(RequireOwnership = true)]
    public void RequestTractorUpdateServerRpc(ulong id, bool setNull, ServerRpcParams serverRpcParams = default)
    {
        if (!isPlayer.Value || !huskEntity) return;
        if (setNull) 
        {
            (huskEntity as ShellCore).SetTractorTarget(null, true);
            SetTractorID(null);
        }
        else 
        {
            (huskEntity as ShellCore).SetTractorTarget(GetDraggableFromNetworkId(id), true);
            SetTractorID(id);
        }
    }

    public static Draggable GetDraggableFromNetworkId(ulong networkId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(networkId)) return null;
        var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkId];
        if (obj.GetComponent<Draggable>()) return obj.GetComponent<Draggable>();
        if (obj.GetComponent<EntityNetworkAdapter>() && obj.GetComponent<EntityNetworkAdapter>().huskEntity) 
            return obj.GetComponent<EntityNetworkAdapter>().huskEntity.GetComponent<Draggable>();
        return null;
    }

    public static bool TransformIsNetworked(Transform transform)
    {
        return transform && (transform.GetComponent<NetworkObject>() || transform.GetComponent<Entity>().networkAdapter);
    }

    public static ulong GetNetworkId(Transform transform)
    {
        if (!transform) return 0;
        var entity = transform.GetComponent<Entity>();
        ulong networkId = entity && entity.networkAdapter ? entity.networkAdapter.NetworkObjectId : 0;
        if (networkId == 0) networkId = transform.GetComponent<NetworkObject>() ? transform.GetComponent<NetworkObject>().NetworkObjectId : 0;
        return networkId;
    }

    [ClientRpc]
    public void UpdateTractorClientRpc(ulong ID, bool setNull, ClientRpcParams clientRpcParams = default)
    {
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Client) return;
        queuedTractor = true;
        tractorID = ID;
        if (setNull) tractorID = null;
    }

    public void ChangePositionWrapper(Vector3 newPos)
    {
        if (wrapper == null) wrapper = new TemporaryStateWrapper();
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
            return core.GetComponent<MainBullet>() ? core.GetComponent<MainBullet>() : core.shell ? core.shell.GetComponent<Cannon>() : null;
        }

        foreach (var part in core.NetworkGetParts())
        {            
            if (!part || part.info.location != location) continue;
            return part.GetComponent<Ability>();
        }
        return null;
    }

    [ClientRpc]
    public void SetWeaponIsEnabledClientRpc(Vector2 location, bool val, ClientRpcParams clientRpcParams = default)
    {
        if (!huskEntity) return;
        var weapon = GetAbilityFromLocation(location, huskEntity);
        weapon.isEnabled = val;
    }

    [ServerRpc(RequireOwnership = true)]
    public void ExecuteAbilityServerRpc(Vector2 location, Vector3 victimPos, ServerRpcParams serverRpcParams = default)
    {   
        if (!huskEntity) return;
        var weapon = GetAbilityFromLocation(location, huskEntity);
        if (!weapon) return;
        weapon.Activate();
    }

    [ServerRpc(RequireOwnership = true)]
    public void ExecuteVendorPurchaseServerRpc(int index, ulong vendorID, ServerRpcParams serverRpcParams = default)
    {   
        if (!isPlayer.Value) return;
        if (!NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(vendorID) ||
            !NetworkManager.SpawnManager.SpawnedObjects[vendorID].GetComponent<EntityNetworkAdapter>().huskEntity) return;
        var vendor = NetworkManager.SpawnManager.SpawnedObjects[vendorID].GetComponent<EntityNetworkAdapter>().huskEntity;
        if (!(vendor is IVendor)) return;
        VendorUI.BuyItem(huskEntity as ShellCore, index, vendor as IVendor);
    }

    [ClientRpc]
    public void ExecuteAbilityCosmeticClientRpc(Vector2 location, Vector3 victimPos)
    {
        if (NetworkManager.Singleton.IsServer) return;
        var core = huskEntity ? huskEntity : PlayerCore.Instance;
        if (!core) return;
        var weapon = GetAbilityFromLocation(location, core);
        if (weapon && MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Host) weapon.ActivationCosmetic(victimPos);
    }
    public bool clientReady;

    [ClientRpc]
    public void DetachPartClientRpc(Vector2 location, ClientRpcParams clientRpcParams = default)
    {
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Host) return;
        if (!huskEntity) return;
        for (int i = 0; i < huskEntity.NetworkGetParts().Count; i++)
        {
            var part = huskEntity.NetworkGetParts()[i];
            if (part.info.location != location) continue;
            huskEntity.RemovePart(part);
            break;
        }
    }

    [ClientRpc]
    public void UpdateStateClientRpc(ServerResponse wrapper, int faction, ClientRpcParams clientRpcParams = default)
    {
        if (wrapper.clientID == NetworkManager.Singleton.LocalClientId && isPlayer.Value)
        {
            UpdatePlayerState(wrapper);
        }
        else if (huskEntity)
        {
            UpdateCoreState(huskEntity, wrapper);
        }

        if (huskEntity && faction != huskEntity.faction)
        {
            huskEntity.faction = faction;
            huskEntity.Rebuild();
        }
    }


    public NetworkVariable<bool> serverReady;

    public void SetHusk(Entity husk)
    {
        huskEntity = husk;
    }

    public string playerName;
    public bool playerNameAdded;
    private bool stringsRequested;
    public string idToUse;


    private bool PreliminaryStatusCheck()
    {
        if (!blueprint)
        {
            if (!stringsRequested)
            {
                RequestDataServerRpc();
                RequestIDServerRpc();
                stringsRequested = true;
            }
            return false;
        }
        return true;
    }

    private void HandleQueuedTractor()
    {
        if (queuedTractor && huskEntity is ShellCore && MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client)
        {
            NetworkObject nObj = null;
            if (tractorID.HasValue && !NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(tractorID.Value))
            {
                queuedTractor = false;
                tractorID = null;
                return;
            }
            if (tractorID.HasValue) nObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[tractorID.Value];
            if (tractorID.HasValue && (!nObj || !nObj.GetComponent<EntityNetworkAdapter>() || !nObj.GetComponent<EntityNetworkAdapter>().huskEntity)) return;
            queuedTractor = false;
            var core = huskEntity as ShellCore;
            if (tractorID == null)
            {
                core.SetTractorTarget(null, false, true);
            }
            else
            {
                core.SetTractorTarget(NetworkManager.Singleton.SpawnManager.SpawnedObjects[tractorID.Value].GetComponent<EntityNetworkAdapter>().huskEntity.GetComponentInChildren<Draggable>(), false, true);
            } 
        }
    }

    public void SetUpHuskEntity()
    {
        if ((!NetworkManager.IsClient || NetworkManager.Singleton.LocalClientId != OwnerClientId || (!isPlayer.Value && !string.IsNullOrEmpty(idToUse))) 
            && !huskEntity && SystemLoader.AllLoaded)
        {
            huskEntity = AIData.entities.Find(e => e.ID == idToUse);
            if (!huskEntity)
            {
                Sector.LevelEntity entity = new Sector.LevelEntity();
                entity.faction = passedFaction;
                var print = Instantiate(blueprint);
                entity.ID = idToUse;
                entity.position = wrapper.position;
                var ent = SectorManager.instance.SpawnEntity(print, entity);
                if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Client)
                { 
                    GetIDClientRpc(entity.ID);
                }
                if (isPlayer.Value) 
                {
                    entity.ID = "player-"+OwnerClientId;
                }
                ent.husk = true;
                huskEntity = ent;
                huskEntity.blueprint = print;
                if (wrapper != null)
                {
                    huskEntity.spawnPoint = huskEntity.transform.position = wrapper.position;
                }
            }
            updateTimer = 0;
            AttemptCreateServerResponse();
            huskEntity.networkAdapter = this;
            clientReady = true;
            if (huskEntity is IOwnable ownable && ownerId != ulong.MaxValue)
            {
                var ownerAdapter = GetNetworkObject(ownerId)?.GetComponent<EntityNetworkAdapter>();
                var ownerEntity = ownerAdapter?.huskEntity as IOwner;
                if (ownerAdapter && ownerAdapter.isPlayer.Value && ownerEntity != null && !ownerEntity.Equals(null)) ownable.SetOwner(ownerEntity);
            }
            ForceNetworkVarUpdateServerRpc();
        }
        else if (NetworkManager.IsClient && NetworkManager.Singleton.LocalClientId == OwnerClientId && !clientReady && (serverReady.Value || NetworkManager.Singleton.IsHost) && (isPlayer.Value && !huskEntity))
        {
            var response = wrapper;
            if (OwnerClientId == response.clientID)
            {
                PlayerCore.Instance.faction = passedFaction;
                PlayerCore.Instance.blueprint = Instantiate(blueprint);
                PlayerCore.Instance.SetPlayerSpawnPoint();
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
                ForceNetworkVarUpdateServerRpc();
            }
        }
        else if (NetworkManager.IsHost || (huskEntity && !huskEntity.GetIsDead()))
        {
            clientReady = true;
        }
    }

    private static float UPDATE_RATE_FOR_PLAYERS = 0F; 
    private static float UPDATE_RATE = 0.05F;
    private float updateTimer = UPDATE_RATE;
    private void AttemptCreateServerResponse()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var closeToPlayer = isPlayer.Value || !serverReady.Value;
            if (!closeToPlayer && huskEntity)
            {
                foreach(var ent in AIData.shellCores)
                {
                    if (ent && (ent.transform.position - huskEntity.transform.position).sqrMagnitude < MasterNetworkAdapter.POP_IN_DISTANCE)
                    {
                        closeToPlayer = true;
                        break;
                    }
                }
            }
            updateTimer -= Time.deltaTime;
            if ((huskEntity && closeToPlayer && updateTimer <= 0) || !serverReady.Value || dirty)
            {
                updateTimer = isPlayer.Value ? UPDATE_RATE_FOR_PLAYERS : (UPDATE_RATE + (AIData.entities.Count > 50 ? 1 : 0));
                dirty = false;
                UpdateStateClientRpc(wrapper.CreateResponse(this), huskEntity ? huskEntity.faction : passedFaction);
            };
        }
    }

    void Update()
    {   
        if (!PreliminaryStatusCheck()) return;
        HandleQueuedTractor();
        SetUpHuskEntity();
        AttemptCreateServerResponse();
        
        if (!playerNameAdded && !string.IsNullOrEmpty(playerName) && huskEntity as ShellCore && ProximityInteractScript.instance && isPlayer.Value)
        {
            playerNameAdded = true;
            ProximityInteractScript.instance.AddPlayerName(huskEntity as ShellCore, playerName);
        }
        if (huskEntity && huskEntity is Craft craft && craft.husk && isPlayer.Value && !(MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Host && huskEntity == PlayerCore.Instance))
        {
            craft.MoveCraft(wrapper.directionalVector);
        }
    }
}
