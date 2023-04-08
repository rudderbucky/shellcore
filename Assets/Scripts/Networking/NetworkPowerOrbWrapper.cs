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


    public override void OnNetworkSpawn()
    {
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client)
        {
            GetComponent<EnergySphereScript>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    [ClientRpc]
    public void SetPositionClientRpc(Vector3 position)
    {
        transform.position = position; 
        GetComponent<EnergySphereScript>().enabled = true;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<EnergySphereScript>().Initialize();
    }
    
    void Update()
    {
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && NetworkManager.Singleton && NetworkManager.IsServer)
        {
            SetPositionClientRpc(transform.position);
        }
    }
}
