using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityInteractScript : MonoBehaviour {
	public PlayerCore player;

	void ActivateInteraction(Entity ent) { 
		// TODO: perhaps make this a static ability so reticle scripts can use it as well
		if(ent as IVendor != null) 
		{
			// activate vendor
			VendorUI outpostUI = transform.parent.Find("Dialogue").GetComponent<VendorUI>();
			PlayerViewScript.SetCurrentWindow(outpostUI);
			outpostUI.outpostPosition = ent.transform.position;
			outpostUI.player = player;
			outpostUI.openUI();
		}
		else if(ent as IShipBuilder != null) 
		{
			// activate ship builder
			ShipBuilder builder = transform.parent.Find("Ship Builder").GetComponent<ShipBuilder>();
			PlayerViewScript.SetCurrentWindow(builder);
			builder.gameObject.SetActive(true);
			builder.yardPosition = ent.transform.position;
			builder.Initialize();
		}
	}
	void Update() {
		if(player != null) 
		{
			Entity closest = null; // get the closest entity
			foreach(Entity ent in AIData.entities) {
				// ignore non vendors or non yards
				if(ent == player || !(ent as IVendor != null || ent as IShipBuilder != null)) continue;
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
			// cannot interact; too far
				if(closest as IVendor != null && closest.faction == player.faction) // vendor
				{
					VendorUI outpostUI = transform.parent.Find("Dialogue").GetComponent<VendorUI>();
					outpostUI.blueprint = (closest as IVendor).GetVendingBlueprint();
					// check if the player is close based on the vendor's custom range as well
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
					player.interactAlerter.showMessage("Press 'Q' to interact with Yard");
					if(Input.GetKeyUp(KeyCode.Q)) {
						ActivateInteraction(closest); // key received; activate interaction
					}
				} else player.interactAlerter.showMessage("");
		}
	}
}
