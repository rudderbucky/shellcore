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

    protected override void Start()
    {
        if (Core as Tank) bonusDamageType = typeof(Tank);
        SetMaterial(ResourceManager.GetAsset<Material>("white_material"));
        line.endColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction) : new Color(0.8F, 1F, 1F, 0.9F);
        line.startColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction) : new Color(0.8F, 1F, 1F, 0.9F);
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
            if (nextTargetPart)
            {
                line.SetPosition(currentVertex+1, partPos);
            }
            else if (targetArray[currentVertex])
            {
                line.SetPosition(currentVertex+1, targetArray[currentVertex].position);
            }
            else
            {
                line.SetPosition(currentVertex+1, line.transform.position); // TODO: Fix
            }

            if (currentVertex == line.positionCount - 2)
                timer += Time.deltaTime;
        }
        else if (firing && timer >= 0.1F*(currentVertex+1) && currentVertex == line.positionCount - 2)
        {
            if (line.startWidth > 0)
            {
                line.startWidth -= 0.01F;
                line.endWidth -= 0.01F;
            }

            if (line.startWidth < 0)
            {
                line.startWidth = line.endWidth = 0;
                firing = false;
                line.positionCount = 0;
            }
        }
        else if (currentVertex == line.positionCount - 2)
        {
            line.positionCount = 0;
            firing = false;
        }
    }

    protected virtual void Update()
    {
        RenderBeam(0);
    }

    protected override bool Execute(Vector3 victimPos)
    {
        if (!beamHitPrefab)
        {
            beamHitPrefab = ResourceManager.GetAsset<GameObject>("weapon_hit_particle");
        }
        if (!particlePrefab)
        {
            particlePrefab = ResourceManager.GetAsset<GameObject>("beamParticle_prefab");
        }

        targetArray.Clear();
        targetArray.Add(targetingSystem.GetTarget());
        FireBeam(victimPos);
        return true;
    }

    protected void FireBeam(Vector3 victimPos)
    {
        AudioManager.PlayClipByID("clip_beam", transform.position);
        Transform targetToAttack = targetArray[targetArray.Count - 1];
        var residue = targetToAttack.GetComponent<IDamageable>().TakeShellDamage(GetDamage(), 0, GetComponentInParent<Entity>());
        // deal instant damage

        if (nextTargetPart)
        {
            nextTargetPart.TakeDamage(residue);
            victimPos = partPos = nextTargetPart.transform.position;
        }

        if (line.positionCount == 0) 
        {
            timer = 0; // start the timer
            line.positionCount = 2; // render the beam line
        }
        else 
        {
            line.positionCount++;
        }
        firing = true;

        Instantiate(beamHitPrefab, victimPos, Quaternion.identity); // instantiate hit effect
        InstantiateParticles(line.positionCount > 2 ? line.GetPosition(line.positionCount - 2) : transform.position, victimPos);

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
