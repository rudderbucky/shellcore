using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DroneUtilities : MonoBehaviour
{
    public static DroneSpawnData GetDefaultData(DroneType type) {
        // pretty much the same thing repeated multiple times so that if one part ever becomes different it doesn't break
        switch(type) {
            case DroneType.Mini:
                var mini = ResourceManager.GetAsset<DroneSpawnData>("mini_drone_spawn");
                mini.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("mini_drone_blueprint"));
                return mini;
            case DroneType.Counter:
                var counter = ResourceManager.GetAsset<DroneSpawnData>("counter_drone_spawn");
                counter.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("counter_drone_blueprint"));
                return counter;
            case DroneType.Light:
                var light = ResourceManager.GetAsset<DroneSpawnData>("light_drone_spawn");
                light.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("light_drone_blueprint"));
                return light;
            case DroneType.Strike:
                var strike = ResourceManager.GetAsset<DroneSpawnData>("strike_drone_spawn");
                strike.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("strike_drone_blueprint"));
                return strike;
            case DroneType.Worker:
                var worker = ResourceManager.GetAsset<DroneSpawnData>("worker_drone_spawn");
                worker.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("worker_drone_blueprint"));
                return worker;
            case DroneType.Gun:
                var gun = ResourceManager.GetAsset<DroneSpawnData>("gun_drone_spawn");
                gun.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("gun_drone_blueprint"));
                return gun;
            case DroneType.Torpedo:
                var torpedo = ResourceManager.GetAsset<DroneSpawnData>("torpedo_drone_spawn");
                torpedo.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("torpedo_drone_blueprint"));
                return torpedo;
            case DroneType.Heavy:
                var heavy = ResourceManager.GetAsset<DroneSpawnData>("heavy_drone_spawn");
                heavy.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("heavy_drone_blueprint"));
                return heavy;
            default:
                return null;
        }
    }

    public static Sprite GetAbilitySpriteBySpawnData(DroneSpawnData data) {
        switch(data.type) {
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

    public static WeaponDiversityType GetDiversityTypeByEntity(Entity ent) {
        Drone drone = ent as Drone;
        if(!drone) return WeaponDiversityType.None;
        else switch(drone.type) {
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
