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
        if (craft.GetHealth()[0] < craft.GetMaxHealth()[0] * 0.8f)
        {
            var shellBoosts = GetAbilities(2);
            foreach (var booster in shellBoosts)
            {
                booster.Tick("activate");
            }

            if (craft.GetHealth()[0] < craft.GetMaxHealth()[0] * 0.25f)
            {
                var stealths = GetAbilities(24);
                foreach (var stealth in stealths)
                {
                    stealth.Tick("activate");
                    if (stealth.GetActiveTimeRemaining() > 0)
                        break;
                }
            }
        }
    }

    Ability[] GetAbilities(int ID)
    {
        return craft.GetAbilities().Where((x) => { return (x != null) && x.GetID() == ID; }).ToArray();
    }
}
