using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
	This class exists to streamline the process of displaying an image representation of a part, and storing actual data.
	In other words, this class is made to reflect the current status of the embedded PartInfo in image form.
 */
public class ShipBuilderPart : DisplayPart {

	public RectTransform rectTransform;
	public ShipBuilderCursorScript cursorScript;
	public bool isInChain;
	public bool validPos;
	public bool highlighted;
	public BuilderMode mode;
	private Vector3? lastValidPos = null;

	public void SetLastValidPos(Vector3? lastPos) {
		lastValidPos = lastPos;
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
		if(highlighted) image.color = (isInChain && validPos ? Color.white : Color.white - new Color(0,0,0,0.5F));
		else image.color = (isInChain && validPos ? FactionColors.colors[0] : FactionColors.colors[0] - new Color(0,0,0,0.5F));
	}

	bool IsTooClose(ShipBuilderPart otherPart) {
		var rect1 = ShipBuilder.GetRect(rectTransform);
		var rect2 = ShipBuilder.GetRect(otherPart.rectTransform);
		rect1.Expand(-0.995F * rect1.extents);
		rect2.Expand(-0.995F * rect2.extents);
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
}
