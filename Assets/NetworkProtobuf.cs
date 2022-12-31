using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkProtobuf : NetworkBehaviour
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

    public EntityBlueprint demoBlueprint;

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

        public ServerResponse CreateResponse(NetworkProtobuf buf)
        {
            Rigidbody2D body = null;
            ShellCore core = null;
            if (NetworkManager.Singleton.IsHost && !buf.huskCore)
            {
                body = PlayerCore.Instance.GetComponent<Rigidbody2D>();
                core = PlayerCore.Instance;
            }
            else
            {
                body = buf.huskCore.GetComponent<Rigidbody2D>();
                core = buf.huskCore;
            }
            return new ServerResponse(core.transform.position, body.velocity, core.transform.rotation, clientID, core.faction, core.GetWeaponGCDTimer(), core.CurrentHealth[0], core.CurrentHealth[1], core.CurrentHealth[2]);
        }
    }

    public TemporaryStateWrapper wrapper;

    public NetworkVariable<ServerResponse> state = new NetworkVariable<ServerResponse>();

    public EntityBlueprint coreBlueprint;
    private ShellCore huskCore;

    void Awake()
    {
        if (partStatuses == null) partStatuses = new NetworkList<PartStatusResponse>();
    }
    void Start()
    {        
        if (NetworkManager.Singleton.IsServer)
        {
            int fac = NetworkManager.Singleton.ConnectedClients == null ? 0 : NetworkManager.Singleton.ConnectedClients.Count - 1;
            state.Value = new ServerResponse(Vector3.zero, Vector3.zero, Quaternion.identity, OwnerClientId, fac, 0, 1000, 250, 500);
        }
        if (NetworkManager.Singleton.IsClient)
        {
            state.OnValueChanged += (x, y) =>
            {
                if (y.clientID == NetworkManager.Singleton.LocalClientId)
                {
                    UpdatePlayerState(y);
                }
                else if (huskCore)
                {
                    UpdateCoreState(huskCore, y);
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

        if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClientId == OwnerClientId)
        {        
            PlayerCore.Instance.protobuf = this;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (huskCore)
        {
            Destroy(huskCore.gameObject);
        }
    }

    private void UpdatePlayerState(ServerResponse response)
    {
        UpdateCoreState(PlayerCore.Instance, response);
    }

    private void UpdateCoreState(ShellCore core, ServerResponse response)
    {
        core.transform.position = response.position;
        core.GetComponent<Rigidbody2D>().velocity = response.velocity;
        core.transform.rotation = response.rotation;
        core.dirty = false;
        core.SetWeaponGCDTimer(response.weaponGCDTimer);
        core.SyncHealth(response.shell, response.core, response.energy);
    }

    
    [ServerRpc(RequireOwnership = true)]
    public void ChangePositionServerRpc(Vector3 newPos, ServerRpcParams serverRpcParams = default)
    {
        if (OwnerClientId == serverRpcParams.Receive.SenderClientId)
            wrapper.position = newPos;
    }

    [ServerRpc(RequireOwnership = true)]
    public void ChangeDirectionServerRpc(Vector3 directionalVector, ServerRpcParams serverRpcParams = default)
    {   
        if (OwnerClientId == serverRpcParams.Receive.SenderClientId)
            wrapper.directionalVector = directionalVector;
    }

    public static WeaponAbility GetWeaponFromLocation(Vector2 location, ShellCore core)
    {
        if (location == Vector2.zero)
        {
            return core.GetComponent<MainBullet>();
        }

        foreach (var part in core.NetworkGetParts())
        {            
            if (part.info.location != location || !part.weapon) continue;
            return part.GetComponent<WeaponAbility>();
        }
        return null;
    }


    [ServerRpc(RequireOwnership = true)]
    public void ExecuteAbilityServerRpc(Vector2 location, Vector3 victimPos, ServerRpcParams serverRpcParams = default)
    {   
        if (!huskCore) return;
        var weapon = GetWeaponFromLocation(location, huskCore);
        if (weapon) weapon.Activate();

    }

    private static float POLL_RATE = 0.00F;
    private float lastPollTime;
    public bool playerReady;

    public void ServerDetachPart(ShellPart part)
    {
        for (int i = 0; i < partStatuses.Count; i++)
        {
            if (partStatuses[i].location != part.info.location) continue;
            partStatuses[i] = new PartStatusResponse(part.info.location, true);
            break;
        }
    }

    public void ServerResetParts()
    {
        for (int i = 0; i < partStatuses.Count; i++)
        {
            partStatuses[i] = new PartStatusResponse(partStatuses[i].location, true);
        }
    }

    void Update()
    {
        if ((!NetworkManager.IsClient || NetworkManager.Singleton.LocalClientId != OwnerClientId) && !huskCore)
        {
            Sector.LevelEntity entity = new Sector.LevelEntity();
            entity.ID = OwnerClientId.ToString();
            var response = state;
            entity.faction = response.Value.faction;
            var print = Instantiate(coreBlueprint);
            var ent = SectorManager.instance.SpawnEntity(print, entity);
            (ent as ShellCore).husk = true;
            huskCore = ent as ShellCore;
            huskCore.blueprint = demoBlueprint;
            huskCore.protobuf = this;
            playerReady = true;
        }
        else if (NetworkManager.IsClient && NetworkManager.Singleton.LocalClientId == OwnerClientId && !playerReady)
        {
            var response = state;
            if (state.Value.time > 0)
            {
                PlayerCore.Instance.faction = response.Value.faction;
                PlayerCore.Instance.blueprint = demoBlueprint;
                PlayerCore.Instance.Rebuild();
                playerReady = true;
            }
        }
        else if (NetworkManager.IsHost) playerReady = true;

        if (huskCore)
        {
            huskCore.MoveCraft(wrapper.directionalVector);
        }

        if (NetworkManager.Singleton.IsServer && Time.time - lastPollTime > POLL_RATE)
        {
            lastPollTime = Time.time;
            state.Value = wrapper.CreateResponse(this);
        }




        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            var core = huskCore ? huskCore : PlayerCore.Instance;

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
