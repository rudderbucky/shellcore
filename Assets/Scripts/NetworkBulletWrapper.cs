using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkBulletWrapper : NetworkBehaviour
{
    public override void OnNetworkDespawn()
    {
        if (DevConsoleScript.networkEnabled && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            GetComponent<BulletScript>().InstantiateHitPrefab();
        }
    }
    
    public NetworkVariable<Vector2> partLocation;
    public NetworkVariable<ulong> clientID;
    [ClientRpc]
    private void SetPositionClientRpc(Vector3 position)
    {
        transform.position = position; 
    }
    
    void Update()
    {
        if (DevConsoleScript.networkEnabled && NetworkManager.Singleton && NetworkManager.IsServer)
        {
            SetPositionClientRpc(transform.position);
        }
    }
}
