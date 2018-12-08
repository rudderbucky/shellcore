using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Part", menuName = "ShellCore/Part", order = 2)]
public class PartBlueprint : ScriptableObject
{
    public string spriteID;
    public float health;
    public float mass;
    public Ability.AbilityType type;
    public bool requiresShooter;
    public string shooterSpriteID;
}
