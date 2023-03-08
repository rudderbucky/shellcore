using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class NetworkPowerOrbWrapper : NetworkBehaviour
{
    
    void Awake()
    {

    }

    [ClientRpc]
    public void SetPositionClientRpc(Vector3 position)
    {
        transform.position = position; 
    }
    
    void Update()
    {
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && NetworkManager.Singleton && NetworkManager.IsServer)
        {
            SetPositionClientRpc(transform.position);
        }
    }
}
