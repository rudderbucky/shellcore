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

    public struct ClientMessage : INetworkSerializable
    {
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            throw new System.NotImplementedException();
        }
    }

    public class StateWrapper
    {
        public Vector3 position;
        public Vector3 directionalVector;
        public ulong clientID;

        public ServerResponse CreateResponse(EntityNetworkAdapter buf)
        {
            Rigidbody2D body = null;
            Entity core = null;
            if (buf && buf.huskEntity)
            {
                body = buf.huskEntity.GetComponent<Rigidbody2D>();
                core = buf.huskEntity;
            }
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

    public string blueprintString;
    public EntityBlueprint blueprint;
    public StateWrapper wrapper;
    public NetworkVariable<bool> isPlayer = new NetworkVariable<bool>(false);
    public Vector3 pos;
    [SerializeField]
    private Entity huskEntity;
    public int passedFaction = 0;
    public static Dictionary<int, int> playerFactions;
    private ulong? tractorID;
    private bool queuedTractor = false;
    private bool dirty;
    public Dictionary<int, bool> weaponActivationStates = new Dictionary<int, bool>();
    public bool clientReady;
    private bool wrapperUpdated = false;
    public NetworkVariable<bool> serverReady;
    public NetworkVariable<bool> safeToRespawn = new NetworkVariable<bool>(true);
    public string playerName;
    public bool playerNameAdded;
    private bool stringsRequested;
    public string idToUse;
    static float TIME_TO_OOB_DEATH = 3;
    float oobKillTimer = TIME_TO_OOB_DEATH;
    private static float UPDATE_RATE_FOR_PLAYERS = 0F; 
    private static float UPDATE_RATE = 0.05F;
    private float updateTimer = UPDATE_RATE;
    ulong ownerId = ulong.MaxValue;

    [ServerRpc(RequireOwnership = false)]
    public void RequestDataServerRpc(ServerRpcParams serverRpcParams = default)
    {
        GetDataClientRpc(playerName, blueprintString, huskEntity is IOwnable ownable && (ownable.GetOwner() != null) ? (ownable.GetOwner() as Entity).networkAdapter.NetworkObjectId : ulong.MaxValue, passedFaction);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestIDServerRpc(ServerRpcParams serverRpcParams = default)
    {
        GetIDClientRpc(idToUse);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CommandMovementServerRpc(Vector3 pos, ServerRpcParams serverRpcParams = default)
    {
        if (!huskEntity || !(huskEntity is Drone drone)) return;
        drone.CommandMovement(pos);
    }


    [ServerRpc(RequireOwnership = false)]
    public void CommandFollowOwnerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (!huskEntity || !(huskEntity is Drone drone)) return;
        drone.CommandFollowOwner();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ForceNetworkVarUpdateServerRpc(ServerRpcParams serverRpcParams = default)
    {
        dirty = true;
    }
    [ServerRpc(RequireOwnership = true)]
    public void RequestTargetChangeServerRpc(ulong id, ServerRpcParams serverRpcParams = default)
    {
        if (!isPlayer.Value || !huskEntity) return;
        if (id == ulong.MaxValue) 
        {
            huskEntity.GetTargetingSystem().SetTarget(null);
            return;
        }

        var ent = GetEntityFromNetworkId(id);
        if (!ent) return;
        huskEntity.GetTargetingSystem().SetTarget(ent.transform);
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

    [ServerRpc(RequireOwnership = true)]
    public void ChangeDirectionServerRpc(Vector3 directionalVector, ServerRpcParams serverRpcParams = default)
    {   
        if (OwnerClientId != serverRpcParams.Receive.SenderClientId) return;
        wrapper.directionalVector = directionalVector;
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
    public void GetDataClientRpc(string name, string blueprint, ulong owner, int faction, ClientRpcParams clientRpcParams = default)
    {
        playerName = name;
        blueprintString = blueprint;
        ownerId = owner;
        this.passedFaction = faction;
        this.blueprint = SectorManager.TryGettingEntityBlueprint(blueprint);
    }


    [ClientRpc]
    public void GetIDClientRpc(string ID, ClientRpcParams clientRpcParams = default)
    {
        this.idToUse = ID;
        if (this.idToUse != "player" && !isPlayer.Value) return;
        this.idToUse = "player-"+OwnerClientId;
    }


    [ClientRpc]
    public void UpdateTractorClientRpc(ulong ID, bool setNull, ClientRpcParams clientRpcParams = default)
    {
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Client) return;
        queuedTractor = true;
        tractorID = ID;
        if (setNull) tractorID = null;
    }


    [ClientRpc]
    public void SetWeaponIsEnabledClientRpc(Vector2 location, bool val, ClientRpcParams clientRpcParams = default)
    {
        if (!huskEntity) return;
        var weapon = GetAbilityFromLocation(location, huskEntity);
        weapon.isEnabled = val;
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
        this.wrapper.position = wrapper.position;
        wrapperUpdated = true;
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

    [ClientRpc]

    public void InflictionCosmeticClientRpc(int abilityID, ClientRpcParams clientRpcParams = default)
    {
        if (!huskEntity || huskEntity.GetIsDead() || MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Host) return;
        switch (abilityID)
        {
            case (int)AbilityID.Disrupt:
                Disrupt.InflictionCosmetic(huskEntity);
                break;            
            case (int)AbilityID.PinDown:
                PinDown.InflictionCosmetic(huskEntity);
                break;
            default:
                return;
        }
    }

    void Awake()
    {
        serverReady = new NetworkVariable<bool>(false);
    }

    private void DeterminePlayerFaction()
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

    void Start()
    {        
        if (!NetworkManager.Singleton.IsServer) return;
        if (isPlayer.Value)
        {
           DeterminePlayerFaction(); 
        }        
            
        UpdateStateClientRpc(wrapper.CreateResponse(this), passedFaction);
    }

    public override void OnNetworkSpawn()
    {
        if (wrapper == null)
        {
            wrapper = new StateWrapper();
            wrapper.clientID = OwnerClientId;
        }

        if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClientId == OwnerClientId && isPlayer.Value)
        {
            PlayerCore.Instance.networkAdapter = this;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!MasterNetworkAdapter.lettingServerDecide && isPlayer.Value)
        {
            HUDScript.RemoveScore(playerName);
            MasterNetworkAdapter.instance.playerSpawned[OwnerClientId] = false;
        }
        ProximityInteractScript.instance.RemovePlayerName(huskEntity);
        if (isPlayer.Value && huskEntity is IOwner owner)
        {
            foreach (var unit in owner.GetUnitsCommanding())
            {
                Destroy((unit as Entity).gameObject);
            }
        }

        if (huskEntity && !(huskEntity as PlayerCore))
        {
            huskEntity.TakeCoreDamage(999999);
            if (huskEntity is ShellCore core) core.KillShellCore();
            Destroy(huskEntity.gameObject);
        }

        if (isPlayer.Value && playerFactions != null && playerFactions.ContainsKey(passedFaction))
        {
            playerFactions[passedFaction]--;
        }

        if (isPlayer.Value) 
        {
            MasterNetworkAdapter.AttemptServerIntroduce();
            SectorManager.instance.UpdateCounters();
        }
    }

    private void UpdatePlayerState(ServerResponse response)
    {
        UpdateCoreState(PlayerCore.Instance, response);
        CameraScript.instance.Focus(PlayerCore.Instance.transform.position);
    }

    private void UpdateCoreState(Entity core, ServerResponse response)
    {
        if (isPlayer.Value && response.core > 0 && huskEntity && huskEntity.GetIsDead() && safeToRespawn.Value)
        {
            huskEntity.CancelDeath();
            (huskEntity as ShellCore).Respawn(true);
        }

        core.transform.position = response.position;
        core.GetComponent<Rigidbody2D>().velocity = response.velocity;
        core.transform.rotation = response.rotation;
        core.dirty = false;
        core.SetWeaponGCDTimer(response.weaponGCDTimer);
        core.SyncHealth(response.shell, response.core, response.energy);
        if (core is ShellCore shellcore) 
            shellcore.SyncPower(response.power);
    }
    

    public void SetTractorID(ulong? ID)
    {
        this.tractorID = ID;
        UpdateTractorClientRpc(ID.HasValue ? ID.Value : 0, !ID.HasValue);
    }

    private static Entity GetEntityFromNetworkId(ulong networkId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(networkId)) return null;
        var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkId];
        if (obj.GetComponent<EntityNetworkAdapter>() && obj.GetComponent<EntityNetworkAdapter>().huskEntity) 
            return obj.GetComponent<EntityNetworkAdapter>().huskEntity;
        return null;
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

    public void ChangePositionWrapper(Vector3 newPos)
    {
        if (wrapper == null) wrapper = new StateWrapper();
        wrapper.position = newPos;
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


    public void SetHusk(Entity husk)
    {
        huskEntity = husk;
    }


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
        if (!queuedTractor || !(huskEntity is ShellCore) || MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Client) return;
        NetworkObject nObj = null;
        if (tractorID.HasValue && !NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(tractorID.Value))
        {
            queuedTractor = false;
            tractorID = null;
            return;
        }
        if (tractorID.HasValue) nObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[tractorID.Value];
        if (tractorID.HasValue && (!TransformIsNetworked(nObj.transform)))
        {
            return;
        }
        var core = huskEntity as ShellCore;
        if (tractorID == null)
        {
            queuedTractor = false;
            core.SetTractorTarget(null, false, true);
        }
        else
        {
            var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[tractorID.Value];
            var ent = obj?.GetComponent<EntityNetworkAdapter>()?.huskEntity?.GetComponentInChildren<Draggable>();
            if (ent)
            {
                core.SetTractorTarget(ent, false, true);
                queuedTractor = false;
            }
            else if (obj.GetComponentInChildren<Draggable>())
            {
                core.SetTractorTarget(obj.GetComponentInChildren<Draggable>(), false, true);
                queuedTractor = false;
            }
        }
    }

    /*
    [ServerRpc(RequireOwnership = false)]
    public void GodModeServerRpc(ServerRpcParams serverRpcParams = default)
    {
        DevConsoleScript.Instance.GodPowers(huskEntity as ShellCore);
    }
    */

    private bool ShouldSpawnHuskEntity()
    {
        return (!NetworkManager.IsClient || (NetworkManager.Singleton.LocalClientId != OwnerClientId && wrapperUpdated) || (!isPlayer.Value && !string.IsNullOrEmpty(idToUse))) 
            && !huskEntity && SystemLoader.AllLoaded;
    }

    private bool ShouldReusePlayerCore()
    {
        return NetworkManager.IsClient && NetworkManager.Singleton.LocalClientId == OwnerClientId && !clientReady && (serverReady.Value || NetworkManager.Singleton.IsHost) && (isPlayer.Value && !huskEntity);
    }

    private void CreateHuskEntity()
    {
        Sector.LevelEntity entity = new Sector.LevelEntity();
        entity.name = blueprint.entityName;
        if (isPlayer.Value) entity.name = playerName;
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

    private void SetUpPlayerCore()
    {
        PlayerCore.Instance.enabled = true;
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
    
    public void SetUpHuskEntity()
    {
        if (!safeToRespawn.Value) return;
        if (ShouldSpawnHuskEntity())
        {
            huskEntity = AIData.entities.Find(e => e.ID == idToUse);
            if (!huskEntity)
            {
                CreateHuskEntity();
            }
            else if (!MasterNetworkAdapter.lettingServerDecide)
            {
                serverReady.Value = true;
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
        else if (ShouldReusePlayerCore())
        {
            var response = wrapper;
            if (OwnerClientId == response.clientID)
            {
                SetUpPlayerCore();
            }
        }
        else if (NetworkManager.IsHost || (huskEntity && !huskEntity.GetIsDead()))
        {
            clientReady = true;
        }
    }

    private void AttemptCreateServerResponse()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        var closeToPlayer = isPlayer.Value || !serverReady.Value;
        var closePlayers = new List<ulong>();
        if (isPlayer.Value) closePlayers.Add(OwnerClientId);
        if (!closeToPlayer && huskEntity)
        {
            foreach(var ent in AIData.shellCores)
            {
                if (!ent || !ent.networkAdapter || !ent.networkAdapter.isPlayer.Value || (ent.transform.position - huskEntity.transform.position).sqrMagnitude > MasterNetworkAdapter.POP_IN_DISTANCE)
                    continue;
                closeToPlayer = true;
                if (NetworkManager.Singleton.ConnectedClients.ContainsKey(ent.networkAdapter.OwnerClientId))
                    closePlayers.Add(ent.networkAdapter.OwnerClientId);
                break;
            }
        }
        updateTimer -= Time.deltaTime;
        var useCustomParams = !dirty && (serverReady.Value || !(huskEntity is Craft craft) || !craft.IsMoving() || !craft.GetIsDead());

        if ((huskEntity && closeToPlayer && updateTimer <= 0) || !useCustomParams)
        {
            updateTimer = isPlayer.Value ? UPDATE_RATE_FOR_PLAYERS : (UPDATE_RATE + (AIData.entities.Count > 200 ? 1 : 0));
            dirty = false;
            UpdateStateClientRpc(wrapper.CreateResponse(this), huskEntity ? huskEntity.faction : passedFaction);
            if (isPlayer.Value || !useCustomParams)
                UpdateStateClientRpc(wrapper.CreateResponse(this), huskEntity ? huskEntity.faction : passedFaction);
            else 
            {
                UpdateStateClientRpc(wrapper.CreateResponse(this), huskEntity ? huskEntity.faction : passedFaction, 
                new ClientRpcParams() 
                {
                    Send = new ClientRpcSendParams()
                    {
                        TargetClientIds = closePlayers
                    }
                });
            }
        };
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

        if (huskEntity is Craft craft && craft.husk && isPlayer.Value && !(MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Host && huskEntity == PlayerCore.Instance))
        {
            craft.MoveCraft(wrapper.directionalVector);
        }

        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Client 
            && isPlayer.Value 
            && huskEntity is ShellCore core 
            && !core.GetIsDead() 
            && !SectorManager.instance.CurrentContainsPosition(core.GetSectorPosition()))
        {
            oobKillTimer -= Time.deltaTime;
            if (oobKillTimer < 0)
            {
                core.TakeCoreDamage(float.MaxValue);
            }
        }
        else oobKillTimer = TIME_TO_OOB_DEATH;
    }
}
