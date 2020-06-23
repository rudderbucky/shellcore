using UnityEngine;
using System.Collections;
using System.Linq;

public class AIAbilityController
{
    AirCraftAI ai;
    Craft craft;

    public bool useAbilities = true;

    public AIAbilityController(AirCraftAI ai)
    {
        this.ai = ai;
        craft = ai.craft;
    }

    public void Update()
    {
        if (!useAbilities)
            return;

        // Use abilities if needed
        if (!ai.movement.targetIsInRange())
        {
            if (ai.movement.DistanceToTarget > 256f)
            {
                bool allowSpeed = true;
                if (craft.faction == 0 && PlayerCore.Instance != null && !PlayerCore.Instance.GetIsDead())
                {
                    // Don't run away or get behind when escorting a player
                    float ownD = (ai.movement.GetTarget() - (Vector2)craft.transform.position).sqrMagnitude;
                    float playerD = (ai.movement.GetTarget() - (Vector2)PlayerCore.Instance.transform.position).sqrMagnitude;
                    allowSpeed = playerD < ownD;
                }
                if (allowSpeed)
                {
                    var speeds = GetAbilities(1);
                    int half = Mathf.CeilToInt(speeds.Count() / 2f);
                    int count = 0;
                    foreach (var booster in speeds)
                    {
                        booster.Tick("activate");
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
            var shellBoosts = GetAbilities(2, 17, 26); // shell heal, shell regen, area restore
            foreach (var booster in shellBoosts)
            {
                if (craft.GetHealth()[0] > craft.GetMaxHealth()[0] * 0.9f)
                    break;
                booster.Tick("activate");
            }
        }
        if (craft.GetHealth()[0] < craft.GetMaxHealth()[0] * 0.25f)
        {
            var stealths = GetAbilities(24); // stealth, stasis field, pin down
            foreach (var stealth in stealths)
            {
                stealth.Tick("activate");
                if (stealth.GetActiveTimeRemaining() > 0)
                    break;
            }
        }
        if (craft.GetHealth()[0] < craft.GetMaxHealth()[0] * 0.1f)
        {
            var retreats = GetAbilities(); // retreat
            foreach (var retreat in retreats)
            {
                bool CD = retreat.GetCDRemaining() > 0f;
                if (!CD)
                {
                    retreat.Tick("activate");
                    if (retreat.GetCDRemaining() > 0f)
                        break;
                }
            }
        }
        var target = craft.GetTargetingSystem().GetTarget();
        if (target != null && target)
        {
            Entity targetEntity = target.GetComponent<Entity>();
            if (targetEntity != null && targetEntity && !targetEntity.GetIsDead())
            {
                var damageBoosts = GetAbilities(25); // damage boost
                foreach (var damageBoost in damageBoosts)
                {
                    damageBoost.Tick("activate");
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
