using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;

public interface IBuilderInterface {
	BuilderMode GetMode();
	void DispatchPart(ShipBuilderPart part, ShipBuilder.TransferMode mode);
	void UpdateChain();
	EntityBlueprint.PartInfo? GetButtonPartCursorIsOn();
	void SetSearcherString(string text);
	bool CheckPartIntersectsWithShell(ShipBuilderPart shipPart);
}

public class ShipBuilderCursorScript : MonoBehaviour, IShipStatsDatabase {

	public List<ShipBuilderPart> parts = new List<ShipBuilderPart>();
	public Canvas canvas;
	public RectTransform grid;
	ShipBuilderPart currentPart;
	ShipBuilderPart lastPart;
	public IBuilderInterface builder;
	public InputField searchField;
	public InputField jsonField;
	bool flipped;
	public AbilityHandler handler;
	public PlayerCore player;
	List<Ability> currentAbilities;
	public int buildValue;
	public int buildCost;
	public RectTransform playerInventory;
	public RectTransform traderInventory;
	public BuilderMode cursorMode = BuilderMode.Yard;
	private Vector2 offset;

	public void SetMode(BuilderMode mode) {
		cursorMode = mode;
	}
	public void SetBuilder(IBuilderInterface builder) {
		this.builder = builder;
	}

	void OnEnable() {
		buildCost = 0;
		currentAbilities = new List<Ability>();

		buildValue = 0;
		foreach(ShipBuilderPart part in parts) {
			buildValue += EntityBlueprint.GetPartValue(part.info);
		}
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
		offset = part.GetComponent<RectTransform>().anchoredPosition - GetComponent<RectTransform>().anchoredPosition;
		currentPart = part;
	}
	void PlaceCurrentPart() {
		var editorMode = (builder as ShipBuilder) != null && !(builder as ShipBuilder).Equals(null) && (builder as ShipBuilder).editorMode;
		if(cursorMode != BuilderMode.Workshop)
			if(traderInventory.gameObject.activeSelf && (!editorMode || !Input.GetKey(KeyCode.LeftControl)) &&
				RectTransformUtility.RectangleContainsScreenPoint(traderInventory, Input.mousePosition)) {
				builder.DispatchPart(currentPart, (currentPart.mode == BuilderMode.Yard 
					? ShipBuilder.TransferMode.Sell : ShipBuilder.TransferMode.Return));
			}
			else if((!editorMode || !Input.GetKey(KeyCode.LeftControl)) && 
				RectTransformUtility.RectangleContainsScreenPoint(playerInventory, Input.mousePosition)) {
				builder.DispatchPart(currentPart, (currentPart.mode == BuilderMode.Yard 
					? ShipBuilder.TransferMode.Return : ShipBuilder.TransferMode.Buy));
			}
			else if (!RectTransformUtility.RectangleContainsScreenPoint(grid, Input.mousePosition) && !editorMode) {
				builder.DispatchPart(currentPart, ShipBuilder.TransferMode.Return);
			} 
			else if(builder.CheckPartIntersectsWithShell(currentPart) && currentPart.GetLastValidPos() == null) {
				builder.DispatchPart(currentPart, ShipBuilder.TransferMode.Return);
			}
			else PlaceCurrentPartInGrid();
		else {
			if(RectTransformUtility.RectangleContainsScreenPoint(playerInventory, Input.mousePosition)) {
				builder.DispatchPart(currentPart, ShipBuilder.TransferMode.Return);
			} else PlaceCurrentPartInGrid();
		}
		UpdateHandler();
	}

	private void PlaceCurrentPartInGrid() {
		lastPart = currentPart;
		currentPart = null;
		if(lastPart.isInChain && lastPart.validPos) {
			lastPart.SetLastValidPos(lastPart.info.location);
		} 
		else if(Input.GetKey(KeyCode.LeftShift) || builder.CheckPartIntersectsWithShell(lastPart) )
		{				
			lastPart.Snapback();
		} 
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

		if(builder as ShipBuilder)
		{
			return (builder as ShipBuilder).RequestInventoryMouseOverInfo();
		}
		return null;
	}
	public void ClearAllParts() {
		while(parts.Count > 0) {
			builder.DispatchPart(parts[0], ShipBuilder.TransferMode.Return);
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
		int baseMoveSize = cursorMode == BuilderMode.Yard ? 4 : 5;
		builder.UpdateChain();
		if(Input.GetKeyDown(KeyCode.C) && (!searchField.isFocused && !jsonField.isFocused && !WCWorldIO.active)) {
			if(builder as ShipBuilder == null || (builder as ShipBuilder).Equals(null) || !(builder as ShipBuilder).editorMode) ClearAllParts();
		}
		System.Func<Vector3, int, int, Vector3> roundToRatios = (x, y, z) => new Vector3(y * ((int)x.x / (int)y), z * ((int)x.y / (int)z), 0);
		var newOffset = roundToRatios(grid.position, baseMoveSize, baseMoveSize) -grid.position;
		transform.position = roundToRatios(Input.mousePosition, baseMoveSize, baseMoveSize) - newOffset;
		var oldPos = GetComponent<RectTransform>().anchoredPosition;
		GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Round(oldPos.x / 10) * 10, Mathf.Round(oldPos.y / 10) * 10);
		// round to nearest 0.1
		// TODO: Make this stuff less messy. Regardless, consistency achieved!
		if(rotateMode) {
			RotateLastPart();
			return;
		}
		if(flipped) {
			flipped = false;
			return;
		}
		if(currentPart) {
			currentPart.info.location = (GetComponent<RectTransform>().anchoredPosition + offset) / 100;
			if(Input.GetMouseButtonUp(0)) {
				PlaceCurrentPart();
			}
		} else if(Input.GetMouseButtonDown(0)) {
			lastPart = null;
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
	}

    public List<DisplayPart> GetParts()
    {
        return parts.ConvertAll(x => x as DisplayPart);
    }

    public BuilderMode GetMode()
    {
        return cursorMode;
    }

    public int GetBuildValue()
    {
        return buildValue;
    }

    public int GetBuildCost()
    {
        return buildCost;
    }
}

