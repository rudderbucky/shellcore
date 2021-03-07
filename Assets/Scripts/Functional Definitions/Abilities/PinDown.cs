using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Immobilizes the nearest enemy
/// </summary>
public class PinDown : ActiveAbility, IChargeOnUseThenBlink
{
    float activationDelay = 2f; // the delay between clicking the ability and its activation
    Craft target;

    protected override void Awake()
    {
        base.Awake(); // base awake
                      // hardcoded values here
        ID = AbilityID.PinDown;
        energyCost = 100f;
        cooldownDuration = 10f;
        CDRemaining = 10f;
        activeDuration = 5f;
        activeTimeRemaining = activeDuration;
    }

    public override void Tick(int key)
    {
        base.Tick(key);
        if (isOnCD && Time.time > activationTime && !trueActive && GetActiveTimeRemaining() > 0)
        {
            AudioManager.PlayClipByID("clip_activateability", transform.position);
            trueActive = true;

            var targeting = Core.GetTargetingSystem();
            float minDist = float.MaxValue;
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
                Debug.Log(target.name + " has been made immobile!");
                target.AddPin();
            }
        }
    }

    protected override void Deactivate()
    {
        base.Deactivate();
        trueActive = false;
        if (target != null && target)
        {
            target.RemovePin();
            Debug.Log(target.name + " has been made mobile again!");
        }
    }

    /// <summary>
    /// Immobilizes a nearby enemy
    /// </summary>
    protected override void Execute()
    {
        activationTime = Time.time + activationDelay;
        isOnCD = true; // set to on cooldown
        isActive = true; // set to "active"
        base.Execute();
    }
}