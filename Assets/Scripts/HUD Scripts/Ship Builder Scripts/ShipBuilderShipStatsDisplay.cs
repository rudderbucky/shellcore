using System.Collections;
using System.Collections.Generic;
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
        if (statsDatabase == null) statsDatabase = cursorScript;
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
                    totalRegens[0] += ShellRegen.regens[0] * part.info.tier;
                    break;
                case 18:
                    totalHealths[0] += ShellMax.maxes[0] * part.info.tier;
                    break;
                case 19:
                    totalRegens[2] += ShellRegen.regens[2] * part.info.tier;
                    break;
                case 20:
                    totalHealths[2] += ShellMax.maxes[2] * part.info.tier;
                    break;
                case 22:
                    totalRegens[1] += ShellRegen.regens[1] * part.info.tier;
                    break;
                case 23:
                    totalHealths[1] += ShellMax.maxes[1] * part.info.tier;
                    break;
            }
            PartBlueprint blueprint = ResourceManager.GetAsset<PartBlueprint>(part.info.partID);
            totalHealths[0] += blueprint.health / 2;
            totalHealths[1] += blueprint.health / 4;
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

        string displayText = string.Join("\n", new string[]
        {
            $"SHELL: {totalHealths[0]}",
            $"CORE: {totalHealths[1]}",
            $"ENERGY: {totalHealths[2]}",
            $"SPEED: {(int)Craft.GetPhysicsSpeed(speed, weight)}",
            $"WEIGHT: {(int)weight}",
            buildStat
        });
        display.text = displayText;
        regenDisplay.text = $"REGEN: {totalRegens[0]}\nREGEN: {totalRegens[1]}\nREGEN: {totalRegens[2]}";
    }
}
