using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityInteractScript : MonoBehaviour {
	public PlayerCore player;

	void ActivateInteraction(Entity ent) { 
		// TODO: perhaps make this a static ability so reticle scripts can use it as well
		DialogueSystem.StartDialogue(ent.dialogue, ent, player);
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

			if(player.GetIsInteracting() || closest == null || (closest.transform.position - player.transform.position).sqrMagnitude >= 200) 
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
		}
	}
}
