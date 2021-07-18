using System.Collections.Generic;

public class Control : PassiveAbility
{
    const float healthAddition = 200;
    public const float damageAddition = 50;
    public const float baseControlFractionBoost = 0.2F;
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
            {
                continue;
            }

            Core.ControlStacks -= abilityTier;
        }

        base.Deactivate();
        Entity.OnEntitySpawn -= EntitySpawn;
    }

    protected override void Execute()
    {
        for (int i = 0; i < AIData.entities.Count; i++)
        {
            if (!AIData.entities[i].GetIsDead())
            {
                Enhance(AIData.entities[i]);
            }
        }

        Entity.OnEntitySpawn += EntitySpawn;
    }

    void EntitySpawn(Entity entity)
    {
        if (!entity.GetIsDead())
        {
            Enhance(entity);
        }
    }

    void Enhance(Entity entity)
    {
        if (!(entity is Turret) && entity.faction == Core.faction && entity != Core && !boosted.Contains(entity))
        {
            Core.ControlStacks += abilityTier;
            boosted.Add(entity);
        }
    }
}
