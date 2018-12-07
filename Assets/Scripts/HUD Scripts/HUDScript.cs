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
    public void InitializeHUD(PlayerCore player) {
        // Initialize all HUD elements
        GetComponentInChildren<HealthBarScript>().Initialize(player);
        GetComponentInChildren<AbilityHandler>().Initialize(player);
        GetComponentInChildren<ReticleScript>().Initialize(player);
        GetComponentInChildren<QuantityDisplayScript>().Initialize(player);
        Camera.main.GetComponent<CameraScript>().Initialize(player);
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
