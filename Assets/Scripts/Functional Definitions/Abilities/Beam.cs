using UnityEngine;
using System.Collections.Generic;

public class Beam : WeaponAbility
{
    public LineRenderer line; // line renderer of the beam
    private Material material; // material used by the line renderer
    protected bool firing; // check for line renderer drawing
    protected float timer; // float timer for line renderer drawing
    public GameObject beamHitPrefab;
    public static readonly int beamDamage = 525;
    protected List<Transform> targetArray;

    protected int numShots = 0;
    protected int MAX_BOUNCES = 1;

    protected override void Awake()
    {
        // set instance fields 
        base.Awake();
        abilityName = "Beam";
        description = $"Instant attack that deals {damage} damage.";
        line = GetComponent<LineRenderer>() ? GetComponent<LineRenderer>() : gameObject.AddComponent<LineRenderer>();
        line.sortingLayerName = "Projectiles";
        line.material = material;
        line.startWidth = line.endWidth = 0.15F;
        damage = beamDamage;
        energyCost = 50;
        ID = AbilityID.Beam;
        range = 8;
        category = Entity.EntityCategory.All;
        bonusDamageType = typeof(ShellCore);
        cooldownDuration = 3f;
        targetArray = new List<Transform>();
        line.positionCount = 0;
    }

