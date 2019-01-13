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

        public Ability.AbilityType abilityType;
        public string spawnID;

        public string partID; //Part blueprint ID
    }
    public string entityName = "Unnamed";
    public string coreSpriteID;
    public string coreShellSpriteID;
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
        AirCarrier
    }

    public IntendedType intendedType;
    public List<PartInfo> parts;
}
