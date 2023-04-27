using UnityEngine;

/// <summary>
/// Immobilizes the craft and converts all damage to energy
/// </summary>
public class AbsorptionField : ActiveAbility
{
    Craft craft;
    GameObject field;

    protected override void Awake()
    {
        base.Awake(); // base awake
        ID = AbilityID.Absorb;
        cooldownDuration = 10;
        activeDuration = 1;
        energyCost = 100;
    }

    protected override void Start()
    {
        craft = Core as Craft;
        base.Start();
    }

    /// <summary>
    /// Removes the field sprite
    /// </summary>
    public override void Deactivate()
    {
        base.Deactivate();
        Destroy(field);
        if (craft)
            craft.absorptions--;
    }

    public override void ActivationCosmetic(Vector3 targetPos)
    {
        SetActivationState();
        Execute();
        base.ActivationCosmetic(targetPos);
    }

    /// <summary>
    /// Creates the field sprite
    /// </summary>
    protected override void Execute()
    {
        if (craft)
        {
            craft.entityBody.velocity = Vector2.zero;
            craft.absorptions++;
            Instantiate(ResourceManager.GetAsset<GameObject>("absorb_prefab"), Core.transform);
        }

        AudioManager.PlayClipByID("clip_buff", transform.position);
        // adjust fields
        base.Execute();
    }
}
