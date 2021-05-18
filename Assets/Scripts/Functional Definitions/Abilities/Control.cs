using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control : PassiveAbility
{

    const float healthAddition = 200;
    const float damageAddition = 200;
    List<Entity> boosted = new List<Entity>();

    protected override void Awake()
    {
        base.Awake();
        ID = AbilityID.Control;
    }

    public override void Deactivate()
    {
        for (int i = 0; i < boosted.Count; i++)
        {
            var entity = boosted[i];
            if (!entity)
                continue;
            var maxHealths = entity.GetMaxHealth();
            maxHealths[0] -= healthAddition * abilityTier;

            // Remove a percentage of health from the ship, based on what max health the core had before destruction
            var healths = entity.GetHealth();
            healths[0] -= (healths[0] / maxHealths[0]) * healthAddition * abilityTier;
            entity.damageAddition -= damageAddition;
        }
        base.Deactivate();
        Entity.OnEntitySpawn -= EntitySpawn;
    }

    protected override void Execute()
    {
        for (int i = 0; i < AIData.entities.Count; i++)
        {
            Enhance(AIData.entities[i]);
        }
        Entity.OnEntitySpawn += EntitySpawn;
    }

    void EntitySpawn(Entity entity)
    {
        Enhance(entity);
    }

    void Enhance(Entity entity)
    {
        if (!(entity is Turret) && entity.faction == Core.faction && entity != Core && !boosted.Contains(entity))
        {
            var maxHealths = entity.GetMaxHealth();
            maxHealths[0] += healthAddition * abilityTier;
            var healths = entity.GetHealth();
            healths[0] += healthAddition * abilityTier;
            entity.damageAddition += damageAddition;
            boosted.Add(entity);
        }
    }
}
