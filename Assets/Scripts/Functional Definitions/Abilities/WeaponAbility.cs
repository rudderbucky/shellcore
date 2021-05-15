using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponDiversityType {
    Strike,
    Gun,
    
    Torpedo,
    None
}


/// <summary>
/// Every ability that is used explicitly to attack other crafts is a weapon ability. 
/// These all have a respective range at which they are effective as well.
/// Their active status also does not depend on a duration and can be directly toggled on and off by the craft.
/// </summary>
public abstract class WeaponAbility : ActiveAbility {

    protected float range; // the range of the ability
    protected float damage;
    protected WeaponTargetingSystem targetingSystem;
    public Entity.TerrainType terrain = Entity.TerrainType.Unset;
    public Entity.EntityCategory category = Entity.EntityCategory.All;
    public WeaponDiversityType type = WeaponDiversityType.None;
    protected System.Type bonusDamageType = null;
    protected float bonusDamageMultiplier = 2f;

    public bool CheckCategoryCompatibility(IDamageable entity)
    {
        if(type == WeaponDiversityType.Torpedo)
            return entity.GetTerrain() == Entity.TerrainType.Ground;
        else
            return (category == Entity.EntityCategory.All || category == entity.GetCategory())
            && (terrain == Entity.TerrainType.All || terrain == entity.GetTerrain());
    }

    public Transform GetTarget()
    {
        return targetingSystem.target;
    }

    protected override void Awake()
    {
        isEnabled = true; // initialize abilities to be active
        targetingSystem = new WeaponTargetingSystem();
        targetingSystem.ability = this;
        if(abilityName == null) abilityName = "Weapon Ability";
    }

    List<AbilityID> damageUnaffectedByTier = new List<AbilityID> {AbilityID.MainBullet, AbilityID.Bomb};

    protected virtual void Start() {
        if(abilityTier != 0) 
        {
            damage *= (damageUnaffectedByTier.Contains(ID)) ? 1 : abilityTier;
            energyCost *= abilityTier;
        }

        switch(type) {
            case WeaponDiversityType.Strike:
                energyCost *= 0.6F;
                break;
            case WeaponDiversityType.Gun:
                cooldownDuration *= 0.6F;
                break;
            default:
                break;
        }
    }
    
    /// <summary>
    /// Get the range of the weapon ability
    /// </summary>
    /// <returns>the range of the weapon ability</returns>
    public float GetRange() {
        return range; // get range
    }

    /// <summary>
    /// Enable / disable the weapon ability
    /// </summary>
    public void SetActive(bool active)
    {
        isEnabled = active;
        State = AbilityState.Ready;
    }

    /// <summary>
    /// Calculates the weapon damage based on the target
    /// </summary>
    /// <returns>damage</returns>
    protected float GetDamage()
    {
        if (GetTarget() != null && bonusDamageType != null && GetTarget().GetComponent<Entity>())
        {
            if (bonusDamageType.IsAssignableFrom(GetTarget().GetComponent<Entity>().GetType()))
            {
                return (damage + Core.damageAddition) * bonusDamageMultiplier;
            }
        }
        return damage + Core.damageAddition;
    }

    /// <summary>
    /// Override for active time remaining, just returns a value that is never greater or equal than zero if the ability is active
    /// and zero if it is not
    /// </summary>
    /// <returns>a float value that is directly based on isActive rather than a duration</returns>
    public override float GetActiveTimeRemaining()
    {
        if (isEnabled) return -1; // -1 is not zero so the ability is active
        else return 0; // inactive ability
    }

    public override void Activate()
    {
        isEnabled = !isEnabled;
        UpdateState();
        Core.MakeBusy(); // make core busy
    }

    /// <summary>
    /// Override for tick that integrates the targeting system of the core for players
    /// and adjusted for the new isActive behaviour
    /// </summary>
    public override void Tick()
    {
        if (State == AbilityState.Destroyed)
        {
            return; // Part has been destroyed, ability can't be used
        }
        if (Core.IsInvisible || Core.isAbsorbing)
        {
            return; // Core is in stealth mode, weapons are disabled
        }
        
        UpdateState(); // Update state

        Shoot();
    }

    void Shoot()
    {
        if (State == AbilityState.Ready && Core.GetHealth()[2] >= energyCost && !Core.GetIsDead()) // if energy is sufficient, core isn't dead and key is pressed
        {
            Transform target = targetingSystem.GetTarget();
            if (target == null || !target || target.GetComponent<IDamageable>().GetIsDead() || !DistanceCheck(target))
            {
                TargetManager.Enqueue(targetingSystem, category);
            }
            else if (target && target.GetComponent<IDamageable>() != null)
            { // check if there is a target
                Core.SetIntoCombat(); // now in combat
                IDamageable tmp = target.GetComponent<IDamageable>();

                // check if allied
                if (!FactionManager.IsAllied(tmp.GetFaction(), Core.faction))
                {
                    if (targetingSystem.GetTarget() && Core.RequestGCD())
                    {
                        bool success = Execute(target.position); // execute ability using the position to fire
                        if (success)
                        {
                            Core.TakeEnergy(energyCost); // take energy, if the ability was executed
                            startTime = Time.time;
                        }
                    }
                }
            }
        }
    }

    protected virtual bool DistanceCheck(Transform targetEntity) {
        return Vector2.SqrMagnitude(transform.position - targetEntity.position) <= GetRange() * GetRange();
    }

    /// <summary>
    /// Unused override for weapon ability, use the position-overloaded Execute() override instead
    /// </summary>
    protected override void Execute()
    {
        Debug.Log("no argument execute is called on a weapon ability!");
        // not supposed to be called, log debug message
    }

    /// <summary>
    /// Virtual Execute() overload for weapon ability
    /// </summary>
    /// <param name="victimPos">The position to execute the ability to</param>
    /// <returns> whether or not the action was executed </returns>
    protected virtual bool Execute(Vector3 victimPos)
    {
        base.Execute();
        return true;
    }
}
