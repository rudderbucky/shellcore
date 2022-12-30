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

    public struct PartStatusResponse : INetworkSerializable, IEquatable<PartStatusResponse>
    {
        public Vector2 location;
        public bool detached;

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


    [ServerRpc(RequireOwnership = true)]
    public void ExecuteWeaponServerRpc(int abilityID, Vector3 victimPos, ServerRpcParams serverRpcParams = default)
    {   
        if (huskCore)
            (huskCore.GetAbilities()[0] as Bullet).BulletTest(victimPos);
    }

    [ServerRpc(RequireOwnership = true)]
    public void DetachPartServerRpc(Vector2 position, ServerRpcParams serverRpcParams = default)
    {   
        if (huskCore)
            foreach (var part in huskCore.NetworkGetParts())
            {
                if (part.info.location != position) continue;
                huskCore.RemovePart(part);
                break;
            }
    }


    private static float POLL_RATE = 0.05F;
    private float lastPollTime;
    private bool playerReady;

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
        }
        else if (!NetworkManager.IsServer && NetworkManager.Singleton.LocalClientId == OwnerClientId && !playerReady)
        {
            var response = state;
            if (state.Value.time > 0)
            {
                PlayerCore.Instance.faction = response.Value.faction;
                PlayerCore.Instance.Rebuild();
                playerReady = true;
            }
        }

        if (huskCore)
        {
            huskCore.MoveCraft(wrapper.directionalVector);
        }

        if (NetworkManager.Singleton.IsServer && Time.time - lastPollTime > POLL_RATE)
        {
            lastPollTime = Time.time;
            state.Value = wrapper.CreateResponse(this);
        }
    }
}
