using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A trait that can be activated for a special effect; parts sometimes come with these
/// All weapons have abilities that deal their effect
/// </summary>
public abstract class Ability : MonoBehaviour {

    Entity core;  // craft that uses this ability
    public Entity Core
    {
        get
        {
            if (core == null)
                core = GetComponentInParent<Entity>();
            return core;
        }
        set
        {
            core = value;
        }
    }
    public enum AbilityState
    {
        Ready,
        Charging,
        Active,
        Cooldown,
        Disabled,
        Destroyed
    }

    public AbilityState State = AbilityState.Ready;

    protected AbilityID ID; // Image ID, perhaps also ability ID if that were ever to be useful (it was)
    protected float chargeDuration; // delay before activation
    protected float activeDuration;  // active time ( + charge)
    protected float cooldownDuration; // cooldown of the ability (+ charge and active time)
    protected float energyCost; // energy cost of the ability
    protected float startTime; // the time the ability was activated by the entity
    private Color originalIndicatorColor;
    protected int abilityTier;
    protected string description = "Does things";
    protected ShellPart part;

    public ShellPart Part { set { part = value; } }
    public string abilityName = "Ability";
    public SpriteRenderer glow;

    public bool isEnabled { get; set; } = true;

    public virtual void SetTier(int abilityTier) {
        if(abilityTier > 3 || abilityTier < 0) Debug.LogError("An ability tier was set out of bounds!" + "number: " + abilityTier);
        this.abilityTier = abilityTier;
    }

    public int GetTier() {
        return abilityTier;
    }

    public AbilityHandler.AbilityTypes GetAbilityType()
    {
        return AbilityUtilities.GetAbilityTypeByID((int)ID);
    }

    /// <summary>
    /// Getter method for ability description, will be used by the AbilityHandler
    /// </summary>
    /// <returns>ability description</returns>
    public string GetDescription()
    {
        return description;
    }

    /// <summary>
    /// Getter method for ability name, will be used by the AbilityHandler
    /// </summary>
    /// <returns>ability name</returns>
    public string GetName()
    {
        return abilityName;
    }

    /// <summary>
    /// Setter method for isDestroyed
    /// </summary>
    /// <param name="input">boolean to set to</param>
    virtual public void SetDestroyed(bool input)
    {
        if (input)
        {
            // delete glow from ability
            if (glow)
                Destroy(glow.gameObject);

            if (State == AbilityState.Active)
                Deactivate();
            State = AbilityState.Destroyed;
        }
        else
        {
            State = AbilityState.Ready;
        }
    }

    void OnDestroy()
    {
        if (State != AbilityState.Destroyed && Core)
            SetDestroyed(true);
    }

    /// <summary>
    /// Getter method for isDestroyed
    /// </summary>
    /// <returns>true if ability is destroyed, false otherwise</returns>
    public bool IsDestroyed()
    {
        return State == AbilityState.Destroyed;
    }


    /// <summary>
    /// Initialization of every ability
    /// </summary>
    protected virtual void Awake()
    {
        State = AbilityState.Ready;
        startTime = -100f; // 0 would trigger the ability at start
    }

    /// <summary>
    /// Get the image ID of the ability
    /// </summary>
    /// <returns>Image ID of the ability</returns>
    public virtual int GetID() {
        return (int)ID; // ID
    }

