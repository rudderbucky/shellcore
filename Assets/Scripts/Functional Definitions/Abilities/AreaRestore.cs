using UnityEngine;
/// <summary>
/// Heals all allies in range
/// </summary>
public class AreaRestore : Ability
{
    const float range = 10;
    public const float heal = 1000;

    public override float GetRange()
    {
        return range;
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        ID = AbilityID.AreaRestore;
        energyCost = 150;
        cooldownDuration = 10;
    }

    private GameObject areaRestoreEffectPrefab;
    public override void ActivationCosmetic(Vector3 targetPos)
    {
        AudioManager.PlayClipByID("clip_healeffect", targetPos);
        if (!areaRestoreEffectPrefab)
        {
            areaRestoreEffectPrefab = ResourceManager.GetAsset<GameObject>("area_restore_effect");
        }

        Instantiate(areaRestoreEffectPrefab, transform.position, Quaternion.identity);
        base.ActivationCosmetic(targetPos);
    }

    /// <summary>
    /// Heals all nearby allies
    /// </summary>
    protected override void Execute()
    {
        ActivationCosmetic(transform.position);
        for (int i = 0; i < AIData.entities.Count; i++)
        {
            if (AIData.entities[i] == Core) continue;
            if (FactionManager.IsAllied(AIData.entities[i].faction, Core.GetFaction()))
            {
                Entity ally = AIData.entities[i];
                float d = (ally.transform.position - Core.transform.position).sqrMagnitude;
                if (d < range * range)
                {
                    if (ally.GetHealth()[0] < ally.GetMaxHealth()[0])
                    {
                        ally.TakeShellDamage(-heal * Mathf.Max(1, abilityTier), 0f, GetComponentInParent<Entity>());
                    }
                }
            }
        }
    }
}
