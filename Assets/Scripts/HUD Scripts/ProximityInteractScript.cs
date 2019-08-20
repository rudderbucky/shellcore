using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityInteractScript : MonoBehaviour {
	public PlayerCore player;
	public RectTransform interactIndicator;
	public VendorUI vendorUI;
	Entity closest;
	Entity lastEnt;
	static ProximityInteractScript instance;

	void Awake() {
		instance = this;
	}
	void ActivateInteraction(Entity ent) {
        // TODO: perhaps make this a static ability so reticle scripts can use it as well
        if (TaskManager.interactionOverrides.ContainsKey(ent.name))
        {
            TaskManager.interactionOverrides[ent.name].Invoke();
        }
        else
            DialogueSystem.StartDialogue(ent.dialogue, ent, player);
	}
	void Update() {
		if(player != null) 
		{
			closest = null; // get the closest entity
			foreach(Entity ent in AIData.entities) {
				// ignore non vendors or non yards
				// TODO: Fix this so it doesn't work for specific entities in other factions (eg outposts/bunkers)
				// instead of the blanket faction fix that is employed currently
				if(ent == player || ent.dialogue == null || ent.faction != player.faction || ent.GetIsDead() || ent == null) continue;
				if(closest == null) closest = ent;
				else if((ent.transform.position - player.transform.position).sqrMagnitude <= 
					(closest.transform.position - player.transform.position).sqrMagnitude) 
					{
						closest = ent;
					}
			}

			if(closest as IVendor != null) {
				var blueprint = (closest as IVendor).GetVendingBlueprint();
				var range = blueprint.range;

				if((closest.transform.position - player.transform.position).sqrMagnitude <= range)
					for (int i = 0; i < blueprint.items.Count; i++)
					{
						if(Input.GetKey(KeyCode.LeftShift)) {
							if(Input.GetKeyDown((1 + i).ToString())) 
							{
                    			vendorUI.SetVendor(closest as IVendor, player);
								vendorUI.onButtonPressed(i);
							}
						}
					}
			}
		}
	}

	public static void Focus() {
		instance.focus();
	}
	void focus() {
		if(player != null) {
			if(player.GetIsInteracting() || closest == null || (closest.transform.position - player.transform.position).sqrMagnitude >= 100) 
			{
				interactIndicator.localScale = new Vector3(1,0,1);
				interactIndicator.gameObject.SetActive(false);
				return;
			}
			else 
			{ 
				// interact indicator image and animation
				interactIndicator.gameObject.SetActive(true);
				var y = interactIndicator.localScale.y;
				if(lastEnt != closest) {
					lastEnt = closest;
					interactIndicator.localScale = new Vector3(1,0,1);	
				}
				if(y < 1) {
					interactIndicator.localScale = new Vector3(1, Mathf.Min(1, y + 0.1F), 1);
				}
				interactIndicator.anchoredPosition = Camera.main.WorldToScreenPoint(closest.transform.position) + new Vector3(0, 50);
				if(Input.GetKeyUp(KeyCode.Q)) 
				{
					ActivateInteraction(closest); // key received; activate interaction
				}
			}
		}
	}
}
