using UnityEngine;

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
    static ProximityInteractScript instance;

    void Awake()
    {
        instance = this;
    }

    public static void ActivateInteraction(IInteractable interactable)
    {
        interactable.Interact();
    }

    void Update()
    {
        if (player != null)
        {
            closest = ProximityManager.GetClosestInteractable(player);

            if (closest is IVendor vendor)
            {
                var blueprint = vendor.GetVendingBlueprint();
                var range = blueprint.range;

                if (!player.GetIsDead() && (closest.GetTransform().position - player.transform.position).sqrMagnitude <= range)
                {
                    for (int i = 0; i < blueprint.items.Count; i++)
                    {
                        if (InputManager.GetKey(KeyName.AutoCastBuyTurret))
                        {
                            if (Input.GetKeyDown((1 + i).ToString()))
                            {
                                vendorUI.SetVendor(vendor, player);
                                vendorUI.onButtonPressed(i);
                            }
                        }
                    }
                }
            }
        }
    }

    public static void Focus()
    {
        instance.focus();
    }

    void focus()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (player != null)
        {
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
                worldToScreenPoint.x *= (float)1920 / Screen.width;
                worldToScreenPoint.y *= (float)1920 / Screen.width;
                interactIndicator.anchoredPosition = 
                    worldToScreenPoint + new Vector3(0, 50);
                if (InputManager.GetKeyUp(KeyName.Interact) && !PlayerViewScript.GetIsWindowActive())
                {
                    ActivateInteraction(closest); // key received; activate interaction
                }
            }
        }
    }
}
