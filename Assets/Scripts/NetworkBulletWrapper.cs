using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkBulletWrapper : NetworkBehaviour
{

    void FindAndActivateWeaponCosmetic(ulong clientID, Vector2 location)
    {
            var ent = AIData.entities.Find(e => e.ID == clientID.ToString());
            if (!(ent is ShellCore)) return;
            var ab = NetworkProtobuf.GetWeaponFromLocation(location, ent as ShellCore);
            if (ab is Bullet bullet) bullet.ActivationCosmetic(transform.position);
    }


    public override void OnNetworkSpawn()
    {
        if (DevConsoleScript.networkEnabled && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            FindAndActivateWeaponCosmetic(clientID.Value, partLocation.Value);
        }
        base.OnNetworkSpawn();
    }

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
