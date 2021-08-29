using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains the list of parts and their relative locations and rotations to the craft
/// </summary>
/// 
[CreateAssetMenu(fileName = "Craft", menuName = "ShellCore/Craft", order = 1)]
public class EntityBlueprint : ScriptableObject
{
    [System.Serializable]
    public struct PartInfo
    {
        // Location, rotation and mirroring
        public Vector2 location;
        public float rotation;
        public bool mirrored;
        public int abilityID;
        public int tier;
        public string secondaryData;
        public string partID; //Part blueprint ID
        public bool shiny;
        public bool Equals(PartInfo other)
        {
            return this.location == other.location && this.rotation == other.rotation && this.mirrored == other.mirrored &&
                this.abilityID == other.abilityID && this.tier == other.tier && this.secondaryData == other.secondaryData && this.partID
                    == other.partID && this.shiny == other.shiny;
        }
    }

    public string entityName = "Unnamed";
    public string coreSpriteID;
    public string coreShellSpriteID;
    public float[] shellHealth = CoreUpgraderScript.defaultHealths;
    public float[] baseRegen = { 60, 0, 30 };

    public enum IntendedType
    {
        ShellCore,
        PlayerCore,
        Turret,
        Tank,
        Bunker,
        Outpost,
        Tower,
        Drone,
        AirCarrier,
        GroundCarrier,
        Yard,
        WeaponStation,
        Trader,
        CoreUpgrader,
        DroneWorkshop,
        AirWeaponStation
    }

    public IntendedType intendedType;
    public List<PartInfo> parts;
    public Dialogue dialogue;

    public static int GetPartValue(PartInfo info)
    {
        return (int)ResourceManager.GetAsset<PartBlueprint>(info.partID).health + info.tier * 200 + (info.abilityID == 0 ? 0 : 300);
    }

    public bool useCustomDroneType = false;
    public DroneType customDroneType;
}
