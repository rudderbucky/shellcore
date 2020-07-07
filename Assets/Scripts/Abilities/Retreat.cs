using System.Linq;
using UnityEngine;

/// <summary>
/// Heals all allies in range
/// </summary>
public class Retreat : Ability
{
    float activationDelay = 3f; // the delay between clicking the ability and its activation
    float activationTime = 0f;
    bool charging = false;

    protected override void Awake()
    {
        base.Awake(); // base awake
                      // hardcoded values here
        ID = 28;
        energyCost = 200;
        cooldownDuration = 30;
        CDRemaining = cooldownDuration;
    }

    public override void Tick(int key)
    {
        base.Tick(key);
        if (isOnCD && Time.time > activationTime && charging)
        {
            charging = false;
            AudioManager.PlayClipByID("clip_activateability", transform.position);
            if (Core is Craft)
            {
                (Core as Craft).Respawn();
                Retreat r = Core.GetAbilities().First((a) => { return a is Retreat; }) as Retreat;
                if (r != null)
                {
                    r.isOnCD = true;
                    r.CDRemaining = r.cooldownDuration - activationDelay;
                }
            }
        }
    }

    /// <summary>
    /// Heals all nearby allies
    /// </summary>
    protected override void Execute()
    {
        activationTime = Time.time + activationDelay;
        isOnCD = true;
        charging = true;
        ToggleIndicator(false);
    }
}