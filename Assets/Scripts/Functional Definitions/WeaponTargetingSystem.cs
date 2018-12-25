using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTargetingSystem {

    public WeaponAbility ability;
    public Transform target;



    /// <summary>
    /// Get the target of the targeting system
    /// </summary>
    /// <param name="findNew">Whether or not the targeting system should find a new target</param>
    /// <returns>The target of the targeting system</returns>
    public Transform GetTarget(bool findNew = false)
    {
        Transform tmp = ability.Core.GetTargetingSystem().GetTarget();
        if (tmp && tmp.GetComponent<Entity>()
            && !tmp.GetComponent<Entity>().GetIsDead()
            && tmp.GetComponent<Entity>().faction != ability.Core.faction 
            && ability.CheckCategoryCompatibility(tmp.GetComponent<Entity>()))
        {
            target = tmp;
            return target; // if the manual target is compatible it overrides everything
        }
        else if (findNew)
        {
            //Find the closest enemy
            //TODO: optimize
            Entity[] entities = GameObject.FindObjectsOfType<Entity>();
            Transform closest = null;
            float closestD = float.MaxValue;

            for (int i = 0; i < entities.Length; i++)
            {
                if (entities[i].transform == ability.Core)
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

                float sqrD = Vector3.SqrMagnitude(ability.Core.transform.position - entities[i].transform.position);
                if (closest == null || sqrD < closestD)
                {
                    closestD = sqrD;
                    closest = entities[i].transform;
                }
            }
            target = closest;
        }
        return target; // get target
    }
}
