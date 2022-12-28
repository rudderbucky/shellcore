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
        public string entityID;

        public ServerResponse(Vector3 position, Vector3 velocity, string entityID)
        {
            this.position = position;
            this.velocity = velocity;
            this.entityID = entityID;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref velocity);
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
        public Vector3 velocity;
        public string entityID;

        public ServerResponse CreateResponse()
        {
            return new ServerResponse(position, velocity, entityID);
        }
    }

    public TemporaryStateWrapper wrapper;

    public NetworkVariable<ServerResponse> state;
    public static NetworkProtobuf instance;

    public override void OnNetworkSpawn()
    {
        instance = this;
        wrapper = new TemporaryStateWrapper();
        state.OnValueChanged += (x, y) => {
            if (NetworkManager.Singleton.IsClient)
            {
                UpdatePlayerState();
            }
        };
    }

    public void UpdatePlayerState()
    {
        PlayerCore.Instance.transform.position = NetworkProtobuf.instance.state.Value.velocity;
        CameraScript.instance.Focus(NetworkProtobuf.instance.state.Value.velocity);
        PlayerCore.Instance.dirty = false;
        UnsetDirtyServerRpc();
    }

    
    [ServerRpc(RequireOwnership = false)]
    public void ChangePositionServerRpc(Vector3 newPos, ServerRpcParams serverRpcParams = default)
    {
        wrapper.velocity = newPos;
        state.Value = wrapper.CreateResponse();
        //NetworkProtobuf.instance.state.Value.velocity = newPos;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UnsetDirtyServerRpc(ServerRpcParams serverRpcParams = default)
    {
    }
}
