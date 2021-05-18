using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Any ability that has a specific active time is an active ability (abilities that are simply click for effect that does not expire are not active abilities)
/// </summary>
public abstract class ActiveAbility : Ability
{
    /// <summary>
    /// Initialization of every active ability
    /// </summary>
    protected override void Awake() {
        base.Awake(); // base awake
        abilityName = "Active Ability";
    }
    /// <summary>
    /// Get the active time remaining
    /// </summary>
    /// <returns>The active time remaining</returns>
    public override float GetActiveTimeRemaining()
    {
        if (State == AbilityState.Active) // active
        {
            return activeDuration - (Time.time - startTime); // return active time remaining
        }
        else return 0; // not active
    }

    public override void SetDestroyed(bool input)
    {
        //if (input && State == AbilityState.Active)
        //    Deactivate();
        base.SetDestroyed(input);
    }

    private void OnDestroy()
    {
        if (State != AbilityState.Destroyed)
            SetDestroyed(true);
    }

    /// <summary>
    /// Called when active time hits 0, used to rollback whatever change was done on the core
    /// </summary>
    override public void Deactivate()
    { 

    }

    override protected void Execute()
    {
        base.Execute();
    }

    /// <summary>
    /// Override on tick that accounts for actives for players
    /// </summary>
    /// <param name="key">Associated string on the button to push to activate</param>
    public override void Tick()
    {
        base.Tick();
    }
}

