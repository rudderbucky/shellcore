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

        public ulong clientID;
        public ServerResponse(Vector3 position, Vector3 velocity, Quaternion rotation, ulong clientID, int faction, float weaponGCDTimer)
        {
            this.position = position;
            this.velocity = velocity;
            this.clientID = clientID;
            this.time = Time.time;
            this.rotation = rotation;
            this.faction = faction;
            this.weaponGCDTimer = weaponGCDTimer;
        }

        public bool Equals(ServerResponse other)
        {
            Debug.LogWarning(clientID == other.clientID && this.time == other.time);
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
        }

        /*
        public override string ToString()
        {
            return $"{position[0]},{position[1]},{position[2]}";
        }
        */
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
            var body = buf.huskCores[clientID].GetComponent<Rigidbody2D>();
            var core = buf.huskCores[clientID];
            return new ServerResponse(core.transform.position, body.velocity, core.transform.rotation, clientID, core.faction, core.GetWeaponGCDTimer());
        }
    }

    public TemporaryStateWrapper wrapper;

    public NetworkList<ServerResponse> states;

    public EntityBlueprint coreBlueprint;
    private Dictionary<ulong, ShellCore> huskCores;

    void Awake()
    {
        if (states == null)
        {
            states = new NetworkList<ServerResponse>();
        }
    }
    void Start()
    {        
        if (!NetworkManager.Singleton.IsClient)
        {
            states.Add(new ServerResponse(Vector3.zero, Vector3.zero, Quaternion.identity, OwnerClientId, NetworkManager.Singleton.ConnectedClients.Count - 1, 0));
        }
        else
        {
            states.OnListChanged += (ce) =>
            {
                if (ce.Value.clientID == NetworkManager.Singleton.LocalClientId)
                {
                    UpdatePlayerState(ce.Value);
                }
                else
                {
                    if (huskCores.ContainsKey(ce.Value.clientID))
                    {
                        UpdateCoreState(huskCores[ce.Value.clientID], ce.Value);
                    }
                }
            };
        }
    }

    public override void OnNetworkSpawn()
    {
        if (huskCores == null)
        {
            huskCores = new Dictionary<ulong, ShellCore>();
        }

        if (wrapper == null)
        {
            wrapper = new TemporaryStateWrapper();
            wrapper.clientID = OwnerClientId;
        }

        if (NetworkManager.Singleton.IsClient)
        {        
            if (NetworkManager.Singleton.LocalClientId == OwnerClientId)
                PlayerCore.Instance.protobuf = this;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (huskCores == null) return;
        if (huskCores.ContainsKey(OwnerClientId))
        {
            Destroy(huskCores[OwnerClientId].gameObject);
            huskCores.Remove(OwnerClientId);
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
        if (OwnerClientId == serverRpcParams.Receive.SenderClientId && huskCores.ContainsKey(OwnerClientId))
            (huskCores[OwnerClientId].GetAbilities()[0] as Bullet).BulletTest(victimPos);
    }

    private static float POLL_RATE = 0.05F;
    private float lastPollTime;
    private bool playerReady;

    void Update()
    {

        if ((!NetworkManager.IsClient || NetworkManager.Singleton.LocalClientId != OwnerClientId) && !huskCores.ContainsKey(OwnerClientId))
        {
            Sector.LevelEntity entity = new Sector.LevelEntity();
            entity.ID = OwnerClientId.ToString();
            var response = GetServerResponse(OwnerClientId);
            if (!response.HasValue) return;
            if (NetworkManager.IsServer)
                entity.faction = response.Value.faction;
            else if (NetworkManager.IsClient)
            {
                entity.faction = response.Value.faction;
            }

            var print = Instantiate(coreBlueprint);
            

            var ent = SectorManager.instance.SpawnEntity(print, entity);
            (ent as ShellCore).husk = true;
            huskCores.Add(OwnerClientId, ent as ShellCore);
        }
        else if (NetworkManager.IsClient && NetworkManager.Singleton.LocalClientId == OwnerClientId && !playerReady)
        {
            var response = GetServerResponse(OwnerClientId);
            if (response.HasValue) 
            {
                PlayerCore.Instance.faction = response.Value.faction;
                PlayerCore.Instance.Rebuild();
                playerReady = true;
            }
        }


        if (NetworkManager.Singleton.IsServer)
        {
            if (!huskCores.ContainsKey(wrapper.clientID)) return;
            if (huskCores != null)
            {
                huskCores[wrapper.clientID].MoveCraft(wrapper.directionalVector);
            }

            if (Time.time - lastPollTime > POLL_RATE)
            {
                lastPollTime = Time.time;
                for (int i = 0; i < states.Count; i++)
                {
                    if (states[i].clientID != wrapper.clientID) continue;
                    states[i] = wrapper.CreateResponse(this);
                }
            }
        }
    }

    private ServerResponse? GetServerResponse(ulong clientID)
    {
        for (int i = 0; i < states.Count; i++)
        {
            if (states[i].clientID == wrapper.clientID) return states[i];
        }
        return null;
    }

}
