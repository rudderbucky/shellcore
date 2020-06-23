using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Immobilizes the nearest enemy
/// </summary>
public class PinDown : ActiveAbility
{
    float activationDelay = 2f; // the delay between clicking the ability and its activation
    float activationTime = 0f;
    bool trueActive = false;
    Craft target;

    protected override void Awake()
    {
        base.Awake(); // base awake
                      // hardcoded values here
        ID = 27;
        energyCost = 100f;
        cooldownDuration = 10f;
        CDRemaining = 10f;
        activeDuration = 5f;
        activeTimeRemaining = activeDuration;
    }

    public override void Tick(string key)
    {
        base.Tick(key);
        if (isOnCD && Time.time > activationTime && !trueActive && GetActiveTimeRemaining() > 0)
        {
            AudioManager.PlayClipByID("clip_activateability", transform.position);
            trueActive = true;
            ToggleIndicator(true);

            var targeting = Core.GetTargetingSystem();
            float minDist = float.MaxValue;
            target = null;
            for (int i = 0; i < AIData.entities.Count; i++)
            {
                if (AIData.entities[i] is Craft && !AIData.entities[i].GetIsDead() && AIData.entities[i].faction != Core.faction)
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
                target.isImmobile = true;
            }
        }
    }

    protected override void Deactivate()
    {
        ToggleIndicator(true);
        trueActive = false;
        if (target != null && target)
        {
            target.isImmobile = false;
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
        ToggleIndicator(false);
    }
}