using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Extra data stored for world logic
public class WorldData : ScriptableObject
{
    // Characters are special entities linked to the storyline of which there can only be one
    // spawned at a time in the entire world. Party members are an example though
    // not all characters are party members. Characters may be spawned independently of sector entities.
    public string[] characters;

    // initial spawn point for the player if the world is loaded for the first time.
    public Vector2 initialSpawn;
}
