using System.Linq;
using UnityEngine;

/// <summary>
/// Respawns the entity
/// </summary>
public class Retreat : Ability
{
    protected override void Awake()
    {
        base.Awake(); // base awake
                      // hardcoded values here
        ID = AbilityID.Retreat;
        energyCost = 200;
        chargeDuration = 3f;
        activeDuration = 3.1f;
        cooldownDuration = 30;
    }

    protected override void Execute()
    {
        if (Core is Craft)
        {
            AudioManager.PlayClipByID("clip_activateability", transform.position);
            (Core as Craft).Respawn();
            Retreat r = Core.GetAbilities().First((a) => { return a is Retreat; }) as Retreat;
            if (r != null)
            {
                r.startTime = Time.time - activeDuration + 0.1f;
            }
        }
    }
}