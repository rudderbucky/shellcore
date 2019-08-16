using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Master HUD script used to initialize individual HUD elements
/// </summary>
public class HUDScript : MonoBehaviour {

    private bool initialized;
    private void InitializeConstantHUD(PlayerCore player) {
        Camera.main.GetComponent<CameraScript>().Initialize(player);
        GetComponentInChildren<ReticleScript>().Initialize(player);
        GetComponentInChildren<QuantityDisplayScript>().Initialize(player);
        GetComponentInChildren<InfoText>().player = player.transform;
        InfoText[] infos = GetComponentsInChildren<InfoText>();
        player.alerter = infos[0];
        GetComponentInChildren<ProximityInteractScript>().player = player;        
        if(GetComponentInChildren<HUDArrowScript>()) GetComponentInChildren<HUDArrowScript>().Initialize(player);
        GetComponentInChildren<MinimapArrowScript>().Initialize(player);
    }
    /// <summary>
    /// Initializes the HUD
    /// </summary>
    public void InitializeHUD(PlayerCore player) {
        // Initialize all HUD elements
        GetComponentInChildren<HealthBarScript>().Initialize(player);
        GetComponentInChildren<AbilityHandler>().Initialize(player);
        if(!initialized) InitializeConstantHUD(player);
        Camera.main.GetComponent<CameraScript>().Focus();
        initialized = true;
        // GetComponentInChildren<FadeUIScript>().Initialize(player); temporarily removed due to implementation difficulties
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
