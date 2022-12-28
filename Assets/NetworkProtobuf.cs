using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkProtobuf : NetworkBehaviour
{
    public struct ServerResponse : INetworkSerializable
    {
        public Vector3 position;
        public Vector3 velocity;
        public Quaternion rotation;
        public float time;
        public string entityID;

        public ServerResponse(Vector3 position, Vector3 velocity, Quaternion rotation, string entityID)
        {
            this.position = position;
            this.velocity = velocity;
            this.entityID = entityID;
            this.time = Time.time;
            this.rotation = rotation;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref time);
            serializer.SerializeValue(ref rotation);
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
        public string entityID;

        public ServerResponse CreateResponse(NetworkProtobuf buf)
        {
            var body = buf.huskCore.GetComponent<Rigidbody2D>();
            return new ServerResponse(buf.huskCore.transform.position, body.velocity, buf.huskCore.transform.rotation, entityID);
        }
    }

    public TemporaryStateWrapper wrapper;

    public NetworkVariable<ServerResponse> state;
    public static NetworkProtobuf instance;

    public EntityBlueprint coreBlueprint;
    private ShellCore huskCore;

    public override void OnNetworkSpawn()
    {
        instance = this;
        wrapper = new TemporaryStateWrapper();
        if (NetworkManager.Singleton.IsClient)
        {
            state.OnValueChanged += (x, y) => 
            {
                UpdatePlayerState();
            };
        }
        else
        {
            Sector.LevelEntity entity = new Sector.LevelEntity();
            entity.ID = OwnerClientId.ToString();
            var ent = SectorManager.instance.SpawnEntity(Instantiate(coreBlueprint), entity);
            (ent as ShellCore).husk = true;
            huskCore = (ent as ShellCore);
        }
    }

    public override void OnNetworkDespawn()
    {
        Destroy(huskCore.gameObject);
    }

    public void UpdatePlayerState()
    {
        PlayerCore.Instance.transform.position = state.Value.position;
        PlayerCore.Instance.GetComponent<Rigidbody2D>().velocity = state.Value.velocity;
        PlayerCore.Instance.transform.rotation = state.Value.rotation;
        PlayerCore.Instance.dirty = false;
    }

    
    [ServerRpc(RequireOwnership = false)]
    public void ChangePositionServerRpc(Vector3 newPos, ServerRpcParams serverRpcParams = default)
    {
        wrapper.position = newPos;
        state.Value = wrapper.CreateResponse(this);
        //NetworkProtobuf.instance.state.Value.velocity = newPos;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeDirectionServerRpc(Vector3 directionalVector, ServerRpcParams serverRpcParams = default)
    {
        wrapper.directionalVector = directionalVector;
        //NetworkProtobuf.instance.state.Value.velocity = newPos;
    }

    private static float POLL_RATE = 0.05F;
    private float lastPollTime;

    void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            huskCore.MoveCraft(wrapper.directionalVector);
            if (Time.time - lastPollTime > POLL_RATE)
            {
                lastPollTime = Time.time;
                state.Value = wrapper.CreateResponse(this);
            }
        }
    }

}
