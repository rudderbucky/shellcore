using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapArrowScript : MonoBehaviour {

	public Camera minicam;
	PlayerCore player;
	public static MinimapArrowScript instance;
	public GameObject arrowPrefab;

	Dictionary<TaskManager.ObjectiveLocation, Transform> arrows = new Dictionary<TaskManager.ObjectiveLocation, Transform>();
	Dictionary<ShellCore, Transform> coreArrows = new Dictionary<ShellCore, Transform>();
	Transform playerTargetArrow;
	public void Initialize(PlayerCore player) {
		this.player = player;
		instance = this;
		if(!playerTargetArrow) playerTargetArrow = Instantiate(arrowPrefab, transform, false).transform;
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

			foreach(var ls in TaskManager.objectiveLocations.Values)
			{
				foreach(var loc in ls)
				{
					var arrow = Instantiate(instance.arrowPrefab, instance.transform, false);
					arrow.GetComponent<SpriteRenderer>().color = Color.red + Color.green / 2; // orange
					arrow.GetComponent<SpriteRenderer>().enabled = true;
					instance.arrows.Add(loc, arrow.transform);
					instance.UpdatePosition(arrow.transform, loc.location);
				}
			}
		}
	}

	bool UpdatePosition(Transform arrow, Vector2 realPos, bool hideIfOffViewport = false)
	{
		// we initially just set the sprite renderer active
		arrow.gameObject.GetComponent<SpriteRenderer>().enabled = true;

		// arrow is in minimap space, and it is directly captured into the minimap via the camera render
		// therefore to display the arrow all that is necessary is a translation of world position into the minimap camera's
		// viewport position, which is done here

		var pos = minicam.WorldToViewportPoint(realPos);
		Vector3 arrowpos = new Vector3(0,0,0);

		// demarcates whether the position is off the minimap screent
		bool xlim = false;
		bool ylim = false;

		// revert the arrow to default rotation if neither xlim nor ylim was marked is true
		arrow.transform.eulerAngles = new Vector3(0,0,180);

		// we hide the arrow if it is off the minimap in specific cases, like enemies in BattleZones
		// viewport coordinates have their left and right edges at 0 and 1 respectively, beyond that is outside the viewport
		// if it is outside the viewport we need to adjust the arrow's rotation and position it on the edge, which is being done here
		// if not, the original position actually fits in the viewport so we can just use that
		if(pos.x > 1) {
			arrow.gameObject.GetComponent<SpriteRenderer>().enabled = !hideIfOffViewport;
			arrowpos.x = minicam.ViewportToWorldPoint(new Vector3(1,0,0)).x;
			arrow.transform.eulerAngles = new Vector3(0,0,-90);
			xlim = true;
		}
		else if(pos.x < 0) {
			arrow.gameObject.GetComponent<SpriteRenderer>().enabled = !hideIfOffViewport;
			arrowpos.x = minicam.ViewportToWorldPoint(new Vector3(0,0,0)).x;
			arrow.transform.eulerAngles = new Vector3(0,0,90);
			xlim = true;
		} else arrowpos.x = realPos.x;
		if(pos.y > 1) {
			arrow.gameObject.GetComponent<SpriteRenderer>().enabled = !hideIfOffViewport;
			arrowpos.y = minicam.ViewportToWorldPoint(new Vector3(0,1,0)).y;
			arrow.transform.eulerAngles = new Vector3(0,0,0);
			ylim = true;
		}
		else if(pos.y < 0) {
			arrow.gameObject.GetComponent<SpriteRenderer>().enabled = !hideIfOffViewport;
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

		foreach(var loc in arrows.Keys)
		{
			UpdatePosition(arrows[loc], loc.location);
		}

		foreach(var core in coreArrows.Keys)
		{
			if(!core)
			{
				coreArrows[core].GetComponent<SpriteRenderer>().enabled = false;
				continue;
			} 
			UpdatePosition(coreArrows[core], core.transform.position, core.faction != PlayerCore.Instance.faction);
			if(core.faction != player.faction && coreArrows[core].GetComponent<SpriteRenderer>().enabled)
			{
				coreArrows[core].GetComponent<SpriteRenderer>().enabled = !core.IsInvisible;
			}
		}
	}

	public void ClearCoreArrows()
	{
		foreach(var kvp in coreArrows)
		{
			Destroy(kvp.Value.gameObject);
		}

		coreArrows.Clear();
	}

	public void AddCoreArrow(ShellCore core)
	{
		if(coreArrows.ContainsKey(core)) 
			return;

		coreArrows.Add(core, Instantiate(arrowPrefab, transform, false).transform);
		coreArrows[core].GetComponent<SpriteRenderer>().color = FactionManager.GetFactionColor(core.faction);
		UpdatePosition(coreArrows[core], core.transform.position, core.faction != PlayerCore.Instance.faction);
	}
}
