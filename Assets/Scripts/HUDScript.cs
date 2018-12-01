using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Master HUD script used to initialize individual HUD elements
/// </summary>
public class HUDScript : MonoBehaviour {

    /// <summary>
    /// Initializes the HUD
    /// </summary>
    public void InitializeHUD() {
        // Initialize all HUD elements
        GetComponentInChildren<HealthBarScript>().Initialize();
        GetComponentInChildren<AbilityHandler>().Initialize();
        GetComponentInChildren<ReticleScript>().Initialize();
        Camera.main.GetComponent<CameraScript>().Start();
    }

    /// <summary>
    /// Deiniitializes the HUD
    /// </summary>
    public void DeinitializeHUD() {
        // Deinitialize all deinitializable HUD elements
        GetComponentInChildren<HealthBarScript>().Deinitialize();
        GetComponentInChildren<AbilityHandler>().Deinitialize();
    }
}
