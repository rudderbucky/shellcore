using UnityEngine;
using System.Collections;
using System.Linq;

public class AIAbilityController
{
    AirCraftAI ai;
    Craft craft;

    public bool useAbilities = true;

    float timer = 0f;
    float interval = 0.25f;
    public float nextStealth = 0f;
    float nextPin = 0f;

    public AIAbilityController(AirCraftAI ai)
    {
        this.ai = ai;
        craft = ai.craft;
    }

    public void Update()
    {
        // TODO: timers & member boolean variables to reduce checks each frame

        if (!useAbilities)
            return;

        if (timer > Time.time)
            return;
        timer = Time.time + interval;

        // Use abilities if needed
        if (!ai.movement.targetIsInRange())
        {
            if (ai.movement.DistanceToTarget > 5f)
            {
                bool allowSpeed = true;
                if (craft.faction == 0 && PlayerCore.Instance != null && !PlayerCore.Instance.GetIsDead()
                    && ai.movement.GetTarget() != null)
                {
                    // Don't run away or get behind when escorting a player
                    float ownD = ((Vector2)ai.movement.GetTarget() - (Vector2)craft.transform.position).sqrMagnitude;
                    float playerD = ((Vector2)ai.movement.GetTarget() - (Vector2)PlayerCore.Instance.transform.position).sqrMagnitude;
                    allowSpeed = playerD < ownD;
                }
                if (allowSpeed)
                {
                    var speeds = GetAbilities(1);
                    int half = Mathf.CeilToInt(speeds.Count() / 2f);
                    int count = 0;
                    foreach (var booster in speeds)
                    {
                        booster.Activate();
                        if (booster.GetActiveTimeRemaining() > 0)
                        {
                            if (++count >= half)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        if (craft.GetHealth()[0] < craft.GetMaxHealth()[0] * 0.8f)
        {
            var shellBoosts = GetAbilities(2, 17, 26, 29, 30, 31); // shell heal, shell regen, area restore
            foreach (var booster in shellBoosts)
            {
                if (craft.GetHealth()[0] > craft.GetMaxHealth()[0] * 0.9f)
                    break;
                booster.Activate();
            }
        }
        if (craft.GetHealth()[0] < craft.GetMaxHealth()[0] * 0.25f && Time.time > nextStealth)
        {
            var escapeAbilities = GetAbilities(24, 29, 27); // stealth, absorption, pin down
            foreach (var escapeAbility in escapeAbilities)
            {
                escapeAbility.Activate();
                if (escapeAbility.GetActiveTimeRemaining() > 0f)
                {
                    nextStealth = Time.time + escapeAbility.GetActiveTimeRemaining() + 1.0f;
                    break;
                }
            }
        }
        if (craft.GetHealth()[0] < craft.GetMaxHealth()[0] * 0.2f)
        {
            var retreats = GetAbilities(28); // retreat
            foreach (var retreat in retreats)
            {
                bool CD = retreat.TimeUntilReady() > 0f;
                if (!CD)
                {
                    retreat.Activate();
                    if (retreat.TimeUntilReady() > 0f)
                        break;
                }
            }
        }
        if (craft.GetHealth()[1] < craft.GetMaxHealth()[1] * 0.5f)
        {
            var core = GetAbilities(11, 31); // core heal & regen
            foreach (var ability in core)
            {
                ability.Activate();
                if (ability.GetActiveTimeRemaining() > 0)
                    break;
            }
        }
        if (craft.GetHealth()[2] < craft.GetMaxHealth()[2] * 0.5f)
        {
            var energy = GetAbilities(12, 32); // energy add & regen
            foreach (var ability in energy)
            {
                ability.Activate();
                if (ability.GetActiveTimeRemaining() > 0)
                    break;
            }
        }
        var target = craft.GetTargetingSystem().GetTarget();
        if (target != null && target)
        {
            Entity targetEntity = target.GetComponent<Entity>();
            if (targetEntity != null && targetEntity && !targetEntity.GetIsDead())
            {
                var damageBoosts = GetAbilities(25, 33); // damage boost, disrupt
                foreach (var damageBoost in damageBoosts)
                {
                    damageBoost.Activate();
                }
                // TODO: use only if the enemy is close enough!
                var pinDown = GetAbilities(27); // pin down
                if (Time.time > nextPin)
                {
                    foreach (var pin in pinDown)
                    {
                        if (pin.GetActiveTimeRemaining() <= 0)
                        {
                            pin.Activate();
                            if (pin.GetActiveTimeRemaining() > 0f)
                            {
                                nextPin = Time.time + pin.GetActiveTimeRemaining() -1.5f; // 2 sec activation time, leave 0.5 sec for fleeing
                                break;
                            }
                        }
                    }
                }
            }
        }
        if (craft is IOwner && target != null && target)
        {
            IOwner owner = craft as IOwner;

            if (owner.GetUnitsCommanding().Count < owner.GetTotalCommandLimit())
            {
                var droneSpawns = GetAbilities(10); // drone spawn
                foreach (var droneSpawn in droneSpawns)
                {
                    droneSpawn.Activate();
                }
            }
        }
    }

    Ability[] GetAbilities(params int[] IDs)
    {
        return craft.GetAbilities().Where((x) => { return (x != null) && IDs.Contains(x.GetID()); }).ToArray();
    }
    Ability[] GetAbilities(int ID)
    {
        return craft.GetAbilities().Where((x) => { return (x != null) && x.GetID() == ID; }).ToArray();
    }
}
