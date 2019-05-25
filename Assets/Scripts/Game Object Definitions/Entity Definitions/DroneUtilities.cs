using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneUtilities : MonoBehaviour
{
    public static DroneSpawnData GetDefaultData(DroneType type) {
        switch(type) {
            case DroneType.Mini:
                var mini = ResourceManager.GetAsset<DroneSpawnData>("mini_drone_spawn");
                mini.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("mini_drone_blueprint"));
                return mini;
            case DroneType.Counter:
                var counter = ResourceManager.GetAsset<DroneSpawnData>("counter_drone_spawn");
                counter.drone = JsonUtility.ToJson(ResourceManager.GetAsset<EntityBlueprint>("counter_drone_blueprint"));
                return counter;
            default:
                return null;
        }
    }
}
