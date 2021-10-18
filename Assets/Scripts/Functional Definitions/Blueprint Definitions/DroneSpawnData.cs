using UnityEngine;

public enum DroneType
{
    Mini,
    Worker,
    Strike,
    Light,
    Gun,
    Counter,
    Torpedo,
    Heavy,
    BulletMini
}

[CreateAssetMenu(fileName = "DroneSpawnAbility", menuName = "ShellCore/Drone Spawn Data", order = 8)]
public class DroneSpawnData : ScriptableObject
{
    public string drone;
    public float energyCost;
    public float delay;
    public float cooldown;
    public DroneType type;
}
