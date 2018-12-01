using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for objects that can be executed by a handler; mainly used for abilities
/// </summary>
interface IPlayerExecutable {
    void Tick(string key); // the state of this object will change over time
}

/// <summary>
/// A trait that can be activated for a special effect; parts sometimes come with these
/// All weapons have abilities that deal their effect
/// </summary>
public abstract class Ability : MonoBehaviour, IPlayerExecutable {

    public Craft core; // craft that uses this ability
    protected int ID; // Image ID, perhaps also ability ID if that were ever to be useful
    protected float cooldownDuration; // cooldown of the ability
    protected int energyCost; // energy cost of the ability
    protected float CDRemaining; // amount of time remaining on cooldown
    protected bool isOnCD = false; // check for cooldown
    protected bool isPassive = false; // if the ability is passive
    protected bool isEnabled = true; // if the ability is enabled

    /// <summary>
    /// Setter method for isEnabled, will be used by parts
    /// </summary>
    /// <param name="input">boolean to set to</param>
    public void SetIsEnabled(bool input) {
        isEnabled = input; // set is enabled
    }

    /// <summary>
    /// Getter method for isEnabled, will be used by the AbilityHandler
    /// </summary>
    /// <returns>true if ability is enabled, false otherwise</returns>
    public bool GetIsEnabled() {
        return isEnabled; // get is enabled
    }
    /// <summary>
    /// Initialization of every ability
    /// </summary>
    protected virtual void Awake() { }

    /// <summary>
    /// Get core, if it isn't specified elsewhere
    /// </summary>
    void Start()
    {
        if (core == null)
            core = GetComponentInParent<Craft>();
    }

    /// <summary>
    /// Get the isPassive of the ability
    /// </summary>
    /// <returns>the isPassive of the ability</returns>
    public bool GetIsPassive() {
        return isPassive; // is passive
    }

    /// <summary>
    /// Get the image ID of the ability
    /// </summary>
    /// <returns>Image ID of the ability</returns>
    public virtual int GetID() {
        return ID; // ID
    }

    /// <summary>
    /// Get the energy cost of the ability
    /// </summary>
    /// <returns>The energy cost of the ability</returns>
    public int GetEnergyCost() {
        return energyCost; // energy cost
    }

    /// <summary>
    /// Get the cooldown duration of the ability
    /// </summary>
    /// <returns>The cooldown of the ability</returns>
    public float GetCDDuration()
    {
        return cooldownDuration; // cooldown duration
    }

    /// <summary>
    /// Get the active time remaining on the ability; here defined as 0
    /// </summary>
    /// <returns>The active time remaining on the ability</returns>
    public virtual float GetActiveTimeRemaining() {
        return 0; // active time (unless overriden this is 0)
    }

    /// <summary>
    /// Get the cooldown remaining on the ability
    /// </summary>
    /// <returns>The cooldown duration remaining on the ability</returns>
    public float GetCDRemaining() {
        if (isOnCD) // on cooldown
        {
            return CDRemaining; // return the cooldown remaining, calculated prior to this call via TickDown
        }
        else return 0; // not on cooldown
    }

    /// <summary>
    /// Helper method used to update the ability's fields, usually called by Tick every update (not always)
    /// </summary>
    /// <param name="duration">Used to reset remaining; this is its default value</param>
    /// <param name="remaining">Duration to tick down</param>
    /// <param name="checkerVal">The boolean to flip once remaining ticks to 0</param>
    public void TickDown(float duration, ref float remaining, ref bool checkerVal) {
        if (remaining > Time.deltaTime) // tick down
        {
            remaining -= Time.deltaTime; // reduce by time passed since last frame
        }
        else
        {
            remaining = duration; // reset remaining
            checkerVal = false; // flip boolean
        }
    }

    /// <summary>
    /// Ability called to change the ability's state over time
    /// </summary>
    /// <param name="key">The associated button to press to activate</param>
    virtual public void Tick(string key) {
        if (isOnCD) // tick the cooldown down
        {
            TickDown(cooldownDuration, ref CDRemaining, ref isOnCD);  // tick down
        }
        else if (core.GetHealth()[2] >= energyCost && Input.GetKeyDown(key)) // enough energy and button pressed
        {
            core.MakeBusy(); // make core busy
            core.TakeEnergy(energyCost); // remove the energy
            Execute(); // execute the ability
        }
    }

    /// <summary>
    /// Used to activate whatever effect the ability has, almost always overriden
    /// </summary>
    virtual protected void Execute() {
        isOnCD = true; // template to be overriden
    }
}