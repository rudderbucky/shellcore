using System.Collections.Generic;
using UnityEngine.UI;

public class DWSelectionDisplayHandler : SelectionDisplayHandler, IShipStatsDatabase
{
    private int buildValue = 0;
    public ShipBuilderShipStatsDisplay statsDisplay;
    public Text droneDesc;

    void Awake()
    {
        statsDisplay.statsDatabase = this;
        ClearDisplay();
    }

    public override void AssignDisplay(EntityBlueprint blueprint, DroneSpawnData data, int faction = 0)
    {
        base.AssignDisplay(blueprint, data);
        statsDisplay.gameObject.SetActive(true);
        droneDesc.enabled = true;

        string description = string.Join("\n", new string[]
        {
            ($"DRONE TYPE: {data.type}").ToUpper(),
            "UNIQUE CHARACTERISTIC:",
            $"<color=lime>{DroneUtilities.GetUniqueCharacteristic(data.type)}</color>",
            $"PART LIMIT: {DroneUtilities.GetPartLimit(data.type)}",
            $"SPAWNING COOLDOWN: {DroneUtilities.GetCooldown(data.type)}",
            $"SPAWNING DELAY: {DroneUtilities.GetDelay(data.type)}",
            $"SPAWNING ENERGY COST: {DroneUtilities.GetEnergyCost(data.type)}",
        });
        droneDesc.text = description;

        foreach (DisplayPart part in parts)
        {
            buildValue += EntityBlueprint.GetPartValue(part.info);
        }
    }

    public override void ClearDisplay()
    {
        statsDisplay.gameObject.SetActive(false);
        droneDesc.enabled = false;
        buildValue = 0;
        base.ClearDisplay();
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
