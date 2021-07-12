﻿using UnityEngine;

/// <summary>
/// Resets active cooldowns of nearby enemies
/// </summary>
public class Disrupt : Ability
{
    const float range = 10f;

    public override float GetRange()
    {
        return range;
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        ID = AbilityID.Disrupt;
        energyCost = 200;
        cooldownDuration = 30;
    }

    /// <summary>
    /// Resets active cooldowns of nearby enemies
    /// </summary>
    protected override void Execute()
    {
        for (int i = 0; i < AIData.entities.Count; i++)
        {
            if (AIData.entities[i] is Craft && !AIData.entities[i].GetIsDead() && !FactionManager.IsAllied(AIData.entities[i].faction, Core.faction) && !AIData.entities[i].invisible)
            {
                float d = (Core.transform.position - AIData.entities[i].transform.position).sqrMagnitude;
                if (d < range * range)
                {
                    foreach (var ability in AIData.entities[i].GetAbilities())
                    {
                        if (ability != null && ability.TimeUntilReady() > 0)
                        {
                            ability.ResetCD();
                        }
                    }

                    foreach (var part in AIData.entities[i].GetComponentsInChildren<ShellPart>())
                    {
                        if (part.GetComponent<Ability>() && !(part.GetComponent<Ability>() as PassiveAbility))
                        {
                            //part.SetPartColor(Color.grey);
                            part.lerpColors();
                            var missileLinePrefab = new GameObject("Missile Line"); // create prefab and set to parent
                            missileLinePrefab.transform.SetParent(transform, false);

                            var missileColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction) : new Color(0.8F, 1F, 1F, 0.9F);

                            // I use this prefab as one of the active lines on the missile 
                            // because what's the point in not doing it this way

                            LineRenderer lineRenderer = missileLinePrefab.AddComponent<LineRenderer>(); // add line renderer
                            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material"); // get material
                            MissileAnimationScript comp = missileLinePrefab.AddComponent<MissileAnimationScript>(); // add the animation script

                            var x = Instantiate(missileLinePrefab, part.transform); // instantiate
                            x.GetComponent<MissileAnimationScript>().Initialize(); // initialize
                            x.GetComponent<MissileAnimationScript>().lineColor = missileColor;
                            Destroy(x, 0.5f);
                        }
                    }
                }
            }
        }

        base.Execute();
    }
}
