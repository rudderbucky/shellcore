using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityInteractScript : MonoBehaviour {
	public PlayerCore player;

	void ActivateInteraction(Entity ent) { 
		// TODO: perhaps make this a static ability so reticle scripts can use it as well
		DialogueSystem.StartDialogue(ent.dialogue, ent.transform.position, player);
	}
	void Update() {
		if(player != null) 
		{
			Entity closest = null; // get the closest entity
			foreach(Entity ent in AIData.entities) {
				// ignore non vendors or non yards
				if(ent == player || ent.dialogue == null) continue;
				if(closest == null) closest = ent;
				else if((ent.transform.position - player.transform.position).sqrMagnitude <= 
					(closest.transform.position - player.transform.position).sqrMagnitude) 
					{
						closest = ent;
					}
			}

			if(closest == null || (closest.transform.position - player.transform.position).sqrMagnitude >= 200) 
			{
				player.interactAlerter.showMessage("");
				return;
			}
			else 
			{ 
				player.interactAlerter.showMessage("Press 'Q' to interact with " + closest.entityName);
				if(Input.GetKeyUp(KeyCode.Q)) 
				{
					ActivateInteraction(closest); // key received; activate interaction
				}
			}
			
			/* // cannot interact; too far
				if(closest as IVendor != null && closest.faction == player.faction) // vendor
				{
					VendorUI outpostUI = transform.parent.Find("Dialogue").GetComponent<VendorUI>();
					outpostUI.blueprint = (closest as IVendor).GetVendingBlueprint();
					// check if the player is close based on the vendor's custom range as well
					// also this code sets the blueprint of the UI
					if ((closest.transform.position - player.transform.position).magnitude < outpostUI.blueprint.range)
					{
						player.interactAlerter.showMessage("Press 'Q' to interact with " 
							+ ((closest as Outpost) ? "Outpost" : "Bunker"));
						if(Input.GetKeyUp(KeyCode.Q)) {
							ActivateInteraction(closest); // key received; activate interaction
						}
					}
				}
				else if(closest as IShipBuilder != null && closest.faction == player.faction
				&&(closest.transform.position - player.transform.position).sqrMagnitude < 200) // ship builder
				{
					BuilderMode mode = (closest as IShipBuilder).GetBuilderMode();
					player.interactAlerter.showMessage("Press 'Q' to interact with " + (mode == BuilderMode.Yard ? "Yard" : "Trader"));
					if(Input.GetKeyUp(KeyCode.Q)) {
						ActivateInteraction(closest); // key received; activate interaction
					}
				} else player.interactAlerter.showMessage(""); */
		}
	}
}
