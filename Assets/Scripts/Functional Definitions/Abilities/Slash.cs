using UnityEngine;

/// <summary>
/// 
public class Slash : Ability
{
    Craft target;
    const float range = 5f;
    float rangeSquared = range * range;
    public static readonly int slashDamage = 375;

    public override float GetRange()
    {
        return range;
    }

    private GameObject slashPrefab;
    
    protected override void Awake()
    {
        base.Awake();
        abilityName = "Slash";
        energyCost = 35;
        ID = AbilityID.Slash;
        cooldownDuration = 5;
    }

    protected override bool ExtraCriteriaToActivate()
    {
        return !Core.IsInvisible;
    }

    public override void ActivationCosmetic(Vector3 targetPos)
    {
        slashPrefab = ResourceManager.GetAsset<GameObject>("slash_effect");
        AudioManager.PlayClipByID("clip_slash", targetPos);
        Instantiate(slashPrefab, Core.transform);
        base.ActivationCosmetic(targetPos);
        //Debug.Log("Active ability");
    }

    protected override void Execute() // Mix between pin down and disrupt, likely needs to be updated
    {
        target = null;
        //float minDist = rangeSquared;
        var targetTransform = Core.GetTargetingSystem().GetTarget();

        if (ExtraCriteriaToActivate())
        {
            ActivationCosmetic(transform.position);
            if (targetTransform)
            {
                var targetTransformDist = (targetTransform.position - Core.transform.position).sqrMagnitude;
                if (targetTransformDist < rangeSquared && ValidityCheck(targetTransform.GetComponent<Entity>()))
                {
                    target = targetTransform.GetComponent<Entity>() as Craft;
                    var residue = target.TakeShellDamage(2 * slashDamage * abilityTier, 0, target);
                    if (target is Entity entity)
                    {
                        entity.TakeCoreDamage(residue/2);
                    }
                }
            }

            if (!target)
            {
                for (int i = 0; i < AIData.entities.Count; i++)
                {
                    if (ValidityCheck(AIData.entities[i]))
                    {
                        float d = (Core.transform.position - AIData.entities[i].transform.position).sqrMagnitude;
                        if (d < rangeSquared)
                        {
                            //minDist = d;
                            target = AIData.entities[i] as Craft;
                            var residue = target.TakeShellDamage(slashDamage * abilityTier, 0, target);
                            if (target is Entity entity)
                            {
                                entity.TakeCoreDamage(residue/2);
                            }
                        }
                    }
                }
            }
            base.Execute();
        }
    }

    bool ValidityCheck(Entity ent)
    {
        return (ent is Craft && !ent.GetIsDead() && !FactionManager.IsAllied(ent.faction, Core.faction) && !ent.IsInvisible && !ent.isAbsorbing);
    }
}
