﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DroneType {
    Mini,
    Counter,
    Worker,
    Strike,
    Light,
    Gun,
    Torpedo,
    Heavy
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
