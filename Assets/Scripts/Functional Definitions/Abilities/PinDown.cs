using System.Xml.Schema;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Immobilizes the nearest enemy
/// </summary>
public class PinDown : ActiveAbility
{
    Craft target;
    const float range = 15f;
    float rangeSquared = range * range;
    private static float PINDOWN_ACTIVE_DURATION = 5f;

    public override float GetRange()
    {
        return range;
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        ID = AbilityID.PinDown;
        energyCost = 100f;
        cooldownDuration = 10f;
        activeDuration = PINDOWN_ACTIVE_DURATION;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        if (target != null && target)
        {
            target.RemovePin();
        }
    }

    public override void ActivationCosmetic(Vector3 targetPos)
    {
        AudioManager.PlayClipByID("clip_activateability", targetPos);
        base.ActivationCosmetic(targetPos);
    }


    private static GameObject missileLinePrefab;
    public static void InflictionCosmetic(Entity entity, int coreFaction = 0)
    {
        if (!missileLinePrefab)
        {
            missileLinePrefab = new GameObject("Missile Line"); // create prefab and set to parent
            LineRenderer lineRenderer = missileLinePrefab.AddComponent<LineRenderer>(); // add line renderer
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material"); // get material
            MissileAnimationScript comp = missileLinePrefab.AddComponent<MissileAnimationScript>(); // add the animation script
        }

        var missileColor = new Color(0.8F, 1F, 1F, 0.9F);

        foreach (var part in entity.GetComponentsInChildren<ShellPart>())
        {
            var x = Instantiate(missileLinePrefab, part.transform); // instantiate
            x.GetComponent<MissileAnimationScript>().Initialize(); // initialize
            x.GetComponent<MissileAnimationScript>().lineColor = missileColor;
            Destroy(x, PINDOWN_ACTIVE_DURATION);
        }
    }
    /// <summary>
    /// Immobilizes a nearby enemy
    /// </summary>
    protected override void Execute()
    {
        ActivationCosmetic(transform.position);
        var targeting = Core.GetTargetingSystem().GetTarget();
        var selection = (targeting.transform.position - Core.transform.position).sqrMagnitude;
        float minDist = rangeSquared;
        target = null;

        for (int i = 0; i < AIData.entities.Count; i++)
        {
            if (targeting != null && selection < minDist && ValidityCheck(targeting.GetComponent<Entity>()))
            {
                target = targeting.GetComponent<Entity>() as Craft;
                break;
            }

            if (ValidityCheck(AIData.entities[i]))
            {
                float d = (Core.transform.position - AIData.entities[i].transform.position).sqrMagnitude;
                if (d < minDist)
                {
                    minDist = d;
                    target = AIData.entities[i] as Craft;
                }
            }
        }

        if (target != null)
        {
            target.AddPin();
            InflictionCosmetic(target, Core.faction);
            if (target.networkAdapter) target.networkAdapter.InflictionCosmeticClientRpc((int)AbilityID.PinDown);
        }

        base.Execute();

    }



    bool ValidityCheck(Entity ent)
    {
        return (ent is Craft && !ent.GetIsDead() && !FactionManager.IsAllied(ent.faction, Core.faction) && !ent.IsInvisible);
    }

}
