﻿using System.Collections.Generic;
using UnityEngine;

public interface ITargetingSystem
{
    Transform GetTarget();
    void SetTarget(Transform t);
    Entity GetEntity();
    WeaponAbility GetAbility();
}

/// <summary>
/// Class used for the targeting of crafts for weapon abilities
/// </summary>
public class TargetingSystem : ITargetingSystem
{
    private Transform target; // the transform of the target
    public Transform parent; // parent object
    int faction;
    Entity ent;

    /// <summary>
    /// Constructor that sets the target to null and takes a transform from which distances are calculate from
    /// </summary>
    public TargetingSystem(Transform parent)
    {
        // initialize instance fields
        target = null;
        this.parent = parent;
        faction = GetEntity().faction;

        // sync up the reticle representations with the new targeting system
        if (GetEntity() as PlayerCore && ReticleScript.instance)
        {
            ReticleScript.instance.ClearSecondaryTargets();
        }
    }

    /// <summary>
    /// Set the target of the targeting system
    /// </summary>
    /// <param name="target">The target to set to</param>
    public void SetTarget(Transform target)
    {
        this.target = target; // set target
    }

    /// <summary>
    /// Get the target of the targeting system
    /// </summary>
    /// <param name="findNew">Whether or not the targeting system should find a new target</param>
    /// <returns>The target of the targeting system</returns>
    public Transform GetTarget()
    {
        return target; // get target
    }

    private List<Entity> secondaryTargets = new List<Entity>();

    public bool AddSecondaryTarget(Entity ent)
    {
        if (!secondaryTargets.Contains(ent))
        {
            secondaryTargets.Insert(secondaryTargets.Count, ent);

            return true;
        }

        return false;
    }

    public void RemoveSecondaryTarget(Entity ent)
    {
        if (secondaryTargets.Contains(ent))
        {
            secondaryTargets.Remove(ent);
        }
    }

    public List<Entity> GetSecondaryTargets()
    {
        return secondaryTargets;
    }

    public void ClearSecondaryTargets()
    {
        secondaryTargets.Clear();
    }

    public Entity GetEntity()
    {
        if (ent == null && parent)
        {
            ent = parent.GetComponent<Entity>();
        }

        return ent;
    }

    public WeaponAbility GetAbility()
    {
        return null;
    }
}
