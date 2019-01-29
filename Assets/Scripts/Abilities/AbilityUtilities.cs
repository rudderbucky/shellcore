using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AbilityUtilities : MonoBehaviour {

	public static Sprite GetAbilityImageByID(int ID) {
		if(ID == 0) return null;
		return ResourceManager.GetAsset<Sprite>("AbilitySprite" + ID);
	}

	public static string GetDescriptionByID(int ID) {
		switch(ID) {
			case 0:
				return "Does nothing.";
			case 1:
				return "Temporarily increases speed.";
			case 2:
				return "Instantly heal 300 shell.";
			case 3:
				return "Projectile that deals " + 100 + " damage. \nStays with you no matter what.";
			case 4:
				return "Instant attack that deals " + 500 + " damage.";
			case 5:
				return "Projectile that deals " + 450 + " damage.";
			case 6:
				return "Instant attack that deals " + 100 + " damage.";
			case 7:
				return "Homing projectile that deals " + 1000 + " damage.";
			case 8:
				return "Slow projectile that deals " + 500 + " damage to ground entities.";
			case 9:
				return "Fast projectile that deals " + 50 + " damage. 50% pierces to core.";
			case 10:
				return "Spawns a drone.";
			case 11:
				return "Instantly heal 300 core.";
			case 12:
				return "Instantly heal 300 energy.";
			case 13:
				return "Passively increases speed.";
			default:
				return "Description unset";
		}
	}

	public static string GetShooterByID(int ID, string data = null) {
		switch(ID) {
			case 0:
			case 13:
				return null;
			case 4:
				return "beamshooter_sprite";
			case 5:
			case 14:
			case 15:
				return "bulletshooter_sprite";
			case 6:
				return "cannonshooter_sprite";
			case 7:
			if(data != "missile_station_shooter")
				return "missileshooter_sprite";
			else return "missile_station_shooter";
			case 8:
				return "torpedoshooter_sprite";
			case 9:
				return "lasershooter_sprite";
			default:
				return "ability_indicator";
		}
	}
	public static string GetAbilityNameByID(int ID) {
        switch(ID) {
			case 0:
				return "None";
            case 1:
                return "Speed Thrust";
            case 2:
                return "Shell Boost";
            case 3:
                return "Main Bullet";
            case 4:
                return "Beam";
            case 5:
                return "Bullet";
            case 6:
                return "Cannon";
            case 7:
                return "Missile";
            case 8:
                return "Torpedo";
            case 9:
                return "Laser";
            case 10:
                return "Spawn Drone";
            case 11:
                return "Core Heal";
            case 12:
                return "Energy";
            case 13:
                return "Speed";
            default:
                return "Name unset";
        }
    }
	public static Ability AddAbilityToGameObjectByID(GameObject obj, int ID, string data = null) {
		Ability ability = null;
		switch(ID) {
			case 1:
				ability = obj.AddComponent<SpeedThrust>();
				break;
			case 2:
				ability = obj.AddComponent<HealthHeal>();
				((HealthHeal)ability).type = HealthHeal.HealingType.shell;
				((HealthHeal)ability).Initialize();
				break;
			case 3:
				Debug.Log("Main bullet should be intrinsically added!");
				ability = obj.AddComponent<MainBullet>();
				break;
			case 4:
				ability = obj.AddComponent<Beam>();
				break;
			case 5:
				ability = obj.AddComponent<Bullet>();
				break;
			case 6:
				ability = obj.AddComponent<Cannon>();
				break;
			case 7:
				ability = obj.AddComponent<Missile>();
				if(data == "missile_station_shooter") {
					((Missile)ability).category = Entity.EntityCategory.All;
					((Missile)ability).terrain = Entity.TerrainType.All;
				}
				break;
			case 8:
				ability = obj.AddComponent<Torpedo>();
				break;
			case 9:
				ability = obj.AddComponent<Laser>();
				break;
			case 10:
				ability = obj.AddComponent<SpawnDrone>();
				((SpawnDrone)ability).spawnData = ResourceManager.GetAsset<DroneSpawnData>(data);
				((SpawnDrone)ability).Init();
				break;
			case 11:
				ability = obj.AddComponent<HealthHeal>();
				((HealthHeal)ability).type = HealthHeal.HealingType.core;
				((HealthHeal)ability).Initialize();
				break;
			case 12:
				ability = obj.AddComponent<HealthHeal>();
				((HealthHeal)ability).type = HealthHeal.HealingType.energy;
				((HealthHeal)ability).Initialize();
				break;
			case 13:
				ability = obj.AddComponent<Speed>();
				break;
			case 14:
				ability = obj.AddComponent<SiegeBullet>();
				break;
			case 15:
				ability = obj.AddComponent<SpeederBullet>();
				break;
			case 16:
				ability = obj.AddComponent<Harvester>();
				break;
		}
		return ability;
	}
}
