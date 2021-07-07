using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public interface IShipStatsDatabase
{
    List<DisplayPart> GetParts();
    BuilderMode GetMode();
    int GetBuildValue();
    int GetBuildCost();
}

public class ShipBuilderShipStatsDisplay : MonoBehaviour
{
    public ShipBuilderCursorScript cursorScript;
    public IShipStatsDatabase statsDatabase;
    public Text display;
    public Text regenDisplay;

    void Start()
    {
        if (statsDatabase == null)
        {
            statsDatabase = cursorScript;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float[] totalHealths = CoreUpgraderScript.defaultHealths;
        float[] totalRegens = CoreUpgraderScript.GetRegens(cursorScript?.player?.blueprint?.coreShellSpriteID);
        float shipMass = 1;
        float enginePower = 200;
        float weight = Entity.coreWeight;
        float speed = Craft.initSpeed;
        foreach (DisplayPart part in statsDatabase.GetParts())
        {
            switch (part.info.abilityID)
            {
                case 13:
                    enginePower *= Mathf.Pow(1.1F, part.info.tier);
                    speed += 15 * part.info.tier;
                    break;
                case 17:
                    totalRegens[0] += 50 * part.info.tier;
                    break;
                case 18:
                    totalHealths[0] += 250 * part.info.tier;
                    break;
                case 19:
                    totalRegens[2] += 50 * part.info.tier;
                    break;
                case 20:
                    totalHealths[2] += 250 * part.info.tier;
                    break;
            }

            PartBlueprint blueprint = ResourceManager.GetAsset<PartBlueprint>(part.info.partID);
            totalHealths[0] += blueprint.health * 0.5f;
            totalHealths[1] += blueprint.health * 0.25f;
            shipMass += blueprint.mass;
            weight += blueprint.mass * Entity.weightMultiplier;
        }

        string buildStat;
        if (statsDatabase.GetMode() == BuilderMode.Yard || statsDatabase.GetMode() == BuilderMode.Workshop)
        {
            buildStat = $"\nTOTAL BUILD VALUE: \n{statsDatabase.GetBuildValue()} CREDITS";
        }
        else
        {
            string colorTag = "<color=white>";
            if (cursorScript.buildCost > 0)
            {
                colorTag = "<color=red>";
            }
            else if (cursorScript.buildCost < 0)
            {
                colorTag = "<color=lime>";
            }

            buildStat = $"TOTAL BUILD COST: \n{colorTag}{statsDatabase.GetBuildCost()} CREDITS</color>";
        }

        StringBuilder displayText = new StringBuilder();
        displayText.AppendLine($"SHELL: {totalHealths[0]}");
        displayText.AppendLine($"CORE: {totalHealths[1]}");
        displayText.AppendLine($"ENERGY: {totalHealths[2]}");
        displayText.AppendLine($"SPEED: {(int)Craft.GetPhysicsSpeed(speed, weight)}");
        displayText.AppendLine($"WEIGHT: {(int)weight}");
        displayText.AppendLine(buildStat);
        display.text = displayText.ToString();
        regenDisplay.text = $"REGEN: {totalRegens[0]}\n\nREGEN: {totalRegens[2]}";
    }
}
