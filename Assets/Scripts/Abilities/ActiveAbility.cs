using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Any ability that has a specific active time is an active ability (abilities that are simply click for effect that does not expire are not active abilities)
/// </summary>
public abstract class ActiveAbility : Ability
{
    protected bool isActive = false; // used to check if the ability is active
    protected float activeTimeRemaining; // how much active time is remaining on the ability
    protected int activeDuration; // the duration it is active for

    /// <summary>
    /// Initialization of every active ability
    /// </summary>
    protected override void Awake() {
        base.Awake(); // base awake
    }
    /// <summary>
    /// Get the active time remaining
    /// </summary>
    /// <returns>The active time remaining</returns>
    public override float GetActiveTimeRemaining()
    {
        if (isActive) // active
        {
            return activeTimeRemaining; // return active time remaining
        }
        else return 0; // not active
    }

    /// <summary>
    /// Called when active time hits 0, used to rollback whatever change was done on the core
    /// </summary>
    virtual protected void Deactivate() { }

    /// <summary>
    /// Overrie on tick that accounts for actives
    /// </summary>
    /// <param name="key">Associated string on the button to push to activate</param>
    public override void Tick(string key)
    {
        if (isActive) // active
        {
            TickDown(activeDuration, ref activeTimeRemaining, ref isActive); // tick the active time
            if (!isActive) // if the boolean got flipped deactivate
            {
                Deactivate(); // deactivate
            }
        }
        if (isOnCD) // on cooldown
        {
            TickDown(cooldownDuration, ref CDRemaining, ref isOnCD); // tick the cooldown time
        }
        else if (!isActive) { // if not active it can run through the base ability behaviour
            base.Tick(key); // base tick
        }
    }
}

