using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;

public class ShipBuilderCursorScript : MonoBehaviour {

	public EntityBlueprint.PartInfo currentPart;
	public RectTransform grid;
	Image shooter;
	public Image image;
	public struct BuilderPartInfo {
		public EntityBlueprint.PartInfo part;
		public Image builderImage;
	}

	public List<BuilderPartInfo> parts = new List<BuilderPartInfo>();
	public ShipBuilder builder;

	bool justFlipped;
	public enum CursorMode {
		AddRemove,
		FlipRotate
	}

	public BuilderPartInfo lastPart;
	public CursorMode mode = CursorMode.AddRemove;
	bool rotateMode;

	public void FlipLastPart() {
		Debug.Log("flip");
		parts.Remove(lastPart);
		builder.parts.Remove(lastPart.part);
		lastPart.part.mirrored = !lastPart.part.mirrored;
		var tmp = lastPart.builderImage.rectTransform.localScale;
		tmp.x = -tmp.x;
		lastPart.builderImage.rectTransform.localScale = tmp;
		parts.Add(lastPart);
		builder.parts.Add(lastPart.part);
		justFlipped = true;
	}
	public void ToggleRotate() {
		rotateMode = !rotateMode;
	}
	void Start() {
		image = GetComponentInChildren<Image>();
		shooter = GetComponentsInChildren<Image>()[1];
		image.preserveAspect = true;
	}

	void GrabPart(BuilderPartInfo info) {
		mode = CursorMode.AddRemove;
		currentPart = info.part;
		image.sprite = info.builderImage.sprite;
		image.enabled = true;
		image.transform.rotation = info.builderImage.transform.rotation;
		var x = image.transform.localScale;
		string shooterID = ShipBuilderInventoryScript.GetShooterID(info.part.abilityType);
		if(shooterID != null) 
		{
			shooter.sprite = ResourceManager.GetAsset<Sprite>(shooterID);
			shooter.color = FactionColors.colors[0];
			shooter.enabled = true;
			shooter.rectTransform.sizeDelta = shooter.sprite.bounds.size * 100;
		} else shooter.enabled = false;
		x.x = currentPart.mirrored ? -1 : 1;
		image.transform.localScale = x;
		parts.Remove(info);
		builder.parts.Remove(info.part);
		Destroy(info.builderImage.gameObject);
	}
	// Update is called once per frame
	void Update () {
		transform.SetAsLastSibling();
		transform.position = new Vector3(10 * ((int)Input.mousePosition.x / 10), 10 * ((int)Input.mousePosition.y / 10), 0);
		currentPart.location = GetComponent<RectTransform>().anchoredPosition / 100;
		if(justFlipped && Input.GetMouseButtonDown(0)) {
			return;
		}
		if(justFlipped && Input.GetMouseButtonUp(0)) {
			justFlipped = false;
		}
		if(rotateMode) {
			var x = Input.mousePosition - lastPart.builderImage.transform.position;
			var y = new Vector3(0,0,(Mathf.Rad2Deg * Mathf.Atan(x.y/x.x) -(x.x >= 0 ? 90 : -90)));
			if(!float.IsNaN(y.z))
				{
					y.z = 15 * (Mathf.RoundToInt(y.z / 15));
					lastPart.builderImage.transform.localEulerAngles = y;
				}
			else lastPart.builderImage.transform.localEulerAngles = Vector3.zero;
			parts.Remove(lastPart);
			builder.parts.Remove(lastPart.part);
			lastPart.part.rotation = lastPart.builderImage.transform.localEulerAngles.z;
			parts.Add(lastPart);
			builder.parts.Add(lastPart.part);
			return;
		}
		if(mode == CursorMode.AddRemove) {
			if(currentPart.partID == "") {
				if(Input.GetMouseButtonDown(0)) {
					for(int i = parts.Count - 1; i >= 0; i--) {
						if(RectTransformUtility.RectangleContainsScreenPoint(parts[i].builderImage.rectTransform, transform.position)) {
							GrabPart(parts[i]);
							break;
						}
					}
				} else {
					image.enabled = false;
					shooter.enabled = false;
				}
			} 
			else {
				image.enabled = true;
				var sprite = ResourceManager.GetAsset<Sprite>(currentPart.partID + "_sprite");
				string shooterID = ShipBuilderInventoryScript.GetShooterID(currentPart.abilityType);
				if(shooterID != null) 
				{
					shooter.sprite = ResourceManager.GetAsset<Sprite>(shooterID);
					shooter.color = FactionColors.colors[0];
					shooter.enabled = true;
					shooter.rectTransform.sizeDelta = shooter.sprite.bounds.size * 100;
				} else shooter.enabled = false;
				image.sprite = sprite;
				image.color = FactionColors.colors[0];
				image.GetComponent<RectTransform>().sizeDelta = sprite.bounds.size * 100;
				if(Input.GetMouseButtonUp(0)) {
					if(RectTransformUtility.RectangleContainsScreenPoint(grid, transform.position)) {
						BuilderPartInfo part = new BuilderPartInfo();
						part.part = currentPart;
						part.builderImage = Instantiate(image.gameObject, 
						transform.position, image.transform.rotation, transform.parent).GetComponent<Image>();
						mode = CursorMode.FlipRotate;
						lastPart = part;
						builder.parts.Add(part.part);
						parts.Add(part);
					} else {
						mode = CursorMode.AddRemove;
					}
					currentPart.partID = "";
					image.transform.localEulerAngles = new Vector3(0,0,1);
					image.enabled = false;
					shooter.enabled = false;
				}
			}
		} 
		else if(Input.GetMouseButtonDown(0)) {
			if(!RectTransformUtility.RectangleContainsScreenPoint(grid, transform.position)) {
				mode = CursorMode.AddRemove;
				return;
			}
			for(int i = parts.Count - 1; i >= 0; i--) {
				if(RectTransformUtility.RectangleContainsScreenPoint(parts[i].builderImage.rectTransform, transform.position)) {
					GrabPart(parts[i]);
					break;
				}
			}
		}
	}
}

