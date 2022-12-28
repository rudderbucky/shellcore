using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkTest : NetworkBehaviour
{
    public NetworkVariable<Vector3> position;
    void Update()
    {
        transform.position = position.Value;
        if (Input.GetKey(KeyCode.W))
        {
            ChangePositionServerRpc(transform.position + Vector3.up);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            ChangePositionServerRpc(transform.position + Vector3.left);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            ChangePositionServerRpc(transform.position + Vector3.down);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            ChangePositionServerRpc(transform.position + Vector3.right);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ChangePositionServerRpc(Vector3 vector)
    {
        this.position.Value = vector;
    }

}
