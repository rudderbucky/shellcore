using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Immobilizes the craft and converts all damage to energy
/// </summary>
public class AbsorptionField : ActiveAbility, IBlinkOnUse
{
    Craft craft;
    GameObject field;

    protected override void Awake()
    {
        base.Awake(); // base awake
        ID = AbilityID.Absorb;
        cooldownDuration = 10;
        CDRemaining = cooldownDuration;
        activeDuration = 1;
        activeTimeRemaining = activeDuration;
        energyCost = 100;
    }

    private void Start()
    {
        craft = Core as Craft;
    }

    /// <summary>
    /// Removes the field sprite
    /// </summary>
    public override void Deactivate()
    {
        base.Deactivate();
        Destroy(field);
        craft.absorptions--;
    }

    /// <summary>
    /// Creates the field sprite
    /// </summary>
    protected override void Execute()
    {
        if(craft)
        {
            craft.entityBody.velocity = Vector2.zero;
            craft.absorptions++;
            field = new GameObject("Field");
            field.transform.SetParent(craft.transform);
            field.transform.localScale = Vector3.one;
            field.transform.localPosition = Vector3.zero;
            var sr = field.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceManager.GetAsset<Sprite>("absorption_sprite");
            sr.sortingOrder = 1000;
        }
        AudioManager.PlayClipByID("clip_buff", transform.position);
        // adjust fields
        isActive = true; // set to active
        isOnCD = true; // set to on cooldown
        base.Execute();
    }
}