    /// <summary>
    /// Get the energy cost of the ability
    /// </summary>
    /// <returns>The energy cost of the ability</returns>
    public float GetEnergyCost() {
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

    public void ResetCD()
    {
        startTime = Time.time - activeDuration;
    }

    /// <summary>
    /// Get the cooldown remaining on the ability
    /// </summary>
    /// <returns>The cooldown duration remaining on the ability</returns>
    public float TimeUntilReady() {
        if (State == AbilityState.Cooldown || State == AbilityState.Charging || State == AbilityState.Active) // active or on cooldown
        {
            return cooldownDuration - (Time.time - startTime); // return the cooldown remaining, calculated prior to this call via TickDown
        }
        else return 0; // not on cooldown
    }


    /// <summary>
    /// Updates the internal state of the ability
    /// </summary>
    public void UpdateState()
    {
        if (State == AbilityState.Destroyed)
            return;
        if (!isEnabled)
            State = AbilityState.Disabled;
        else if (Time.time >= startTime + cooldownDuration)
            State = AbilityState.Ready;
        else if (Time.time >= startTime + activeDuration)
            State = AbilityState.Cooldown;
        else if (Time.time >= startTime + chargeDuration)
            State = AbilityState.Active;
        else
            State = AbilityState.Charging;

        if (!Core || Core.GetIsDead())
        {
            State = AbilityState.Destroyed;
            Debug.Log($"Destroyed!! Core: {Core}, IsDead: {Core?.GetIsDead()}");
        }
    }

    public virtual void Activate()
    {
        // If (NPC or (Player and not interacting)) and enough energy
        if (State == AbilityState.Ready && (!(Core as PlayerCore) || !(Core as PlayerCore).GetIsInteracting()) && Core.GetHealth()[2] >= energyCost)
        {
            Core.MakeBusy(); // make core busy
            Core.TakeEnergy(energyCost); // remove the energy
            startTime = Time.time; // Set activation time
            UpdateState(); // Update state
            // If there's no charge time, execute immediately
            if (State == AbilityState.Active || State == AbilityState.Cooldown)
                Execute();
        }
    }

    /// <summary>
    /// Ability called to change the ability's state over time for players
    /// </summary>
    /// <param name="action">The associated button to press to activate</param>
    virtual public void Tick() {
        if(State == AbilityState.Destroyed)
        {
            return; // Part has been destroyed, ability can't be used
        }

        AbilityState prevState = State;
        UpdateState();

        UpdateBlinker();

        // If ability activated
        if (State == AbilityState.Active && prevState == AbilityState.Charging)
        {
            Execute(); // execute the ability
        }
        // If the ability needs to cool down
        else if (State == AbilityState.Cooldown && prevState != AbilityState.Cooldown)
        {
            Deactivate(); // deactivate the ability
        }
    }

    void UpdateBlinker()
    {
        if (State == AbilityState.Charging)
        {
            // Make sure the glow exists
            GetBlinker();
        }
        else if (State == AbilityState.Active)
        {
            // Do not reveal stealth enemies by blinking their parts
            if (ID == AbilityID.Stealth && core.faction != 0)
            {
                if (glow)
                    Destroy(glow.gameObject);
                return;
            }

            var blinker = GetBlinker();

            // Blink
            blinker.enabled = (Time.time % 0.25f) > 0.125f;

            // Update alpha
            Color newColor = glow.color;
            if (Core.invisible)
            {
                // Invisible player
                if (Core.faction == 0)
                    newColor.a = 0.1f;
                // Invisible enemy
                else
                    newColor.a = 0f;
            }
            // Visible entity
            else
                newColor.a = 0.5f;

            glow.color = newColor;
        }
        else if(glow)
                Destroy(glow.gameObject);
    }

    protected SpriteRenderer GetBlinker()
    {
        if (!glow)
        {
            var glowPrefab = ResourceManager.GetAsset<GameObject>("glow_prefab");
            if (glowPrefab)
            {
                var obj = Instantiate(glowPrefab, transform, false);
                obj.transform.localScale = new Vector3(0.75F, 0.75F, 1);
                glow = obj.GetComponent<SpriteRenderer>();
                glow.color = new Color(1, 1, 1, 0.5F);
            }
        }
        return glow;
    }

    /// <summary>
    /// Used to activate whatever effect the ability has, almost always overriden
    /// </summary>
    virtual protected void Execute()
    {
        
    }

    /// <summary>
    /// Remove effects of an ability
    /// </summary>
    virtual public void Deactivate()
    {

    }
}
