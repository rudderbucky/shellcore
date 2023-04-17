using UnityEngine;

/// <summary>
/// Heals the shell of the associated craft
/// </summary>
public class Slash : Ability
{
    public void Initialize()
    {
        
        abilityName = "Slash";
    }

    private GameObject slashPrefab;
    

    protected override void Awake()
    {
        base.Awake();

        energyCost = 35;

        ID = AbilityID.Slash;
        cooldownDuration = 2;
    }

    

    protected override bool ExtraCriteriaToActivate()
    {
        return !Core.IsInvisible;
    }

    public override void ActivationCosmetic(Vector3 targetPos)
    {
        slashPrefab = ResourceManager.GetAsset<GameObject>("slash_prefab");
        AudioManager.PlayClipByID("clip_slasheffect", targetPos);
        Instantiate(slashPrefab, Core.transform);
        base.ActivationCosmetic(targetPos);
    }

    /// <summary>
    /// Heals the shell of the core (doesn't heal and refunds the energy used if it would overheal)
    /// </summary>
    protected override void Execute()
    {
        if (ExtraCriteriaToActivate())
        {
            ActivationCosmetic(transform.position);
            base.Execute();
        }
        else
        {
            Core.TakeEnergy(-energyCost); // refund energy
            startTime = Time.time - cooldownDuration;
        }
    }
}
