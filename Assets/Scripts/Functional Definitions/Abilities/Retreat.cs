using System.Collections.Generic;
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

    public override void ActivationCosmetic(Vector3 targetPos)
    {
        base.ActivationCosmetic(targetPos);
        Execute();
    }

    protected override void Execute()
    {
        if (Core is Craft)
        {
            if (Core is ShellCore shellCore && shellCore.GetCarrier() == null)
            {
                if (!(Core is PlayerCore))
                    return;
                else if (Core is PlayerCore && (DialogueSystem.isInCutscene || PlayerCore.Instance.GetIsInteracting() || DialogueSystem.Instance.IsWindowActive()))
                    return;
            }
            if (Core && Core.networkAdapter && !MasterNetworkAdapter.lettingServerDecide)
            {
                Core.networkAdapter.ExecuteAbilityCosmeticClientRpc(part ? part.info.location : Vector2.zero, part ? part.transform.position : Vector3.zero);
            }


            AudioManager.PlayClipByID("clip_activateability", transform.position);
            // get all current retreats, set the new retreat's start times to the old retreat start times, find one that is off CD
            // and set it to CD
            List<Retreat> oldRetreats = new List<Retreat>();
            foreach (Ability ability in Core.GetAbilities())
            {
                if (ability && !ability.IsDestroyed() && ability is Retreat retreat)
                {
                    oldRetreats.Add(retreat);
                }
            }

            (Core as Craft).Respawn();

            List<Retreat> newRetreats = new List<Retreat>();
            foreach (Ability ability in Core.GetAbilities())
            {
                if (ability && !ability.IsDestroyed() && ability is Retreat retreat)
                {
                    newRetreats.Add(retreat);
                    retreat.abilityIsReadyOnServer = false;
                }
            }

            for (int i = 0; i < oldRetreats.Count; i++)
            {
                newRetreats[i].startTime = oldRetreats[i].startTime;
            }

            // prior to Awake activation since Respawn was set on this stack. We therefore search for 0 here
            Retreat r = newRetreats.Find((a) => { return a.startTime == 0; });
            if (r != null)
            {
                r.startTime = Time.time - activeDuration + 0.1f;
            }
        }
    }
}
