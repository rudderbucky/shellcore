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
    }
    public string entityName = "Unnamed";
    public string coreSpriteID;
    public string coreShellSpriteID;
    public float[] shellHealth = new float[] {1000,250,500};
    public float[] baseRegen = new float[] {60,0,60};
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
        DroneWorkshop
    }

    public IntendedType intendedType;
    public List<PartInfo> parts;
    public Dialogue dialogue;

    public static int GetPartValue(PartInfo info) {
        return (int)ResourceManager.GetAsset<PartBlueprint>(info.partID).health + info.tier * 200 + (info.abilityID == 0 ? 0 : 300);
    }

    public bool useCustomDroneType = false;
    public DroneType customDroneType;
}
