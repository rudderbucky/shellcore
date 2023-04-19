using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Master HUD script used to initialize individual HUD elements
/// </summary>
public class HUDScript : MonoBehaviour
{
    private bool initialized;
    public static HUDScript instance;
    public AbilityHandler abilityHandler;
    [SerializeField]
    private GameObject scoreboard;

    private void InitializeConstantHUD(PlayerCore player)
    {
        Camera.main.GetComponent<CameraScript>().Initialize(player);
        GetComponentInChildren<ReticleScript>().Initialize(player);
        GetComponentInChildren<QuantityDisplayScript>().Initialize(player);
        GetComponentInChildren<InfoText>().player = player.transform;
        InfoText[] infos = GetComponentsInChildren<InfoText>();
        player.alerter = infos[0];
        GetComponentInChildren<ProximityInteractScript>().player = player;
        GetComponentInChildren<MinimapArrowScript>().Initialize(player);

        if (scoreboard)
        {
            scoreboard.SetActive(MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off);
        }
    }

    public static void ClearScore()
    {
        scores.Clear();
        UpdateScore();
    }

    /// <summary>
    /// Initializes the HUD
    /// </summary>
    public void InitializeHUD(PlayerCore player)
    {
        // Initialize all HUD elements
        if (initialized)
        {
            DeinitializeHUD();
        }

        GetComponentInChildren<HealthBarScript>().Initialize(player);
        abilityHandler = GetComponentInChildren<AbilityHandler>();
        if (abilityHandler) abilityHandler.Initialize(player);
        if (!initialized)
        {
            InitializeConstantHUD(player);
        }

        Camera.main.GetComponent<CameraScript>().Focus(player.transform.position);
        initialized = true;
        instance = this;
    }

    public static Dictionary<string, int> scores = new Dictionary<string, int>();

    public static void AddScore(string player, int val)
    {
        if (!scores.ContainsKey(player))
        {
            SetScore(player, val);
        }
        else SetScore(player, scores[player]+val);
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && !MasterNetworkAdapter.lettingServerDecide) 
            MasterNetworkAdapter.instance.AddScoreClientRpc(player, val);
    }

    public static void SetScore(string player, int val)
    {
        if (!scores.ContainsKey(player))
        {
            scores.Add(player, val);
        }
        scores[player] = val;
        UpdateScore();
    }

    public static void RemoveScore(string player)
    {
        if (!scores.ContainsKey(player)) return;
        scores.Remove(player);
        UpdateScore();
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && !MasterNetworkAdapter.lettingServerDecide) 
            MasterNetworkAdapter.instance.RemoveScoreClientRpc(player);
    }

    private static void UpdateScore()
    {
        if (!instance) return;
        var list = scores.ToList();
        list.Sort((pair1,pair2) => -pair1.Value.CompareTo(pair2.Value));

        instance.scoreboard.GetComponentInChildren<Text>().text = "";
        StringBuilder builder = new StringBuilder();
        builder.Append("SCOREBOARD:\n");
        for(int i = 0; i < Mathf.Min(3, list.Count); i++)
        {
            builder.Append($"{(list[i].Key.Length > 12 ? list[i].Key.Substring(0,9) + "..." : list[i].Key)}: {list[i].Value}\n");
        }
        if (list.Count > 3)
        {
            builder.Append($"AND {list.Count - 3} OTHERS");
        }

        instance.scoreboard.GetComponentInChildren<Text>().text = builder.ToString();
    }

    /// <summary>
    /// Deiniitializes the HUD
    /// </summary>
    public void DeinitializeHUD()
    {
        // Deinitialize all deinitializable HUD elements
        GetComponentInChildren<HealthBarScript>().Deinitialize();
        GetComponentInChildren<AbilityHandler>().Deinitialize();
        initialized = false;
    }
}
