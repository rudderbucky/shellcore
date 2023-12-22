using System.Collections.Generic;
using UnityEngine;
using static Entity;

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
    Entity ent;

    /// <summary>
    /// Constructor that sets the target to null and takes a transform from which distances are calculate from
    /// </summary>
    public TargetingSystem(Transform parent)
    {
        // initialize instance fields
        target = null;
        this.parent = parent;

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
        if (!MasterNetworkAdapter.lettingServerDecide 
        || !(GetEntity() as PlayerCore) || !GetEntity().networkAdapter) return;
        if (target == null)
        {
            GetEntity().networkAdapter.RequestTargetChangeServerRpc(ulong.MaxValue);
            return; 
        }
        var ent = target.GetComponent<Entity>();
        if (!ent || !ent.networkAdapter) return;
        GetEntity().networkAdapter.RequestTargetChangeServerRpc(ent.networkAdapter.NetworkObjectId); 
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

    private List<Transform> secondaryTargets = new List<Transform>();

    public bool AddSecondaryTarget(Transform ent)
    {
        if (!secondaryTargets.Contains(ent))
        {
            secondaryTargets.Insert(secondaryTargets.Count, ent);

            return true;
        }

        return false;
    }

    public void RemoveSecondaryTarget(Transform ent)
    {
        if (secondaryTargets.Contains(ent))
        {
            secondaryTargets.Remove(ent);
        }
    }

    public List<Transform> GetSecondaryTargets()
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
