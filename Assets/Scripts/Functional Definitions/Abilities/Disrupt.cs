using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Resets active cooldowns of nearby enemies
/// </summary>
public class Disrupt : Ability
{
    const float range = 10f;

    public override float GetRange()
    {
        return range;
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        ID = AbilityID.Disrupt;
        energyCost = 200;
        cooldownDuration = 30;
    }

    public override void ActivationCosmetic(Vector3 targetPos)
    {
        AudioManager.PlayClipByID("clip_disrupt", targetPos);
        base.ActivationCosmetic(targetPos);
    }

    private static GameObject missileLinePrefab;

    public static void InflictionCosmetic(Entity entity)
    {
        foreach (var part in entity.GetComponentsInChildren<ShellPart>())
        {
            if (!part) continue;
            if (part.GetComponent<Ability>() && !(part.GetComponent<Ability>() as PassiveAbility))
            {
                //part.SetPartColor(Color.grey);
                part.lerpColors();
                if (!missileLinePrefab)
                {

                    missileLinePrefab = new GameObject("Missile Line"); // create prefab
                    LineRenderer lineRenderer = missileLinePrefab.AddComponent<LineRenderer>(); // add line renderer
                    lineRenderer.material = ResourceManager.GetAsset<Material>("white_material"); // get material
                    MissileAnimationScript comp = missileLinePrefab.AddComponent<MissileAnimationScript>(); // add the animation script
                }

                var missileColor = new Color(0.8F, 1F, 1F, 0.9F);
                var x = Instantiate(missileLinePrefab, part.transform); // instantiate
                x.GetComponent<MissileAnimationScript>().Initialize(); // initialize
                x.GetComponent<MissileAnimationScript>().lineColor = missileColor;
                Destroy(x, 0.5f);
            }
        }
    }

    /// <summary>
    /// Resets active cooldowns of nearby enemies
    /// </summary>
    protected override void Execute()
    {
        ActivationCosmetic(transform.position);
        for (int i = 0; i < AIData.entities.Count; i++)
        {
            if (AIData.entities[i] is Craft && !AIData.entities[i].GetIsDead() && !FactionManager.IsAllied(AIData.entities[i].faction, Core.faction) && !AIData.entities[i].IsInvisible)
            {
                float d = (Core.transform.position - AIData.entities[i].transform.position).sqrMagnitude;
                if (d < range * range)
                {
                    foreach (var ability in AIData.entities[i].GetAbilities())
                    {
                        if (ability != null)
                        {
                            ability.ResetCD();
                        }
                    }

                    InflictionCosmetic(AIData.entities[i]);
                    if (AIData.entities[i].networkAdapter) AIData.entities[i].networkAdapter.InflictionCosmeticClientRpc((int)AbilityID.Disrupt);
                }
            }
        }

        base.Execute();
    }
}
