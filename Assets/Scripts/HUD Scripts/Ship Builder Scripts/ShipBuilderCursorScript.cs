using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;


public class ShipBuilderCursorScript : MonoBehaviour {

	public List<ShipBuilderPart> parts = new List<ShipBuilderPart>();
	public Canvas canvas;
	public RectTransform grid;
	ShipBuilderPart currentPart;
	ShipBuilderPart lastPart;
	public ShipBuilder builder;
	public InputField field;
	public InputField jsonField;
	bool flipped;
	public AbilityHandler handler;
	public PlayerCore player;
	List<Ability> currentAbilities;

	void OnEnable() {
		currentAbilities = new List<Ability>();
	}
	public EntityBlueprint.PartInfo? GetCurrentInfo() {
		if(!currentPart) return null;
		return currentPart.info;
	}

	public EntityBlueprint.PartInfo? GetLastInfo() {
		if(!lastPart) return null;
		return lastPart.info;
	}
	public void GrabPart(ShipBuilderPart part) {
		lastPart = null;
		if(parts.Contains(part)) {
			parts.Remove(part);
			parts.Add(part);
			part.rectTransform.SetAsLastSibling();
		}
		currentPart = part;
	}
	void PlaceCurrentPart() {
		if(!RectTransformUtility.RectangleContainsScreenPoint(grid, Input.mousePosition)) {
			builder.DispatchPart(currentPart);
		} else {
			lastPart = currentPart;
			currentPart = null;
			if(lastPart.isInChain && lastPart.validPos) {
				lastPart.SetLastValidPos(lastPart.info.location);
			} else lastPart.Snapback();
		}
		UpdateHandler();
	}

	public void UpdateHandler() {
		currentAbilities.Clear();
		foreach(Ability ab in gameObject.GetComponentsInChildren<Ability>()) {
			Destroy(ab);
		}
		foreach(ShipBuilderPart part in parts) {
			if(part.info.abilityID != 0) {
				Ability dispAb = AbilityUtilities.AddAbilityToGameObjectByID(gameObject, part.info.abilityID, 
					part.info.secondaryData, part.info.tier);
				currentAbilities.Insert(0, dispAb);
			}
		}
		currentAbilities.Insert(0, gameObject.AddComponent<MainBullet>());
		if(handler) 
		{
			handler.Deinitialize();
			handler.Initialize(player, currentAbilities.ToArray());
		}
	}
	public EntityBlueprint.PartInfo? GetPartCursorIsOn() {
		foreach(ShipBuilderPart part in parts) {
			if(RectTransformUtility.RectangleContainsScreenPoint(part.rectTransform, Input.mousePosition)) {
				return part.info;
			}
		}
		return null;
	}
	public void ClearAllParts() {
		while(parts.Count > 0) {
			builder.DispatchPart(parts[0]);
		}
		UpdateHandler();
	}
	public bool rotateMode;
	public void RotateLastPart() {
		var x = Input.mousePosition - lastPart.transform.position;
		var y = new Vector3(0,0,(Mathf.Rad2Deg * Mathf.Atan(x.y/x.x) -(x.x >= 0 ? 90 : -90)));
		if(!float.IsNaN(y.z))
		{
			y.z = 15 * (Mathf.RoundToInt(y.z / 15));
			lastPart.info.rotation = y.z;
		}
		else lastPart.info.rotation = 0;
			return;
	}
	public void FlipLastPart() {
		lastPart.info.mirrored = !lastPart.info.mirrored;
		flipped = true;
	}
	void Update() {
		builder.UpdateChain();
		if(Input.GetKeyDown("c") && !field.isFocused && !jsonField.isFocused) {
			ClearAllParts();
		}
		transform.position = new Vector3(5 * ((int)Input.mousePosition.x / 5), 5 * ((int)Input.mousePosition.y / 5), 0);
		if(rotateMode) {
			RotateLastPart();
			return;
		}
		if(flipped) {
			flipped = false;
			return;
		}
		if(currentPart) {
			currentPart.info.location = GetComponent<RectTransform>().anchoredPosition / 100;
			if(Input.GetMouseButtonUp(0)) {
				PlaceCurrentPart();
			}
		} else if(Input.GetMouseButtonDown(0)) {
			for(int i = parts.Count - 1; i >= 0; i--) {
				Bounds bound = ShipBuilder.GetRect(parts[i].rectTransform);
				bound.extents /= 1.5F;
				var origPos = transform.position;
				transform.position = Input.mousePosition;
				if(bound.Contains(GetComponent<RectTransform>().anchoredPosition)) {
					GrabPart(parts[i]);
					break;
				}
				transform.position = origPos;
			}
		}
		foreach(ShipBuilderPart part in parts) {
			if(part == currentPart || part == lastPart) part.highlighted = true;
			else part.highlighted = false;
		}
	}
}

