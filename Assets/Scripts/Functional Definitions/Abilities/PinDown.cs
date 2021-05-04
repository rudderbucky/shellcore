using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Immobilizes the nearest enemy
/// </summary>
public class PinDown : ActiveAbility
{
    Craft target;
    float rangeSquared = 15f * 15f;

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
        }
        base.Execute();
    }
}