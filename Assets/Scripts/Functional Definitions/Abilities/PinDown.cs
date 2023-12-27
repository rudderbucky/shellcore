using System.Collections;
using UnityEngine;

/// <summary>
/// Immobilizes the nearest enemy
/// </summary>
public class PinDown : ActiveAbility
{
    Craft target;
    const float range = 15f;
    float rangeSquared = range * range;
    private static readonly float PINDOWN_ACTIVE_DURATION = 5f;

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
    public static void InflictionCosmetic(Entity entity, int coreFaction = 0, bool destroy = true)
    {
        if (!missileLinePrefab)
        {
            missileLinePrefab = new GameObject("Missile Line"); // create prefab and set to parent
            LineRenderer lineRenderer = missileLinePrefab.AddComponent<LineRenderer>(); // add line renderer
            lineRenderer.useWorldSpace = false;
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material"); // get material
            MissileAnimationScript comp = missileLinePrefab.AddComponent<MissileAnimationScript>(); // add the animation script
        }

        var missileColor = new Color(0.8F, 1F, 1F, 0.9F);

        if (entity.pinDownCosmetic != null)
        {
            entity.StopPinDownCosmetic();
        }
        entity.pinDownCosmetic = PindownCoroutine(missileColor, destroy, entity);
        if (entity && entity.pinDownCosmetic != null) entity.StartCoroutine(entity.pinDownCosmetic);
        return;
    }

    static IEnumerator PindownCoroutine(Color missileColor, bool destroy, Entity entity)
    {
        float time = 0;
        while (time < PINDOWN_ACTIVE_DURATION || !destroy)
        {
            foreach (var part in entity.GetComponentsInChildren<ShellPart>())
            {
                if (!part || !part.GetComponent<SpriteRenderer>() ) continue;
                var rend = part.GetComponent<SpriteRenderer>();
                if (!rend.sprite) continue;
                var localPosition = new Vector2();
                var extents = rend.sprite.bounds.extents;
                localPosition.x = Random.Range(-extents.x, extents.x);
                localPosition.y = Random.Range(-extents.y, extents.y);
                var x = Instantiate(missileLinePrefab, part.transform); // instantiate
                x.transform.localPosition = localPosition;
                x.GetComponent<MissileAnimationScript>().Initialize(); // initialize
                x.GetComponent<MissileAnimationScript>().lineColor = missileColor;
                Destroy(x, 0.3F);
            }
            time += 0.25F;
            yield return new WaitForSeconds(0.25F);
        }
    }


    /// <summary>
    /// Immobilizes a nearby enemy
    /// </summary>
    protected override void Execute()
    {
        ActivationCosmetic(transform.position);
        target = null;
        float minDist = rangeSquared;
        var targetTransform = Core.GetTargetingSystem().GetTarget();

        if (targetTransform)
        {
            var targetTransformDist = (targetTransform.position - Core.transform.position).sqrMagnitude;
            if (targetTransformDist < minDist && ValidityCheck(targetTransform.GetComponent<Entity>()))
            {
                target = targetTransform.GetComponent<Entity>() as Craft;
            }
        }


        if (!target)
            for (int i = 0; i < AIData.entities.Count; i++)
            {
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
            InflictionCosmetic(target, Core.faction.factionID);
            if (target.networkAdapter) target.networkAdapter.InflictionCosmeticClientRpc((int)AbilityID.PinDown);
        }

        base.Execute();

    }



    bool ValidityCheck(Entity ent)
    {
        return (ent is Craft && !ent.GetIsDead() && !FactionManager.IsAllied(ent.faction, Core.faction) && !ent.IsInvisible);
    }

}
