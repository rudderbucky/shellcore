using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The targeting system of each weapon ability
/// </summary>
public class WeaponTargetingSystem {

    public WeaponAbility ability; // owner ability of the targeting system
    public Transform target; // target of the targeting system

    /// <summary>
    /// Get the target of the targeting system
    /// </summary>
    /// <param name="findNew">Whether or not the targeting system should find a new target</param>
    /// <returns>The target of the targeting system</returns>
    public Transform GetTarget(bool findNew = false)
    {
        if(DialogueSystem.isInCutscene) return null; // TODO: remove the hack and prevent weapons from firing somehow else

        Transform tmp = ability.Core.GetTargetingSystem().GetTarget(); // get the core's target if it has one

        if (tmp && tmp.GetComponent<IDamageable>() != null
            && !tmp.GetComponent<IDamageable>().GetIsDead()
            && tmp.GetComponent<IDamageable>().GetFaction() != ability.Core.faction 
            && ability.CheckCategoryCompatibility(tmp.GetComponent<IDamageable>())) // check if target is compatible
        {
            target = tmp;
            return target; // if the manual target is compatible it overrides everything
        }
        else if (findNew) // check if call wants to find a new target
        {
            //Find the closest enemy
            //TODO: optimize
            Transform closest = null;
            float closestD = float.MaxValue;

            for (int i = 0; i < AIData.entities.Count; i++) // go through all entities and check them for several factors
            {
                // checks for: if it is the same faction as the ability entity, 
                // if it's dead, if it is weapon-compatible

                if (ability.Core.faction == AIData.entities[i].faction)
                {
                    // if(ability as Beam) Debug.Log(entities[i]);
                    continue;
                }
                if (AIData.entities[i].GetIsDead())
                {
                    continue;
                }
                if (!ability.CheckCategoryCompatibility(AIData.entities[i]))
                    continue;

                // check if it is the closest entity that passed the checks so far

                float sqrD = Vector3.SqrMagnitude(ability.Core.transform.position - AIData.entities[i].transform.position);
                if (closest == null || sqrD < closestD)
                {
                    closestD = sqrD;
                    closest = AIData.entities[i].transform;
                }
            }
            // set to the closest compatible target
            target = closest;
        }
        return target; // get target
    }
}
