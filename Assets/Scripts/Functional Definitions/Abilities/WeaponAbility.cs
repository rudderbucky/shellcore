using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum WeaponDiversityType
{
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
public abstract class WeaponAbility : ActiveAbility
{
    protected float range; // the range of the ability
    protected float damage;
    protected WeaponTargetingSystem targetingSystem;
    public Entity.TerrainType terrain = Entity.TerrainType.Unset;
    public Entity.EntityCategory category = Entity.EntityCategory.All;
    public WeaponDiversityType type = WeaponDiversityType.None;
    protected System.Type bonusDamageType = null;
    protected float bonusDamageMultiplier = 2f;

    public string GetBonusDamageType()
    {
        if (bonusDamageType == typeof(AirConstruct))
        {
            return "Air Stations and Turrets";
        }

        if (bonusDamageType == typeof(GroundConstruct))
        {
            return "Ground Stations";
        }

        if (bonusDamageType == typeof(ShellCore))
        {
            return "ShellCores";
        }

        if (bonusDamageType == typeof(Drone))
        {
            return "Drones";
        }

        return bonusDamageType?.ToString();
    }

    public bool CheckCategoryCompatibility(IDamageable entity)
    {
        return CheckCategoryCompatibility(entity.GetTerrain(), entity.GetCategory());
    }

    public bool CheckCategoryCompatibility(Entity.TerrainType terrain, Entity.EntityCategory category)
    {
        if (type == WeaponDiversityType.Torpedo)
        {
            return terrain == Entity.TerrainType.Ground;
        }
        else
        {
            return TerrainCheck(terrain)
                   && CategoryCheck(category);
        }
    }

    public bool TerrainCheck(Entity.TerrainType targetTerrain)
    {
        if (type == WeaponDiversityType.Torpedo)
        {
            return targetTerrain == Entity.TerrainType.Ground;
        }

        return this.terrain == Entity.TerrainType.All || targetTerrain == this.terrain;
    }

    public bool CategoryCheck(Entity.EntityCategory targetCategory)
    {
        return this.category == Entity.EntityCategory.All || this.category == targetCategory;
    }

    public Transform GetTarget()
    {
        return targetingSystem.GetTarget();
    }

    protected override void Awake()
    {
        base.Awake();
        isEnabled = true; // initialize abilities to be active
        targetingSystem = new WeaponTargetingSystem();
        targetingSystem.ability = this;
        if (abilityName == null)
        {
            abilityName = "Weapon Ability";
        }
    }

    List<AbilityID> damageUnaffectedByTier = new List<AbilityID> { AbilityID.MainBullet };

    protected override void Start()
    {
        if (abilityTier != 0)
        {
            damage *= (damageUnaffectedByTier.Contains(ID)) ? 1 : abilityTier;
        }

        switch (type)
        {
            case WeaponDiversityType.Strike:
                // strike only multiplies tier 1 energy cost
                energyCost *= 0.6F;
                break;
            case WeaponDiversityType.Gun:
                cooldownDuration *= 0.8F;
                break;
            default:
                break;
        }
        base.Start();
    }

    /// <summary>
    /// Get the range of the weapon ability
    /// </summary>
    /// <returns>the range of the weapon ability</returns>
    public override float GetRange()
    {
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
                return (damage + Core.GetDamageAddition()) * bonusDamageMultiplier;
            }
        }

        return damage + Core.GetDamageAddition();
    }

    /// <summary>
    /// Override for active time remaining, just returns a value that is never greater or equal than zero if the ability is active
    /// and zero if it is not
    /// </summary>
    /// <returns>a float value that is directly based on isActive rather than a duration</returns>
    public override float GetActiveTimeRemaining()
    {
        if (isEnabled)
        {
            return -1; // -1 is not zero so the ability is active
        }
        else
        {
            return 0; // inactive ability
        }
    }

    public override void Activate()
    {
        if (Core as PlayerCore)
            isEnabled = !isEnabled;
        UpdateState();
        Core.MakeBusy(); // make core busy
        if (DevConsoleScript.networkEnabled && !(Core is PlayerCore) && NetworkManager.Singleton && NetworkManager.Singleton.IsServer)
        {
            Shoot();
        }
    }

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
        if (State != AbilityState.Ready || Core.GetHealth()[2] < energyCost || Core.GetIsDead()) return;
        Transform target = targetingSystem.GetTarget();
        if (target == null || !target || target.GetComponent<IDamageable>().GetIsDead() || !DistanceCheck(target))
        {
            TargetManager.Enqueue(targetingSystem, category);
        }
        else if (target && target.GetComponent<IDamageable>() != null)
        {
            // check if there is a target
            Core.SetIntoCombat(); // now in combat
            IDamageable tmp = target.GetComponent<IDamageable>();

            // check if allied
            if (FactionManager.IsAllied(tmp.GetFaction(), Core.faction)) return;
            if (!targetingSystem.GetTarget() || !Core.RequestGCD()) return;
            if (!DevConsoleScript.networkEnabled || (NetworkManager.Singleton.IsServer))
            {
                if (!Execute(target.position)) return;
            }
            else if (Core.protobuf)
            {
                Core.protobuf.ExecuteAbilityServerRpc(part ? part.info.location : Vector2.zero, target.position);
            }
            Core.TakeEnergy(energyCost); // take energy, if the ability was executed
            startTime = Time.time;
        }
    }

    // What immediately happens when a weapon is fired
    public virtual void ActivationCosmetic(Vector3 targetPos)
    {

    }


    protected virtual bool DistanceCheck(Transform targetEntity)
    {
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

    protected virtual List<Transform> GetClosestTargets(int num, Vector3 pos)
    {
        List<Entity> potentialTargets = TargetManager.GetTargetList(targetingSystem, category);
        List<Transform> targets = new List<Transform>();
        // Just get the N closest entities, the complexity is just O(N) instead of sorting which would be O(NlogN)
        for (int i = 0; i < num; i++)
        {
            var target = TargetManager.GetClosestFromList(potentialTargets, pos, targetingSystem, category);
            if (target != null)
            {
                potentialTargets.Remove(target.GetComponentInChildren<Entity>());
                targets.Add(target);
            }
            else
            {
                break;
            }
        }
        return targets;
    }

    protected List<Transform> GetClosestTargets(int num)
    {
        return GetClosestTargets(num, targetingSystem.GetEntity().transform.position);
    }

}
