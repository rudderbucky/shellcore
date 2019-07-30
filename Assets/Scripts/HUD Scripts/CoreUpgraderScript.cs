using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CoreUpgraderScript : GUIWindowScripts
{
    public PlayerCore player;
    public static CoreUpgraderScript instance;
    private static int maxAbilityCap = 15;
    private static int minAbilityCap = 10;
    public GameObject optionPrefab;
    public RectTransform reputationBar;
    public Text repText;
    public Transform optionHolder;

    public void initialize() {
        instance = this;
        player.SetIsInteracting(true);

        // reputation display
        var repReq = GetReputationRequirement(player.blueprint.coreShellSpriteID);
        reputationBar.sizeDelta = new Vector2(player.reputation * 800 / repReq, 30);
        repText.text = "Reputation: " + player.reputation + "/" + repReq;

        // create option icons
        var nextIDs = GetNextUpgrades(player.blueprint.coreShellSpriteID);
        var offset = -500 * (nextIDs.Length - 1);
        for(int i = 0; i < nextIDs.Length; i++) {
            var option = Instantiate(optionPrefab, optionHolder, false).GetComponent<RectTransform>();
            option.anchoredPosition = new Vector2(offset + offset * 2 * i / nextIDs.Length, 20);
            var shell = option.GetComponentsInChildren<Image>()[1];
            shell.sprite = ResourceManager.GetAsset<Sprite>(nextIDs[i]);
            shell.SetNativeSize();
            shell.rectTransform.anchoredPosition = -shell.sprite.pivot + shell.rectTransform.sizeDelta / 2;
        }

        Activate();
        gameObject.SetActive(true);
        ShardCountScript.StickySlideIn(player.shards);
        CUAbilityCapDisplay.Initialize(player.abilityCaps);
    }

    public override void CloseUI() {
        ShardCountScript.StickySlideOut();
        player.SetIsInteracting(false);
        gameObject.SetActive(false);
        ResourceManager.PlayClipByID("clip_back");
    }	
    
    /// prevent dragging the window if the mouse is on the grid
	public override void OnPointerDown(PointerEventData eventData) {
		base.OnPointerDown(eventData);
	}

    public static void IncrementAbilityCap(int type) {
        instance.incrementAbilityCap(type);
    } 

    private void incrementAbilityCap(int type) {
        if(player.abilityCaps[type] < maxAbilityCap) {
            player.shards -= GetUpgradeCost(type);
            player.abilityCaps[type]++;
            ShardCountScript.UpdateNumber(player.shards);
        }

    }

    public static int GetUpgradeCost(int type) {
        return 5 * Mathf.RoundToInt(Mathf.Pow(2, instance.player.abilityCaps[type] - minAbilityCap));
    }

    public static int GetShards() {
        return instance.player.shards;
    }

    public static string[] GetNextUpgrades(string coreName) {
        switch(coreName) {
            case "core1_shell":
                return new string[] {"core2_shell"};
            case "core2_shell":
                return new string[] {"core3skills_shell", "core3weapons_shell"};
            case "core3skills_shell":
                return new string[] {"core4commando_shell", "core4elite_shell"};
            case "core3weapons_shell":
                return new string[] {"core4captain_shell", "core4admiral_shell"};
            default:
                return null;
        }
    }

    public static int GetReputationRequirement(string coreName) {
        switch(coreName) {
            case "core1_shell":
                return 100;
            case "core2_shell":
                return 500;
            case "core3skills_shell":
            case "core3weapons_shell":
                return 2500;
            default:
                return 0;
        }
    }

    public static int GetPartTierLimit(string coreName) {
        switch(coreName) {
            case "core1_shell":
                return 0;
            case "core2_shell":
                return 1;
            case "core3skills_shell":
            case "core3weapons_shell":
            case "core4commando_shell":
            case "core4elite_shell":
            case "core4captain_shell":
            case "core4admiral_shell":
                return 2;
            default:
                return 0;
        }
    }
}
