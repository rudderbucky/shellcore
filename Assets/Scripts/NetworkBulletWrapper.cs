using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkBulletWrapper : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        GetComponent<BulletScript>().enabled = false;
        base.OnNetworkSpawn();
    }
    
    [ClientRpc]
    private void SetPositionClientRpc(Vector3 position)
    {
        transform.position = position; 
    }
    
    void Update()
    {
        if (DevConsoleScript.networkEnabled && NetworkManager.IsServer)
        {
            SetPositionClientRpc(transform.position);
        }
    }
}
