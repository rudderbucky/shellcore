using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DWSelectionDisplayHandler : MonoBehaviour, IShipStatsDatabase
{
    public Image shell;
    public Image core;
    public Image miniDroneShooter;
    public GameObject partPrefab;
    private List<DisplayPart> parts = new List<DisplayPart>();
    private int buildValue = 0;
    public ShipBuilderShipStatsDisplay statsDisplay;
    public Text droneDesc;
    void Awake() {
        statsDisplay.statsDatabase = this;
        ClearDisplay();
    }
    public void AssignDisplay(EntityBlueprint blueprint, DroneSpawnData data) {
        ClearDisplay();
        statsDisplay.gameObject.SetActive(true);
        droneDesc.enabled = true;
        droneDesc.text = ("DRONE TYPE: " + data.type).ToUpper()
                     + "\nUNIQUE CHARACTERISTIC:\n" + "<color=lime>"
                      + DroneUtilities.GetUniqueCharacteristic(data.type) + "</color>"
                     + "\nPART LIMIT: " + DroneUtilities.GetPartLimit(data.type)
                     + "\nSPAWNING COOLDOWN: " + DroneUtilities.GetCooldown(data.type)
                     + "\nSPAWNING DELAY: " + DroneUtilities.GetDelay(data.type)
                     + "\nSPAWNING ENERGY COST: " + DroneUtilities.GetEnergyCost(data.type);
        shell.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
        if(shell.sprite) {
            shell.enabled = true;
            shell.rectTransform.sizeDelta = shell.sprite.bounds.size * 100;
            shell.color = FactionColors.colors[0];
        } else {
            shell.enabled = false;
        }
        core.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);
        if(core.sprite) {
            core.enabled = true;
            core.rectTransform.sizeDelta = core.sprite.bounds.size * 100;
            core.type = Image.Type.Sliced;
        } else {
            core.enabled = false;
        }
        if(data.type == DroneType.Mini) {
            miniDroneShooter.enabled = true;
            miniDroneShooter.sprite = ResourceManager.GetAsset<Sprite>(AbilityUtilities.GetShooterByID(6));
            miniDroneShooter.color = FactionColors.colors[0];
            miniDroneShooter.rectTransform.sizeDelta = miniDroneShooter.sprite.bounds.size * 100;
            miniDroneShooter.type = Image.Type.Sliced;
        } else {
            miniDroneShooter.enabled = false;
        }
        foreach(EntityBlueprint.PartInfo part in blueprint.parts) {
            DisplayPart basePart = Instantiate(partPrefab, transform, false).GetComponent<DisplayPart>();
            parts.Add(basePart);
            basePart.info = part;
        }
        foreach(DisplayPart part in parts) {
			buildValue += EntityBlueprint.GetPartValue(part.info);
		}
    }

    public void ClearDisplay() {
        statsDisplay.gameObject.SetActive(false);
        droneDesc.enabled = false;
        buildValue = 0;
        parts.Clear();
        shell.enabled = false;
        core.enabled = false;
        miniDroneShooter.enabled = false;
        for(int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public List<DisplayPart> GetParts()
    {
        return parts;
    }

    public BuilderMode GetMode()
    {
        return BuilderMode.Yard;
    }

    public int GetBuildValue()
    {
        return buildValue;
    }

    public int GetBuildCost()
    {
        return 0;
    }
}
