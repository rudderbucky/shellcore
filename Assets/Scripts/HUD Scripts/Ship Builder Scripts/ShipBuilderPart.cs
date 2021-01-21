using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
	This class exists to streamline the process of displaying an image representation of a part, and storing actual data.
	In other words, this class is made to reflect the current status of the embedded PartInfo in image form.
 */
public class ShipBuilderPart : DisplayPart, IPointerEnterHandler, IPointerExitHandler {

	public RectTransform rectTransform;
	public ShipBuilderCursorScript cursorScript;
	public Image boundImage;
	public bool isInChain;
	public bool validPos;
	public bool highlighted;
	public BuilderMode mode;
	private Vector3? lastValidPos = null;

	public void SetLastValidPos(Vector3? lastPos) {
		lastValidPos = lastPos;
	}

	public Vector3? GetLastValidPos() {
		return lastValidPos;
	}
	
	public void Snapback() {
		if(lastValidPos != null) info.location = (Vector3)lastValidPos;
	}

	protected override void Awake() {
		base.Awake();
		validPos = true;
		rectTransform = image.rectTransform;
	}

	public void InitializeMode(BuilderMode mode) {
		this.mode = mode;
	}

	protected override void UpdateAppearance() {
		// set colors
		base.UpdateAppearance();
		var mainColor = info.shiny ? FactionManager.GetFactionShinyColor(0) : FactionManager.GetFactionColor(0);
		if(highlighted) {
			if(isInChain && validPos) {
				image.material = ResourceManager.GetAsset<Material>("material_outline");
				image.color = mainColor;
			} else {
				image.color = mainColor - new Color(0,0,0,0.5F);
				image.material = null;
			}
		} 
		else {
			image.color = (isInChain && validPos ? mainColor : mainColor - new Color(0,0,0,0.5F));
			image.material = null;
		}
	}

	bool IsTooClose(ShipBuilderPart otherPart) {
		var closeConstant = mode == BuilderMode.Workshop ? -1.2F : -1F;
		var rect1 = ShipBuilder.GetRect(rectTransform);
		var rect2 = ShipBuilder.GetRect(otherPart.rectTransform);
		rect1.Expand(closeConstant * rect1.extents);
		rect2.Expand(closeConstant * rect2.extents);
		return rect1.Intersects(rect2);
	}
	
	void OnDestroy() {
		if(shooter) Destroy(shooter.gameObject);
	}

	void Update() {
		if(validPos) {
			foreach(ShipBuilderPart part in cursorScript.parts) {
				if(part != this && IsTooClose(part)) {
					validPos = false;
					break;
				}
			}
		} else {
			bool stillTouching = false;
			foreach(ShipBuilderPart part in cursorScript.parts) {
				if(part != this && IsTooClose(part)) {
					stillTouching = true;
					break;
				}
			}
			if(!stillTouching) validPos = true;
		}
		UpdateAppearance();
	}

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlighted = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlighted = false;
    }
}
