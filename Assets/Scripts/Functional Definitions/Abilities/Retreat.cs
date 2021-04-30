using System.Linq;
using UnityEngine;

/// <summary>
/// Respawns the entity
/// </summary>
public class Retreat : Ability
{
    float activationDelay = 3f; // the delay between clicking the ability and its activation
    bool charging = false;

    protected override void Awake()
    {
        base.Awake(); // base awake
                      // hardcoded values here
        ID = AbilityID.Retreat;
        energyCost = 200;
        cooldownDuration = 30;
    }

    protected override void Execute()
    {
        AudioManager.PlayClipByID("clip_activateability", transform.position);
        if (Core is Craft)
        {
            (Core as Craft).Respawn();
            Retreat r = Core.GetAbilities().First((a) => { return a is Retreat; }) as Retreat;
            if (r != null)
            {
                r.startTime = Time.time + activationDelay + 0.1f;
            }
        }
    }
}