using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkProtobuf : NetworkBehaviour
{
    public class ObjectProtocolBuffer : INetworkSerializable
    {
        public Vector3 position;
        public Vector3 velocity;
        public string entityID;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
        }

        /*
        public override string ToString()
        {
            return $"{position[0]},{position[1]},{position[2]}";
        }
        */
    }

    public NetworkVariable<Vector3> states;
    public static NetworkProtobuf instance;


    public override void OnNetworkSpawn()
    {
        instance = this;
        states.OnValueChanged += (x, y) => 
        {
            if (!NetworkManager.Singleton.IsClient) return;
            PlayerCore.Instance.transform.position = y;
            CameraScript.instance.Focus(y);
            PlayerCore.Instance.dirty = false;
        };
    }

    
    [ServerRpc(RequireOwnership = false)]
    public void ChangePositionServerRpc(Vector3 newPos, ServerRpcParams serverRpcParams = default)
    {
        NetworkProtobuf.instance.states.Value = newPos;
    }
}
