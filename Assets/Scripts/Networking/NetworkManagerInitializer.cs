using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkManagerInitializer : MonoBehaviour
{
    public GameObject networkManagerPrefab;

    void Awake()
    {
        if (!NetworkManager.Singleton) {
            Instantiate(networkManagerPrefab);
        }
        Destroy(gameObject);
    }
}