    protected void SetUpCosmetics()
    {
        SetMaterial(ResourceManager.GetAsset<Material>("white_material"));
        line.endColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction.factionID) : new Color(0.8F, 1F, 1F, 0.9F);
        line.startColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction.factionID) : new Color(0.8F, 1F, 1F, 0.9F);
        if (!beamHitPrefab)
        {
            beamHitPrefab = ResourceManager.GetAsset<GameObject>("weapon_hit_particle");
        }
        if (!particlePrefab)
        {
            particlePrefab = ResourceManager.GetAsset<GameObject>("beamParticle_prefab");
        }
    }

    protected override void Start()
    {
        if (Core as Tank) bonusDamageType = typeof(Tank);
        
        base.Start();
    }

    public void SetMaterial(Material material)
    {
        this.material = material;
        line.material = material;
    }

    protected void RenderBeam(int currentVertex)
    {
        if (firing && timer < 0.1F*(currentVertex+1)) // timer for drawing the beam, past the set timer float value and it stops being drawn
        {
            line.startWidth = line.endWidth = 0.15F;
            line.SetPosition(0, transform.position); // draw and increment timer
            if (currentVertex+1 < line.positionCount)
            {
                if (nextTargetPart && !MasterNetworkAdapter.lettingServerDecide)
                {
                    line.SetPosition(currentVertex + 1, partPos);
                }
                else if (targetArray.Count > currentVertex && targetArray[currentVertex])
                {
                    line.SetPosition(currentVertex + 1, targetArray[currentVertex].position);
                }
                else if (!MasterNetworkAdapter.lettingServerDecide)
                {
                    line.SetPosition(currentVertex + 1, line.transform.position); // TODO: Fix
                }
            }
        }
    }

    protected virtual void Update()
    {
        if (DialogueSystem.isInCutscene || (Core && Core.IsInvisible))
        {
            line.startWidth = line.endWidth = 0;
            firing = false;
            line.positionCount = 0;
            numShots = 0;
        }

        if (!firing)
        {
            numShots = 0;
            if (!(this is ChainBeam))
            {
                MAX_BOUNCES = gasBoosted ? 3 : 1;
            }
            else MAX_BOUNCES = 3;
            return;
        }

        if (firing)
            timer += Time.deltaTime;

        if (timer > 0.1f * numShots && numShots < MAX_BOUNCES)
        {
            var vec = numShots == 0 ? transform.position : line.GetPosition(numShots);
            var closestEntity = targetingSystem.GetTarget();

            if (closestEntity && !targetArray.Contains(closestEntity) && numShots == 0)
            {
                targetArray.Add(closestEntity);
                FireBeam(closestEntity.position);
                numShots++;
            }
            else
            {
                var ents = GetClosestTargets(MAX_BOUNCES, vec);
                closestEntity = null;
                foreach (var ent in ents)
                {
                    if (targetArray.Contains(ent))
                    {
                        continue;
                    }
                    closestEntity = ent;
                    break;
                }

                if (!closestEntity)
                {
                    //numShots = 0;
                }
                else
                {
                    targetArray.Add(closestEntity);
                    FireBeam(closestEntity.position);
                    numShots++;
                }
            }
        }

        for (int i = 0; i < line.positionCount; i++)
        {
            RenderBeam(i);
        }

        if (firing && timer >= 0.1F * line.positionCount)
        {
            if (line.startWidth > 0)
            {
                line.startWidth -= 0.01F * Time.timeScale;
                line.endWidth -= 0.01F * Time.timeScale;
            }

            if (line.startWidth < 0)
            {
                line.startWidth = line.endWidth = 0;
                firing = false;
                line.positionCount = 0;
                numShots = 0;
            }
        }
    }

    protected override bool Execute(Vector3 victimPos)
    {
        if (!beamHitPrefab)
        {
            SetUpCosmetics();
        }
        targetArray.Clear();
        firing = true;
        numShots = 0;
        return true;
    }

    public override void ActivationCosmetic(Vector3 targetPos)
    {
        if (!beamHitPrefab)
        {
            SetUpCosmetics();
        }
        AudioManager.PlayClipByID("clip_beam", transform.position);
        if (MasterNetworkAdapter.lettingServerDecide && targetingSystem.GetTarget() && targetingSystem.GetTarget().GetComponentInParent<Entity>())
        {
            GetClosestPart(targetingSystem.GetTarget().GetComponentInParent<Entity>().NetworkGetParts().ToArray());
            targetPos = nextTargetPart.transform.position;
        }

        if (line.positionCount == 0) 
        {
            timer = 0; // start the timer
            line.positionCount = 2; // render the beam line
            if (MasterNetworkAdapter.lettingServerDecide)
            {
                line.SetPosition(1, targetPos);
            }
        }
        else 
        {
            line.positionCount++;
        }
        firing = true;

        Instantiate(beamHitPrefab, targetPos, Quaternion.identity); // instantiate hit effect
        InstantiateParticles(line.positionCount > 2 ? line.GetPosition(line.positionCount - 2) : transform.position, targetPos);
    }


    protected void FireBeam(Vector3 victimPos)
    {
        Transform targetToAttack = targetArray[targetArray.Count - 1];
        var residue = targetToAttack.GetComponent<IDamageable>().TakeShellDamage(GetDamage(), 0, GetComponentInParent<Entity>());
        // deal instant damage

        if (nextTargetPart && nextTargetPart.craft.gameObject != targetToAttack.gameObject)
        {
            nextTargetPart = null;
        }

        if (nextTargetPart)
        {
            nextTargetPart.TakeDamage(residue);
            victimPos = partPos = nextTargetPart.transform.position;
        }

        ActivationCosmetic(victimPos);
    }

    protected Transform[] GetClosestTargets(int num, Vector3 pos, bool dronesAreFree = false)
    {
        var list = targetingSystem.GetClosestTargets(num, pos);
        if (list.Length > 0)
        {
            foreach (var ent in list)
            {
                if (targetArray.Contains(ent)) continue;
                GetClosestPart(pos, ent.GetComponentsInChildren<ShellPart>());
                break;
            }
        }
        return list;
    }

    public GameObject particlePrefab;

    private void InstantiateParticles(Vector3 origPos, Vector3 victimPos)
    {
        Vector3 distance = victimPos - origPos;
        Vector3 distanceNormalized = distance.normalized;
        Vector3 currentPos = origPos;

        while ((currentPos - origPos).sqrMagnitude < distance.sqrMagnitude)
        {
            Instantiate(particlePrefab, (Vector2)currentPos, Quaternion.identity);
            currentPos += distanceNormalized * 0.8F;
        }
    }

    ShellPart nextTargetPart;
    Vector2 partPos;

    protected void GetClosestPart(ShellPart[] parts)
    {
        GetClosestPart(transform.position, parts);
    }

    protected void GetClosestPart(Vector3 pos, ShellPart[] parts)
    {
        float closestD = range;
        nextTargetPart = null;
        foreach (var part in parts)
        {
            var distance = Vector2.Distance(part.transform.position, pos);
            if (distance < closestD)
            {
                closestD = distance;
                nextTargetPart = part;
            }
        }
    }

    protected override bool DistanceCheck(Transform targetEntity)
    {
        var parts = targetEntity.GetComponentsInChildren<ShellPart>();
        if (parts.Length == 0)
        {
            return base.DistanceCheck(targetEntity);
        }
        else
        {
            GetClosestPart(parts);
            return nextTargetPart;
        }
    }
}
