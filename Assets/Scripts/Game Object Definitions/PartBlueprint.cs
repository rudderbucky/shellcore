using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Part", menuName = "ShellCore/Part", order = 2)]
public class PartBlueprint : ScriptableObject
{
    public string spriteID;
    public float health;
    public float mass;
    public Ability.AbilityType abilityType;
    public bool requiresShooter;
    public string shooterSpriteID;
}

// TODO: editor:
// Show part sprite when the ID is correct & exist in built in resources
// Sprite search?
// This would be really useful b/c Unity's own show & search don't work with this