using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IInteractable
{
    void Interact();
    bool GetInteractible();
    Transform GetTransform();
}

public class ProximityInteractScript : MonoBehaviour
{
    public PlayerCore player;
    public RectTransform interactIndicator;
    public VendorUI vendorUI;
    IInteractable closest;
    IInteractable lastInteractable;
    public static ProximityInteractScript instance;
    public static Dictionary<ShellCore, RectTransform> playerNames;
    public GameObject playerNamePrefab;

    void Awake()
    {
        instance = this;
        if (playerNames == null)
            playerNames = new Dictionary<ShellCore, RectTransform>();
    }

    public static void ActivateInteraction(IInteractable interactable)
    {
        interactable.Interact();
    }

    public void AddPlayerName(ShellCore core, string playerName)
    {
        if (playerNames.ContainsKey(core)) return;
        var transform = Instantiate(playerNamePrefab, interactIndicator.parent).GetComponent<RectTransform>();
        transform.GetComponentInChildren<Text>().text = playerName;
        HUDScript.AddScore(playerName, 0);
        playerNames.Add(core, transform);
    }

    public void RemovePlayerName(Entity ent)
    {
        if (ent is ShellCore core && playerNames.ContainsKey(core)) 
        {
            Destroy(playerNames[core].gameObject);
            playerNames.Remove(core);
        }
    }


    void Update()
    {
        if (player == null) return;
        closest = ProximityManager.GetClosestInteractable(player);

        if (!(closest is IVendor vendor)) return;
        var blueprint = vendor.GetVendingBlueprint();
        var range = blueprint.range;

        if (player.GetIsDead() || (closest.GetTransform().position - player.transform.position).sqrMagnitude > range) return;
        for (int i = 0; i < blueprint.items.Count; i++)
        {
            if (!InputManager.GetKey(KeyName.AutoCastBuyTurret)) continue;
            if (!Input.GetKeyDown((1 + i).ToString())) continue;
            vendorUI.SetVendor(vendor, player);
            vendorUI.onButtonPressed(i);
        }
    }

    public static void Focus()
    {
        instance.focus();
    }

    void focus()
    {
        foreach (var core in playerNames.Keys)
        {
            if (!core) continue;
            playerNames[core].gameObject.SetActive(!core.GetIsDead() && (!PlayerCore.Instance || (core.StealthStacks == 0 || FactionManager.IsAllied(PlayerCore.Instance.faction, core.faction))));
            if (!playerNames[core].gameObject.activeSelf) continue;
            var worldToScreenPoint = Camera.main.WorldToScreenPoint(core.GetTransform().position);
            worldToScreenPoint.x *= UIScalerScript.GetScale();
            worldToScreenPoint.y *= UIScalerScript.GetScale();
            playerNames[core].anchoredPosition = 
                worldToScreenPoint + new Vector3(0, 50);
        }

        if (Time.timeScale == 0)
        {
            return;
        }

        if (player == null) return;
        if (player.GetIsInteracting() || closest == null || closest.Equals(null) || (closest.GetTransform().position - player.transform.position).sqrMagnitude >= 100)
        {
            interactIndicator.localScale = new Vector3(1, 0, 1);
            interactIndicator.gameObject.SetActive(false);
            return;
        }
        else
        {
            // interact indicator image and animation
            interactIndicator.gameObject.SetActive(true);
            var q = InputManager.GetKeyCode(KeyName.Interact) == KeyCode.Q;
            interactIndicator.GetComponentsInChildren<Text>(true)[1].gameObject.SetActive(q);
            interactIndicator.GetComponentsInChildren<Text>(true)[0].GetComponent<RectTransform>().anchoredPosition = q ? new Vector2(20, -1) : Vector2.down;

            var y = interactIndicator.localScale.y;
            if (lastInteractable != closest)
            {
                lastInteractable = closest;
                interactIndicator.localScale = new Vector3(1, 0, 1);
            }

            if (y < 1)
            {
                interactIndicator.localScale = new Vector3(1, Mathf.Min(1, y + 0.1F), 1);
            }

            var worldToScreenPoint = Camera.main.WorldToScreenPoint(closest.GetTransform().position);
            worldToScreenPoint.x *= UIScalerScript.GetScale();
            worldToScreenPoint.y *= UIScalerScript.GetScale();
            interactIndicator.anchoredPosition = 
                worldToScreenPoint + new Vector3(0, 50);
            if (InputManager.GetKeyUp(KeyName.Interact) && !PlayerViewScript.GetIsWindowActive() && (!PlayerCore.Instance || !PlayerCore.Instance.GetIsDead()))
            {
                ActivateInteraction(closest); // key received; activate interaction
            }
        }
    }
}
