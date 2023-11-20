using UnityEngine;
/// <summary>
/// Temporarily increases the craft's damage multiplier
/// </summary>
public class DamageBoost : ActiveAbility
{
    public const float damageFactor = 0.1F;
    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        ID = AbilityID.DamageBoost;
        cooldownDuration = 20;
        activeDuration = 5;
        energyCost = 200;
    }

    /// <summary>
    /// Returns the engine power to the original value
    /// </summary>
    public override void Deactivate()
    {
        if (Core)
        {
            Core.damageBoostGasBoosted = gasBoosted;
            Core.DamageBoostStacks -= Mathf.Max(1, abilityTier);
            base.Deactivate();
        }
    }

    private GameObject damageBoostEffectPrefab;

    public override void ActivationCosmetic(Vector3 targetPos)
    {
        AudioManager.PlayClipByID("clip_buff", targetPos);
        if (!damageBoostEffectPrefab)
        {
            damageBoostEffectPrefab = ResourceManager.GetAsset<GameObject>("damage_boost_effect");
        }

        Instantiate(damageBoostEffectPrefab, Core.transform.position, Quaternion.identity);
        base.ActivationCosmetic(targetPos);
    }

    /// <summary>
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        if (Core)
        {
            Core.DamageBoostStacks += Mathf.Max(1, abilityTier);
            Core.damageBoostGasBoosted = gasBoosted;
            ActivationCosmetic(transform.position);
            base.Execute();
        }
    }
}
