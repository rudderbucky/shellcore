using UnityEngine;

public class InvertTractor : ActiveAbility
{
    public override void SetTier(int abilityTier)
    {
        if (abilityTier < 1) abilityTier = 1;
        base.SetTier(abilityTier);
        activeDuration = 5 * abilityTier;
        cooldownDuration = 5 + abilityTier * 5;
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        ID = AbilityID.InvertTractor;
        cooldownDuration = 15;
        energyCost = 50;
    }

    /// <summary>
    /// Disable invert tractor
    /// </summary>
    public override void Deactivate()
    {
        Core.invertTractors--;
        if (Core is ShellCore core && !MasterNetworkAdapter.lettingServerDecide) core.SetTractorTarget(null);
        base.Deactivate();
    }

    // What immediately happens when a weapon is fired
    public override void ActivationCosmetic(Vector3 targetPos)
    {
        SetActivationState();
        Execute();
    }

    /// <summary>
    /// Invert tractor
    /// </summary>
    protected override void Execute()
    {
        Core.invertTractors++;
        AudioManager.PlayClipByID("clip_buff", transform.position);
        base.Execute();
    }
}
