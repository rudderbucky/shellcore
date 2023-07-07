using System.Collections.Generic;
using UnityEngine;

public static class DroneUtilities
{
    public static DroneSpawnData GetDefaultData(DroneType type)
    {
        DroneSpawnData data;
        // pretty much the same thing repeated multiple times so that if one part ever becomes different it doesn't break
        switch (type)
        {
            case DroneType.Mini:
                data = ResourceManager.GetAsset<DroneSpawnData>("mini_drone_spawn");
                data.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("mini_drone_blueprint"));
                break;
            case DroneType.Counter:
                data = ResourceManager.GetAsset<DroneSpawnData>("counter_drone_spawn");
                data.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("counter_drone_blueprint"));
                break;
            case DroneType.Light:
                data = ResourceManager.GetAsset<DroneSpawnData>("light_drone_spawn");
                data.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("light_drone_blueprint"));
                break;
            case DroneType.Strike:
                data = ResourceManager.GetAsset<DroneSpawnData>("strike_drone_spawn");
                data.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("strike_drone_blueprint"));
                break;
            case DroneType.Worker:
                data = ResourceManager.GetAsset<DroneSpawnData>("worker_drone_spawn");
                data.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("worker_drone_blueprint"));
                break;
            case DroneType.Gun:
                data = ResourceManager.GetAsset<DroneSpawnData>("gun_drone_spawn");
                data.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("gun_drone_blueprint"));
                break;
            case DroneType.Torpedo:
                data = ResourceManager.GetAsset<DroneSpawnData>("torpedo_drone_spawn");
                data.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("torpedo_drone_blueprint"));
                break;
            case DroneType.Heavy:
                data = ResourceManager.GetAsset<DroneSpawnData>("heavy_drone_spawn");
                data.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("heavy_drone_blueprint"));
                break;
            default:
                return null;
        }

        data.cooldown = GetCooldown(type);
        data.energyCost = GetEnergyCost(type);
        return data;
    }

    public static Sprite GetAbilitySpriteBySpawnData(DroneSpawnData data)
    {
        switch (data.type)
        {
            // similarity for reasons described above
            case DroneType.Mini:
                return ResourceManager.GetAsset<Sprite>("mini_drone_ability");
            case DroneType.Counter:
                return ResourceManager.GetAsset<Sprite>("counter_drone_ability");
            case DroneType.Light:
                return ResourceManager.GetAsset<Sprite>("light_drone_ability");
            case DroneType.Strike:
                return ResourceManager.GetAsset<Sprite>("strike_drone_ability");
            case DroneType.Worker:
                return ResourceManager.GetAsset<Sprite>("worker_drone_ability");
            case DroneType.Gun:
                return ResourceManager.GetAsset<Sprite>("gun_drone_ability");
            case DroneType.Torpedo:
                return ResourceManager.GetAsset<Sprite>("torpedo_drone_ability");
            case DroneType.Heavy:
                return ResourceManager.GetAsset<Sprite>("heavy_drone_ability");
            default:
                return null;
        }
    }

    public static WeaponDiversityType GetDiversityTypeByEntity(Entity ent)
    {
        Drone drone = ent as Drone;
        if (!drone)
        {
            return WeaponDiversityType.None;
        }
        else
        {
            switch (drone.type)
            {
                case DroneType.Gun:
                    return WeaponDiversityType.Gun;
                case DroneType.Strike:
                    return WeaponDiversityType.Strike;
                case DroneType.Torpedo:
                    return WeaponDiversityType.Torpedo;
                default:
                    return WeaponDiversityType.None;
            }
        }
    }

    public static string GetUniqueCharacteristic(DroneType type)
    {
        switch (type)
        {
            case DroneType.Mini:
                return "FREE CANNON.\n";
            case DroneType.Worker:
                return "COLLECTS DIFFERENT OBJECTS.\n";
            case DroneType.Strike:
                return "60% WEAPON ENERGY USAGE.\n";
            case DroneType.Light:
                return "60% LIGHTER THAN USUAL.\n";
            case DroneType.Gun:
                return "60% COOLDOWN TIMES.\n";
            case DroneType.Counter:
                return "75% MORE WEAPON DAMAGE AGAINST DRONES.";
            case DroneType.Torpedo:
                return "WEAPONS ATTACK ONLY GROUND ENTITIES.";
            case DroneType.Heavy:
                return "REGENERATES 20 CORE PER SECOND.\n";
            default:
                return "";
        }
    }

    public static int GetPartLimit(DroneType type)
    {
        switch (type)
        {
            case DroneType.Mini:
            case DroneType.Worker:
                return 2;
            case DroneType.Strike:
            case DroneType.Light:
            case DroneType.Counter:
            case DroneType.Gun:
                return 4;
            case DroneType.Torpedo:
                return 6;
            case DroneType.Heavy:
                return 8;
            default:
                return 0;
        }
    }

    public static int GetEnergyCost(DroneType type)
    {
        switch (type)
        {
            case DroneType.Mini:
            case DroneType.Worker:
                return 100;
            case DroneType.Strike:
            case DroneType.Light:
            case DroneType.Counter:
            case DroneType.Gun:
                return 150;
            case DroneType.Torpedo:
                return 200;
            case DroneType.Heavy:
                return 400;
            default:
                return 0;
        }
    }

    public static int GetCooldown(DroneType type)
    {
        switch (type)
        {
            case DroneType.Mini:
            case DroneType.Worker:
                return 10;
            case DroneType.Strike:
            case DroneType.Light:
            case DroneType.Counter:
            case DroneType.Gun:
                return 15;
            case DroneType.Torpedo:
                return 20;
            case DroneType.Heavy:
                return 30;
            default:
                return 0;
        }
    }

    public static int GetDelay(DroneType type)
    {
        switch (type)
        {
            case DroneType.Mini:
            case DroneType.Worker:
                return 2;
            case DroneType.Strike:
            case DroneType.Light:
            case DroneType.Counter:
            case DroneType.Gun:
                return 3;
            case DroneType.Torpedo:
                return 5;
            case DroneType.Heavy:
                return 8;
            default:
                return 0;
        }
    }

    public static string GetAbilityNameByType(DroneType type)
    {
        switch (type)
        {
            case DroneType.Mini:
                return "Mini Drone";
            case DroneType.Worker:
                return "Worker Drone";
            case DroneType.Strike:
                return "Strike Drone";
            case DroneType.Counter:
                return "Counter Drone";
            case DroneType.Gun:
                return "Gun Drone";
            case DroneType.Torpedo:
                return "Torpedo Drone";
            case DroneType.Light:
                return "Light Drone";
            case DroneType.Heavy:
                return "Heavy Drone";
            default:
                return "Spawn Drone";
        }
    }

    public static string GetDescriptionByType(DroneType type)
    {
        switch (type)
        {
            case DroneType.Mini:
                return "Spawns a Mini Drone, which have free cannons.";
            case DroneType.Worker:
                return "Spawns a Worker Drone, which can collect various objects for you.";
            case DroneType.Strike:
                return "Spawns a Strike Drone, their weapons use less energy.";
            case DroneType.Counter:
                return "Spawns a Counter Drone, they deal much more damage against other drones.";
            case DroneType.Gun:
                return "Spawns a Gun Drone, their weapons cooldown faster.";
            case DroneType.Torpedo:
                return "Spawns a Torpedo Drone, all of their weapons attack only ground entities.";
            case DroneType.Light:
                return "Spawns a Light Drone, they are lighter than usual.";
            case DroneType.Heavy:
                return "Spawns a Heavy Drone, they regenerate their core.";
            default:
                return "Spawns a drone.";
        }
    }

    public static List<string> DEFAULT_SECONDARY_DATA = new List<string>() {"mini_drone", "counter_drone", "light_drone", "strike_drone", "gun_drone", "heavy_drone", "torpedo_drone", "worker_drone"};

    public static DroneSpawnData GetDroneSpawnDataByShorthand(string secondaryData)
    {
        switch (secondaryData)
        {
            case "mini_drone":
                return ResourceManager.GetAsset<DroneSpawnData>("mini_drone_spawn");
            case "counter_drone":
                return ResourceManager.GetAsset<DroneSpawnData>("counter_drone_spawn");
            case "light_drone":
                return ResourceManager.GetAsset<DroneSpawnData>("light_drone_spawn");
            case "strike_drone":
                return ResourceManager.GetAsset<DroneSpawnData>("strike_drone_spawn");
            case "gun_drone":
                return ResourceManager.GetAsset<DroneSpawnData>("gun_drone_spawn");
            case "heavy_drone":
                return ResourceManager.GetAsset<DroneSpawnData>("heavy_drone_spawn");
            case "torpedo_drone":
                return ResourceManager.GetAsset<DroneSpawnData>("torpedo_drone_spawn");
            case "worker_drone":
                return ResourceManager.GetAsset<DroneSpawnData>("worker_drone_spawn");
            default:
                try
                {
                    var blueprint = SectorManager.TryGettingEntityBlueprint(secondaryData);
                    var data = ScriptableObject.CreateInstance<DroneSpawnData>();
                    data.cooldown = GetCooldown(blueprint.customDroneType);
                    data.energyCost = GetEnergyCost(blueprint.customDroneType);
                    data.delay = GetDelay(blueprint.customDroneType);
                    blueprint.entityName = GetAbilityNameByType(blueprint.customDroneType);
                    data.drone = JsonUtility.ToJson(blueprint);
                    data.type = blueprint.customDroneType;
                    return data;
                }
                catch {};
                var spawnData = ScriptableObject.CreateInstance<DroneSpawnData>();
                JsonUtility.FromJsonOverwrite(secondaryData, spawnData);
                return spawnData;
        }
    }
}
