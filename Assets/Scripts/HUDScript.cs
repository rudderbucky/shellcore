using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDScript : MonoBehaviour {

    public void InitializeHUD() {
        GetComponentInChildren<HealthBarScript>().Initialize();
        GetComponentInChildren<AbilityHandler>().Initialize();
        GetComponentInChildren<ReticleScript>().Initialize();
        Camera.main.GetComponent<CameraScript>().Start();
    }

    public void DeinitializeHUD() {
        GetComponentInChildren<HealthBarScript>().Deinitialize();
    }
}
