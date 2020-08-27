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

        // Performance: Don't allow weapon targeting for non-ShellCores
        if(!(ability.Core as ShellCore))
        {
            if(IsValidTarget(tmp)) 
                target = tmp;
            else target = null;
            return target;
        }

        if (tmp != null && tmp && IsValidTarget(tmp))
        {
            target = tmp;
            return target; // if the manual target is compatible it overrides everything
        }

        if (findNew || !IsValidTarget(target)) // check if call wants to find a new target
        {
            //Find the closest enemy
            //TODO: optimize
            Transform closest = null;
            float closestD = float.MaxValue;
            var pos = ability.Core.transform.position;

            for (int i = 0; i < AIData.entities.Count; i++) // go through all entities and check them for several factors
            {
                // checks for: if it is the same faction as the ability entity, 
                // if it's dead, if it is weapon-compatible, if it is invisible

                //if (ability.Core.faction == AIData.entities[i].faction)
                //{
                //    // if(ability as Beam) Debug.Log(entities[i]);
                //    continue;
                //}
                //if (AIData.entities[i].GetIsDead())
                //{
                //    continue;
                //}
                //if (AIData.entities[i].invisible)
                //{
                //    continue;
                //}
                //if (!ability.CheckCategoryCompatibility(AIData.entities[i]))
                //    continue;

                if (!IsValidTarget(AIData.entities[i].transform))
                    continue;

                // check if it is the closest entity that passed the checks so far

                float sqrD = Vector3.SqrMagnitude(pos - AIData.entities[i].transform.position);
                if (closest == null || sqrD < closestD)
                {
                    closestD = sqrD;
                    closest = AIData.entities[i].transform;
                }
            }
            // set to the closest compatible target
            target = closest;
        }

        return target; // return the target
    }

    bool IsValidTarget(Transform t)
    {
        if (t == null || !t)
            return false;
        IDamageable damageable = t.GetComponent<IDamageable>();

        return (damageable != null
            && !damageable.GetIsDead()
            && damageable.GetTransform() != ability.Core.GetTransform()
            && !FactionManager.IsAllied(damageable.GetFaction(), ability.Core.faction)
            && ability.CheckCategoryCompatibility(damageable)
            && (t.position - ability.transform.position).magnitude <= ability.GetRange()
            && !damageable.GetInvisible());
    }
}
