using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
	This class exists to streamline the process of displaying an image representation of a part, and storing actual data.
	In other words, this class is made to reflect the current status of the embedded PartInfo in image form.
 */
public class ShipBuilderPart : MonoBehaviour {

	Image image;
	Image shooter;
	public RectTransform rectTransform;
	public EntityBlueprint.PartInfo info;
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
	void Awake() {
		validPos = true;
		image = GetComponent<Image>();
		GameObject shooterObj = new GameObject("shooter");
		shooterObj.transform.SetParent(transform.parent);
		shooter = shooterObj.AddComponent<Image>();
		shooter.rectTransform.localScale = Vector3.one;
		rectTransform = image.rectTransform;
	}

	void Start() {
		image.enabled = true;
		shooter.enabled = true;
		if(AbilityUtilities.GetShooterByID(info.abilityID) != null) {
			shooter.sprite = ResourceManager.GetAsset<Sprite>(AbilityUtilities.GetShooterByID(info.abilityID));
			shooter.rectTransform.sizeDelta = shooter.sprite.bounds.size * 100;
		}
		image.rectTransform.anchoredPosition = shooter.rectTransform.anchoredPosition = info.location * 100;
		image.sprite = ResourceManager.GetAsset<Sprite>(info.partID +"_sprite");
		image.rectTransform.sizeDelta = image.sprite.bounds.size * 100;
	}
	bool IsTooClose(ShipBuilderPart otherPart) {
		bool z = Mathf.Abs(rectTransform.anchoredPosition.x - otherPart.rectTransform.anchoredPosition.x) <
		0.28F*(rectTransform.sizeDelta.x + otherPart.rectTransform.sizeDelta.x) &&
		Mathf.Abs(rectTransform.anchoredPosition.y - otherPart.rectTransform.anchoredPosition.y) <
		0.28F*(rectTransform.sizeDelta.y + otherPart.rectTransform.sizeDelta.y);
		return z;
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
		if(highlighted) image.color = (isInChain && validPos ? Color.white : Color.white - new Color(0,0,0,0.5F));
		else image.color = (isInChain && validPos ? FactionColors.colors[0] : FactionColors.colors[0] - new Color(0,0,0,0.5F));
		image.rectTransform.anchoredPosition = info.location * 100;
		if(shooter) 
		{
			shooter.color = image.color;
			shooter.gameObject.transform.SetAsLastSibling();
			shooter.rectTransform.anchoredPosition = info.location * 100;
			if(AbilityUtilities.GetShooterByID(info.abilityID) == null) {
				Destroy(shooter.gameObject);
			}
		}
		image.rectTransform.localEulerAngles = new Vector3(0,0,info.rotation);
		image.rectTransform.localScale = new Vector3(info.mirrored ? -1 : 1,1,1);
	}
}
