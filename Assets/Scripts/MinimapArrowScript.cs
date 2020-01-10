using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapArrowScript : MonoBehaviour {

	public Camera minicam;
	PlayerCore player;
	static MinimapArrowScript instance;
	public GameObject arrowPrefab;

	Dictionary<TaskManager.ObjectiveLocation, Transform> arrows = new Dictionary<TaskManager.ObjectiveLocation, Transform>();
	Transform playerTargetArrow;
	public void Initialize(PlayerCore player) {
		this.player = player;
		instance = this;
		playerTargetArrow = Instantiate(arrowPrefab, transform, false).transform;
		playerTargetArrow.GetComponent<SpriteRenderer>().color = Color.cyan;
	}

	// Draw arrows signifying objective locations. Do not constantly call this method.
	public static void DrawObjectiveLocations()
	{
		if(instance)
		{
			// clear the dictionary, then recreate the arrows
			foreach(var rectTransform in instance.arrows.Values)
			{
				if(rectTransform && rectTransform.gameObject) Destroy(rectTransform.gameObject);
			}
			instance.arrows.Clear();

			foreach(var loc in TaskManager.objectiveLocations)
			{
				var arrow = Instantiate(instance.arrowPrefab, instance.transform, false);
				instance.arrows.Add(loc, arrow.GetComponent<Transform>());
				
			}
		}
	}

	bool UpdatePosition(Transform arrow, Vector2 realPos)
	{
		// arrow is in minimap space, and it is directly captured into the minimap via the camera render
		// therefore to display the arrow all that is necessary is a translation of world position into the minimap camera's
		// viewport position, which is done here

		var pos = minicam.WorldToViewportPoint(realPos);
		Vector3 arrowpos = new Vector3(0,0,0);

		// demarcates whether the position is off the minimap screent
		bool xlim = false;
		bool ylim = false;

		// viewport coordinates have their left and right edges at 0 and 1 respectively, beyond that is outside the viewport
		// if it is outside the viewport we need to adjust the arrow's rotation and position it on the edge, which is being done here
		// if not, the original position actually fits in the viewport so we can just use that
		if(pos.x > 1) {
			arrowpos.x = minicam.ViewportToWorldPoint(new Vector3(1,0,0)).x;
			arrow.transform.eulerAngles = new Vector3(0,0,-90);
			xlim = true;
		}
		else if(pos.x < 0) {
			arrowpos.x = minicam.ViewportToWorldPoint(new Vector3(0,0,0)).x;
			arrow.transform.eulerAngles = new Vector3(0,0,90);
			xlim = true;
		} else arrowpos.x = realPos.x;
		if(pos.y > 1) {
			arrowpos.y = minicam.ViewportToWorldPoint(new Vector3(0,1,0)).y;
			arrow.transform.eulerAngles = new Vector3(0,0,0);
			ylim = true;
		}
		else if(pos.y < 0) {
			arrowpos.y = minicam.ViewportToWorldPoint(new Vector3(0,0,0)).y;
			arrow.transform.eulerAngles = new Vector3(0,0,180);
			ylim = true;
		} else arrowpos.y = realPos.y;
		
		// set the arrow's position, return whether a viewport limit was reached (player target marker uses this)
		arrow.transform.position = arrowpos;
		return (xlim || ylim);
	}

	void Update() {
		if(playerTargetArrow && player) {
			if(player.GetTargetingSystem().GetTarget()) { 
				playerTargetArrow.GetComponent<SpriteRenderer>().enabled = 
					UpdatePosition(playerTargetArrow, player.GetTargetingSystem().GetTarget().position);
			}
			else
			{
				playerTargetArrow.GetComponent<SpriteRenderer>().enabled = false;
			}
		}
		else
		{
			playerTargetArrow.GetComponent<SpriteRenderer>().enabled = false;
		}
	}
}
