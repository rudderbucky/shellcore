using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;
public class PresetButton : MonoBehaviour, IPointerClickHandler {

	public GameObject SBPrefab;
	public ShipBuilder builder;
	public ShipBuilderCursorScript cursorScript;
	public PlayerCore player;
	EntityBlueprint blueprint;
	public int number;
	Image image;
	Text text;
    public void OnPointerClick(PointerEventData eventData)
    {
		if(Input.GetKey(KeyCode.LeftShift)){
			if(player.cursave.presetBlueprints != null) player.cursave.presetBlueprints[number - 1] = null;
			blueprint = null;
			return;
		}
		if(!blueprint) {
			blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
			blueprint.coreShellSpriteID = player.blueprint.coreShellSpriteID;
			blueprint.coreSpriteID = player.blueprint.coreSpriteID;
			blueprint.parts = new List<EntityBlueprint.PartInfo>();
			foreach(ShipBuilderPart part in cursorScript.parts) {
				blueprint.parts.Add(part.info);
			}
			if(player.cursave.presetBlueprints == null) {
				player.cursave.presetBlueprints = new string[5];
			}
			player.cursave.presetBlueprints[number - 1] = JsonUtility.ToJson(blueprint);
		} else {
			cursorScript.ClearAllParts();
			foreach(EntityBlueprint.PartInfo info in blueprint.parts) {
				if(!builder.DecrementPartButton(ShipBuilder.CullSpatialValues(info))) {
					// cursorScript.ClearAllParts();
					builder.CloseUI(false);
					return;
				}
			}
			var x = new EntityBlueprint.PartInfo[blueprint.parts.Count];
			blueprint.parts.CopyTo(x);
			player.blueprint.parts = new List<EntityBlueprint.PartInfo>(x);
			builder.CloseUI(true);
			player.Rebuild();
		}
    }

    // Use this for initialization
    public void Initialize () {
		image = GetComponent<Image>();
		text = GetComponentInChildren<Text>();
		blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
		if(player.cursave.presetBlueprints != null && player.cursave.presetBlueprints[number - 1] != null) 
			JsonUtility.FromJsonOverwrite(player.cursave.presetBlueprints[number - 1], blueprint);
		if(blueprint.parts == null) blueprint = null;
	}
	
	// Update is called once per frame
	void Update () {
		if(!blueprint) 
		{
			image.color = text.color = Color.yellow;
			text.text = " Create Preset " + number;
		}
		else 
		{
			image.color = text.color = Color.green;
			text.text = " Load Preset " + number;
		}
	}
}
