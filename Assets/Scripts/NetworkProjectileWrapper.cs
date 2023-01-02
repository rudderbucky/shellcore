using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkProjectileWrapper : NetworkBehaviour
{
    public override void OnNetworkDespawn()
    {
        if (NetworkAdaptor.mode != NetworkAdaptor.NetworkMode.Off && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost && GetComponent<BulletScript>())
        {
            GetComponent<BulletScript>().InstantiateHitPrefab();
        }
    }
    
    [ClientRpc]
    private void SetPositionClientRpc(Vector3 position)
    {
        transform.position = position; 
    }
    
    void Update()
    {
        if (NetworkAdaptor.mode != NetworkAdaptor.NetworkMode.Off && NetworkManager.Singleton && NetworkManager.IsServer)
        {
            SetPositionClientRpc(transform.position);
        }
    }
}
