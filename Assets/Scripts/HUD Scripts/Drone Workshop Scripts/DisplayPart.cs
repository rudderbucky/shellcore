using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayPart : MonoBehaviour
{
    protected Image image;
	protected Image shooter;
	public EntityBlueprint.PartInfo info;

	private int faction = 0;

	protected virtual void Awake() {
		image = GetComponent<Image>();
		GameObject shooterObj = new GameObject("shooter");
		shooterObj.transform.SetParent(transform.parent);
		shooter = shooterObj.AddComponent<Image>();
		shooter.enabled = false;
		shooter.rectTransform.localScale = Vector3.one;
	}

	void Start() {
		if(AbilityUtilities.GetShooterByID(info.abilityID) != null) {
			shooter.sprite = ResourceManager.GetAsset<Sprite>(AbilityUtilities.GetShooterByID(info.abilityID));
			shooter.rectTransform.sizeDelta = shooter.sprite.bounds.size * 100;
			shooter.enabled = true;
			shooter.raycastTarget = false;
		}
		image.rectTransform.anchoredPosition = shooter.rectTransform.anchoredPosition = info.location * 100;
		image.sprite = ResourceManager.GetAsset<Sprite>(info.partID +"_sprite");
		// NEVER name something with "_sprite" at the end UNLESS it is a PART SPRITE!
		image.rectTransform.sizeDelta = image.sprite.bounds.size * 100;
		UpdateAppearance();
		image.enabled = true;
	}

	public virtual void UpdateFaction(int faction) {
		this.faction = faction;
	}

	protected virtual void UpdateAppearance() {
		// set colors
        image.color = info.shiny ? FactionManager.GetFactionShinyColor(faction) : FactionManager.GetFactionColor(faction);
		// set position
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
		// set rotation and flipping
		image.rectTransform.localEulerAngles = new Vector3(0,0,info.rotation);
		image.rectTransform.localScale = new Vector3(info.mirrored ? -1 : 1,1,1);
	}
}
