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
        Transform tmp = ability.Core.GetTargetingSystem().GetTarget(); // get the core's target if it has one

        if (tmp && tmp.GetComponent<Entity>()
            && !tmp.GetComponent<Entity>().GetIsDead()
            && tmp.GetComponent<Entity>().faction != ability.Core.faction 
            && ability.CheckCategoryCompatibility(tmp.GetComponent<Entity>())) // check if target is compatible
        {
            target = tmp;
            return target; // if the manual target is compatible it overrides everything
        }
        else if (findNew) // check if call wants to find a new target
        {
            //Find the closest enemy
            //TODO: optimize
            Entity[] entities = GameObject.FindObjectsOfType<Entity>();
            Transform closest = null;
            float closestD = float.MaxValue;

            for (int i = 0; i < entities.Length; i++) // go through all entities and check them for several factors
            {
                // checks for: if the entity is the ability entity, if it is the same faction as the ability entity, 
                // if it's dead, if it is weapon-compatible

                if (entities[i].transform == ability.Core) // TODO: Do we need this check? Doesn't the faction check cover this?
                    continue;
                if (ability.Core.faction == entities[i].faction)
                {
                    // if(ability as Beam) Debug.Log(entities[i]);
                    continue;
                }
                if (entities[i].GetIsDead())
                {
                    continue;
                }
                if (!ability.CheckCategoryCompatibility(entities[i]))
                    continue;

                // check if it is the closest entity that passed the checks so far

                float sqrD = Vector3.SqrMagnitude(ability.Core.transform.position - entities[i].transform.position);
                if (closest == null || sqrD < closestD)
                {
                    closestD = sqrD;
                    closest = entities[i].transform;
                }
            }
            // set to the closest compatible target
            target = closest;
        }
        return target; // get target
    }
}
