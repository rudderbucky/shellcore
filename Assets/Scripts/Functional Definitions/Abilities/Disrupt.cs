using System.Linq;
using UnityEngine;

/// <summary>
/// Resets active cooldowns of nearby enemies
/// </summary>
public class Disrupt : Ability
{
    const float range = 100f;

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
            if (AIData.entities[i] is Craft && !AIData.entities[i].GetIsDead() && !FactionManager.IsAllied(AIData.entities[i].faction, Core.faction))
            {
                float d = (Core.transform.position - AIData.entities[i].transform.position).sqrMagnitude;
                if (d < range)
                {
                    foreach (var ability in AIData.entities[i].GetAbilities())
                    {
                        if (ability != null && ability.TimeUntilReady() > 0)
                        {
                            ability.ResetCD();
                        }
                    }
                }
            }
        }
        base.Execute();
    }
}