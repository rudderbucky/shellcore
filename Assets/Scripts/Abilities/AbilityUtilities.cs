using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AbilityUtilities : MonoBehaviour {

	public static Sprite GetAbilityImageByID(int ID, string secondaryData) {
		if(ID == 0) return null;
		if(ID == 10) {
			DroneSpawnData data = ScriptableObject.CreateInstance<DroneSpawnData>();
			JsonUtility.FromJsonOverwrite(secondaryData, data);
			return DroneUtilities.GetAbilitySpriteBySpawnData(data);
		}
		return ResourceManager.GetAsset<Sprite>("AbilitySprite" + ID);
	}

	public static Sprite GetAbilityImage(Ability ability) {
		var ID = ability.GetID();
		if(ID == 0) return null;
		if(ID == 10) {
			return DroneUtilities.GetAbilitySpriteBySpawnData((ability as SpawnDrone).spawnData);
		}
		return ResourceManager.GetAsset<Sprite>("AbilitySprite" + ID);
	}

	public static AbilityHandler.AbilityTypes GetAbilityTypeByID(int ID) {
		switch(ID) {
			case 10:
				return AbilityHandler.AbilityTypes.Spawns;
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
				return AbilityHandler.AbilityTypes.Weapons;
			case 1:
			case 2:
			case 11:
			case 12:
				return AbilityHandler.AbilityTypes.Skills;
			case 13:
			case 17:
			case 18:
			case 19:
			case 20:
				return AbilityHandler.AbilityTypes.Passive;
			case 0:
			default:
				return AbilityHandler.AbilityTypes.None;
		}
		
	}
	public static string GetDescriptionByID(int ID, int tier, string secondaryData) {
		switch(ID) {
			case 0:
				return "Does nothing.";
			case 1:
				return "Temporarily increases speed.";
			case 2:
				return "Instantly heal " + 300 * tier + " shell.";
			case 3:
				return "Projectile that deals " + 100 + " damage. \nStays with you no matter what.";
			case 4:
				return "Instant attack that deals " + 500 * tier + " damage.";
			case 5:
				return "Projectile that deals " + 450 * tier + " damage.";
			case 6:
				return "Instant attack that deals " + 100 * tier + " damage.";
			case 7:
				return "Homing projectile that deals " + 1000 * tier + " damage.";
			case 8:
				return "Slow projectile that deals " + 500 * tier + " damage to ground entities.";
			case 9:
				return "Fast projectile that deals " + 50 * tier + " damage. 50% pierces to core.";
			case 10:
				if(secondaryData == null) return "Spawns a drone.";			
				DroneSpawnData data = ScriptableObject.CreateInstance<DroneSpawnData>();
				JsonUtility.FromJsonOverwrite(secondaryData, data);
				return DroneUtilities.GetDescriptionByType(data.type);
			case 11:
				return "Instantly heal " + 300 * tier + " core.";
			case 12:
				return "Instantly heal " + 100 * tier + " energy.";
			case 13:
				return "Passively increases speed.";
			case 17:
				return "Passively increases shell regen by "  + 50 * tier + " points.";
			case 18:
				return "Passively increases maximum shell by " + 250 * tier + " points.";
			case 19:
				return "Passively increases energy regen by " + 50 * tier + " points.";
			case 20:
				return "Passively increases maximum energy by " + 250 * tier + " points.";
			default:
				return "Description unset";
		}
	}

	public static string GetDescription(Ability ability) {
		switch(ability.GetID()) {
			case 0:
				return "Does nothing.";
			case 1:
				return "Temporarily increases speed.";
			case 2:
				return "Instantly heal " + 300 * ability.GetTier() + " shell.";
			case 3:
				return "Projectile that deals " + 100 + " damage. \nStays with you no matter what.";
			case 4:
				return "Instant attack that deals " + 500 * ability.GetTier() + " damage.";
			case 5:
				return "Projectile that deals " + 450 * ability.GetTier() + " damage.";
			case 6:
				return "Instant attack that deals " + 100 * ability.GetTier() + " damage.";
			case 7:
				return "Homing projectile that deals " + 1000 * ability.GetTier() + " damage.";
			case 8:
				return "Slow projectile that deals " + 500 * ability.GetTier() + " damage to ground entities.";
			case 9:
				return "Fast projectile that deals " + 50 * ability.GetTier() + " damage. 50% pierces to core.";
			case 10:
				return DroneUtilities.GetDescriptionByType((ability as SpawnDrone).spawnData.type);
			case 11:
				return "Instantly heal " + 300 * ability.GetTier() + " core.";
			case 12:
				return "Instantly heal " + 100 * ability.GetTier() + " energy.";
			case 13:
				return "Passively increases speed.";
			case 17:
				return "Passively increases shell regen by "  + 50 * ability.GetTier() + " points.";
			case 18:
				return "Passively increases maximum shell by " + 250 * ability.GetTier() + " points.";
			case 19:
				return "Passively increases energy regen by " + 50 * ability.GetTier() + " points.";
			case 20:
				return "Passively increases maximum energy by " + 250 * ability.GetTier() + " points.";
			default:
				return "Description unset";
		}
	}
	public static string GetShooterByID(int ID, string data = null) {
		switch(ID) {
			case 0:
			case 13:
			case 17:
			case 18:
			case 19:
			case 20:
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
	public static string GetAbilityNameByID(int ID, string secondaryData) {
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
				if(secondaryData == null) return "Spawn Drone";			
				DroneSpawnData data = ScriptableObject.CreateInstance<DroneSpawnData>();
				JsonUtility.FromJsonOverwrite(secondaryData, data);
				return DroneUtilities.GetAbilityNameByType(data.type);
            case 11:
                return "Core Heal";
            case 12:
                return "Energy";
            case 13:
                return "Speed";
			case 17:
				return "Shell Regen";
			case 18:
				return "Shell Max";
			case 19:
				return "Energy Regen";
			case 20:
				return "Energy Max";
            default:
                return "Name unset";
        }
    }
	public static string GetAbilityName(Ability ability) {
        switch(ability.GetID()) {
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
				return DroneUtilities.GetAbilityNameByType((ability as SpawnDrone).spawnData.type);
            case 11:
                return "Core Heal";
            case 12:
                return "Energy";
            case 13:
                return "Speed";
			case 17:
				return "Shell Regen";
			case 18:
				return "Shell Max";
			case 19:
				return "Energy Regen";
			case 20:
				return "Energy Max";
            default:
                return "Name unset";
        }
    }
	public static Ability AddAbilityToGameObjectByID(GameObject obj, int ID, string data = null, int tier = 0) {
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
				((SpawnDrone)ability).spawnData = ScriptableObject.CreateInstance<DroneSpawnData>();
				JsonUtility.FromJsonOverwrite(data, ((SpawnDrone)ability).spawnData);
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
			case 17:
				ability = obj.AddComponent<ShellRegen>();
				(ability as ShellRegen).index = 0;
				(ability as ShellRegen).Initialize();
				break;
			case 18:
				ability = obj.AddComponent<ShellMax>();
				(ability as ShellMax).index = 0;
				(ability as ShellMax).Initialize();
				break;
			case 19:
				ability = obj.AddComponent<ShellRegen>();
				(ability as ShellRegen).index = 2;
				(ability as ShellRegen).Initialize();
				break;
			case 20:
				ability = obj.AddComponent<ShellMax>();
				(ability as ShellMax).index = 2;
				(ability as ShellMax).Initialize();
				break;
		}
		if(ability) ability.SetTier(tier);
		return ability;
	}
}
