using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;
public class ShipBuilderCursorScript : MonoBehaviour {

	public EntityBlueprint.PartInfo currentPart;
	public RectTransform grid;
	public Image image;
	public struct BuilderPartInfo {
		public EntityBlueprint.PartInfo part;
		public Image builderImage;
	}

	public ShipBuilder builder;
	void Start() {
		image = GetComponentInChildren<Image>();
		image.preserveAspect = true;
	}
	
	// Update is called once per frame
	void Update () {
		transform.SetAsLastSibling();
		transform.position = new Vector3(10 * ((int)Input.mousePosition.x / 10), 10 * ((int)Input.mousePosition.y / 10), 0);
		currentPart.location = GetComponent<RectTransform>().anchoredPosition / 100;
		if(currentPart.partID == "") image.enabled = false;
		else {
			image.enabled = true;
			var sprite = ResourceManager.GetAsset<Sprite>("BigCenter1_sprite");
			image.sprite = sprite;
			image.color = FactionColors.colors[0];
			image.GetComponent<RectTransform>().sizeDelta = sprite.bounds.size * 100;
			if(Input.GetMouseButtonUp(0)) {
				if(RectTransformUtility.RectangleContainsScreenPoint(grid, transform.position)) {
					BuilderPartInfo part = new BuilderPartInfo();
					part.part = currentPart;
					part.builderImage = Instantiate(image, transform.position, Quaternion.identity, transform.parent);
					builder.parts.Add(part.part);
				}
				sprite = null;
				currentPart.partID = "";
			}
		}
	}
}
