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
        public ulong clientID;

        public ServerResponse(Vector3 position, Vector3 velocity, Quaternion rotation, ulong clientID)
        {
            this.position = position;
            this.velocity = velocity;
            this.clientID = clientID;
            this.time = Time.time;
            this.rotation = rotation;
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
            return new ServerResponse(buf.huskCores[clientID].transform.position, body.velocity, buf.huskCores[clientID].transform.rotation, clientID);
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
            states.Add(new ServerResponse(Vector3.zero, Vector3.zero, Quaternion.identity, OwnerClientId));
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

        if (!NetworkManager.IsClient || NetworkManager.Singleton.LocalClientId != OwnerClientId)
        {
            Sector.LevelEntity entity = new Sector.LevelEntity();
            entity.ID = OwnerClientId.ToString();
            var ent = SectorManager.instance.SpawnEntity(Instantiate(coreBlueprint), entity);
            (ent as ShellCore).husk = true;
            huskCores.Add(OwnerClientId, ent as ShellCore);
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

    private static float POLL_RATE = 0.05F;
    private float lastPollTime;

    void Update()
    {

        if (NetworkManager.Singleton.IsServer)
        {
            if (huskCores != null && huskCores.ContainsKey(wrapper.clientID))
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

}
