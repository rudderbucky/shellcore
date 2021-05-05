using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes the craft temporarily invisible
/// </summary>
public class Stealth : ActiveAbility
{
    Craft craft;

    protected override void Awake()
    {
        base.Awake(); // base awake
        ID = AbilityID.Stealth;
        cooldownDuration = 10;
        activeDuration = 4;
        energyCost = 100;
    }

    private void Start()
    {
        craft = Core as Craft;
    }

    /// <summary>
    /// Makes the craft visible again
    /// </summary>
    public override void Deactivate()
    {
        base.Deactivate();

        craft.stealths--;

        if (craft.stealths < 0)
        {
            Debug.LogError($"Stealth is bugged, complain to Ormanus [entity name: {craft.name}, faction: {craft.faction}, count: {craft.stealths}");
        }

        if (craft.stealths == 0)
        {
            SpriteRenderer[] renderers = craft.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var c = renderers[i].color;
                c.a = 1f;
                renderers[i].color = c;
            }
            Collider2D[] colliders = craft.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = true;
            }
        }
    }

    /// <summary>
    /// Makes the craft invisible
    /// </summary>
    protected override void Execute()
    {
        if(craft)
        {
            // change visibility
            craft.stealths++;
            SpriteRenderer[] renderers = craft.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var c = renderers[i].color;
                c.a = Core.faction == 0 ? 0.2f : 0f;
                renderers[i].color = c;
            }
            Collider2D[] colliders = craft.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            AudioManager.PlayClipByID("clip_activateability", transform.position);
            base.Execute();
        }
    }
}
