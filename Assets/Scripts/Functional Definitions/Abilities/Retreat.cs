using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
            if((Core as ShellCore
                && (Core as ShellCore).GetCarrier() == null) && (!(Core as PlayerCore) || (Core as PlayerCore).havenSpawnPoint == Vector2.zero)) 
                {
                    return;
                }
            AudioManager.PlayClipByID("clip_activateability", transform.position);
            // get all current retreats, set the new retreat's start times to the old retreat start times, find one that is off CD
            // and set it to CD
            List<Retreat> oldRetreats = new List<Retreat>();
            foreach(Ability ability in Core.GetAbilities())
            {
                if(ability as Retreat)
                    oldRetreats.Add(ability as Retreat);
            }

            (Core as Craft).Respawn();

            List<Retreat> newRetreats = new List<Retreat>();
            foreach(Ability ability in Core.GetAbilities())
            {
                if(ability as Retreat)
                    newRetreats.Add(ability as Retreat);
            }
            for(int i = 0; i < oldRetreats.Count; i++)
            {
               newRetreats[i].startTime = oldRetreats[i].startTime; 
            }

            // prior to Awake activation since Respawn was set on this stack. We therefore search for 0 here
            Retreat r = newRetreats.Find((a) => { return a.startTime == 0; }) as Retreat;
            if (r != null)
            {
                r.startTime = Time.time - activeDuration + 0.1f;
            }
        }
    }
}