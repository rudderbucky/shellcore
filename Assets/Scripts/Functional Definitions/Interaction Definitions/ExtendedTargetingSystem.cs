using System;
using System.Collections.Generic;
using UnityEngine;

public class ExtendedTargetingSystem
{
    //Note - Functions may update with similar health range comparisons I.E. - Two shellcores within 200 points of health of eachother treated as equal.

    public Transform parent;
    public float range;
    private Entity parentEntity;
    private Transform target = null;
    private float parentPositionSqrd;
    private Transform coreTargeting;

    public ExtendedTargetingSystem(Transform parent)
    {
        this.parent = parent;
        parentEntity = parent.GetComponentInParent<Entity>();
        float parentPositionSqrd = parent.position.sqrMagnitude;
        coreTargeting = parentEntity.GetTargetingSystem().GetTarget();
    }

    public Transform ReturnHighestHealth(float range, bool furthest) //Furthest functions on returns select furthest or nearest enemy if of equal health
    {
        var rangeSqrd = Mathf.Pow(range, 2);
        var highestHealth = 0f;

        if (coreTargeting != null && parentEntity is PlayerCore) //Allows player to select an entity to target, otherwise use the targeting system
        {
            return coreTargeting;
        }

        for (int i = 0; AIData.entities.Count > i; i++)
        {
            var scan = AIData.entities[i].transform;
            var scanHealth = scan.GetComponent<Entity>().GetMaxHealth()[0] + scan.GetComponent<Entity>().GetMaxHealth()[1]; //Grab max core and shell
            var scanSqrMagnitude = scan.position.sqrMagnitude;

            Debug.Log(highestHealth);
            if (ValidityCheck(scan.GetComponent<Entity>()) && parentPositionSqrd - scanSqrMagnitude < rangeSqrd)
            {
                if (scanHealth > highestHealth)
                {
                    highestHealth = scanHealth;
                    target = scan;
                }

                if (scanHealth == highestHealth && parentPositionSqrd - scanSqrMagnitude < parentPositionSqrd - target.position.sqrMagnitude && !furthest) //Grab nearest comparison
                {
                    target = scan;
                }
                else if (scanHealth == highestHealth && parentPositionSqrd - scanSqrMagnitude > parentPositionSqrd - target.position.sqrMagnitude) //Grab furthest comparison
                {
                    target = scan;
                }
            }
        }
        return target;
    }

    public Transform ReturnHighestCurrentHealth(float range, bool furthest)
    {
        var rangeSqrd = Mathf.Pow(range, 2);
        var highestHealth = 0f;

        if (coreTargeting != null && parentEntity is PlayerCore)
        {
            return coreTargeting;
        }

        for (int i = 0; AIData.entities.Count > i; i++)
        {
            var scan = AIData.entities[i].transform;
            var scanHealth = scan.GetComponent<Entity>().GetHealth()[0] + scan.GetComponent<Entity>().GetHealth()[1]; //Grab current core and shell
            var scanSqrMagnitude = scan.position.sqrMagnitude;

            if (ValidityCheck(scan.GetComponent<Entity>()) && parentPositionSqrd - scanSqrMagnitude < rangeSqrd)
            {
                if (scanHealth > highestHealth)
                {
                    highestHealth = scanHealth;
                    target = scan;
                }

                if (scanHealth == highestHealth && parentPositionSqrd - scanSqrMagnitude < parentPositionSqrd - target.position.sqrMagnitude && !furthest)
                {
                    target = scan;
                }
                else if (scanHealth == highestHealth && parentPositionSqrd - scanSqrMagnitude > parentPositionSqrd - target.position.sqrMagnitude)
                {
                    target = scan;
                }
            }
        }
        return target;
    }

    public Transform ReturnLoestHealth(float range, bool furthest)
    {
        var rangeSqrd = Mathf.Pow(range, 2);
        var lowestHealth = 0f;

        if (coreTargeting != null && parentEntity is PlayerCore)
        {
            return coreTargeting;
        }

        for (int i = 0; AIData.entities.Count > i; i++)
        {
            var scan = AIData.entities[i].transform;
            var scanHealth = scan.GetComponent<Entity>().GetMaxHealth()[0] + scan.GetComponent<Entity>().GetMaxHealth()[1];
            var scanSqrMagnitude = scan.position.sqrMagnitude;

            if (ValidityCheck(scan.GetComponent<Entity>()) && parentPositionSqrd - scanSqrMagnitude < rangeSqrd)
            {
                if (scanHealth < lowestHealth)
                {
                    lowestHealth = scanHealth;
                    target = scan;
                }

                if (scanHealth == lowestHealth && parentPositionSqrd - scanSqrMagnitude < parentPositionSqrd - target.position.sqrMagnitude && !furthest)
                {
                    target = scan;
                }
                else if (scanHealth == lowestHealth && parentPositionSqrd - scanSqrMagnitude > parentPositionSqrd - target.position.sqrMagnitude)
                {
                    target = scan;
                }
            }
        }
        return target;
    }

    public Transform ReturnLowestCurrentHealth(float range, bool furthest)
    {
        var rangeSqrd = Mathf.Pow(range, 2);
        var lowestHealth = 0f;

        if (coreTargeting != null && parentEntity is PlayerCore)
        {
            return coreTargeting;
        }

        for (int i = 0; AIData.entities.Count > i; i++)
        {
            var scan = AIData.entities[i].transform;
            var scanHealth = scan.GetComponent<Entity>().GetHealth()[0] + scan.GetComponent<Entity>().GetHealth()[1];
            var scanSqrMagnitude = scan.position.sqrMagnitude;

            if (ValidityCheck(scan.GetComponent<Entity>()) && parentPositionSqrd - scanSqrMagnitude < rangeSqrd)
            {
                if (scanHealth < lowestHealth)
                {
                    lowestHealth = scanHealth;
                    target = scan;
                }

                if (scanHealth == lowestHealth && parentPositionSqrd - scanSqrMagnitude < parentPositionSqrd - target.position.sqrMagnitude && !furthest)
                {
                    target = scan;
                }
                else if (scanHealth == lowestHealth && parentPositionSqrd - scanSqrMagnitude > parentPositionSqrd - target.position.sqrMagnitude)
                {
                    target = scan;
                }
            }
        }
        return target;
    }

    public Transform Farthest(float range)
    {
        for (int i = 0; AIData.entities.Count > i; i++)
        {
            var scan = AIData.entities[i].transform;
            var scanSqrMagnitude = scan.position.sqrMagnitude;

            if (parentPositionSqrd - scanSqrMagnitude > parentPositionSqrd - target.position.sqrMagnitude)
            {
                target = scan;
            }
        }
        return target;
    }


    bool ValidityCheck(Entity ent)
    {
        return (!ent.GetIsDead() && !FactionManager.IsAllied(ent.faction, parentEntity.faction) && !ent.IsInvisible);
    }

}