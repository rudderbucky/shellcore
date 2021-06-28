using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Immobilizes the nearest enemy
/// </summary>
public class PinDown : ActiveAbility
{
    Craft target;
    const float range = 15f;
    float rangeSquared = range * range;

    public override float GetRange()
    {
        return range;
    }
    
    protected override void Awake()
    {
        base.Awake(); // base awake
                      // hardcoded values here
        ID = AbilityID.PinDown;
        energyCost = 100f;
        cooldownDuration = 10f;
        activeDuration = 5f;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        if (target != null && target)
        {
            target.RemovePin();
        }
    }

    /// <summary>
    /// Immobilizes a nearby enemy
    /// </summary>
    protected override void Execute()
    {
        AudioManager.PlayClipByID("clip_activateability", transform.position);
        var targeting = Core.GetTargetingSystem();
        float minDist = rangeSquared;
        target = null;
        for (int i = 0; i < AIData.entities.Count; i++)
        {
            if (AIData.entities[i] is Craft && !AIData.entities[i].GetIsDead() && !FactionManager.IsAllied(AIData.entities[i].faction, Core.faction))
            {
                float d = (Core.transform.position - AIData.entities[i].transform.position).sqrMagnitude;
                if (d < minDist)
                {
                    minDist = d;
                    target = AIData.entities[i] as Craft;
                }
            }
        }

        if (target != null)
        {
            target.AddPin();

            
            var missileLinePrefab = new GameObject("Missile Line"); // create prefab and set to parent
            missileLinePrefab.transform.SetParent(transform, false);

            var missileColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction) : new Color(0.8F, 1F, 1F, 0.9F);

            // I use this prefab as one of the active lines on the missile 
            // because what's the point in not doing it this way

            LineRenderer lineRenderer = missileLinePrefab.AddComponent<LineRenderer>(); // add line renderer
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material"); // get material
            MissileAnimationScript comp = missileLinePrefab.AddComponent<MissileAnimationScript>(); // add the animation script
            foreach(var part in target.GetComponentsInChildren<ShellPart>())
            {
                var x = Instantiate(missileLinePrefab, part.transform); // instantiate
                x.GetComponent<MissileAnimationScript>().Initialize(); // initialize
                x.GetComponent<MissileAnimationScript>().lineColor = missileColor;
                Destroy(x, activeDuration);
            }
        }
        base.Execute();
    }
}