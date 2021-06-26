using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Railgun : WeaponAbility
{
    public LineRenderer line; // line renderer of the beam
    private Material material; // material used by the line renderer
    private bool firing; // check for line renderer drawing
    private float timer; // float timer for line renderer drawing
    public GameObject beamHitPrefab;
    public static readonly int beamDamage = 800; //arbitrary
    public static readonly float chargeTime = 1.5f; //arbitrary

    protected override void Awake()
    {
        // set instance fields 
        base.Awake();
        abilityName = "Railgun";
        description = "Long range beam attack that deals " + damage + " damage after a " + chargeTime + " second delay.";
        line = GetComponent<LineRenderer>() ? GetComponent<LineRenderer>() : gameObject.AddComponent<LineRenderer>();
        line.sortingLayerName = "Projectiles";
        line.material = material;
        line.startWidth = line.endWidth = 0.35F;
        damage = beamDamage;
        energyCost = 100; //arbitrary
        ID = AbilityID.Railgun;
        range = 15; //arbitrary
        category = Entity.EntityCategory.All;
        bonusDamageType = typeof(ShellCore); //arbitrary
        cooldownDuration = 10f;
    }
    protected override void Start() {
        SetMaterial(ResourceManager.GetAsset<Material>("white_material"));
        particlePrefab = ResourceManager.GetAsset<GameObject>("beamParticle_prefab");
        line.endColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction) : new Color(0.8F,1F,1F,0.9F);
        line.startColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction) : new Color(0.8F,1F,1F,0.9F);
        base.Start();
    }
    public void SetMaterial(Material material)
    {
        this.material = material;
        line.material = material;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (firing && timer < 0.1F) // timer for drawing the beam, past the set timer float value and it stops being drawn
        {
            line.startWidth = line.endWidth = 0.35F;
            line.SetPosition(0, transform.position); // draw and increment timer
            if(nextTargetPart) line.SetPosition(1, partPos);
            else if(targetingSystem.GetTarget()) line.SetPosition(1, targetingSystem.GetTarget().position);
            else line.SetPosition(1, line.transform.position); // TODO: Fix
        }
        else if(firing && timer >= 0.1F)
        {
            if(line.startWidth > 0)
            {
                line.startWidth -= 0.01F;
                line.endWidth -= 0.01F;
            }
            if(line.startWidth < 0)
            {
                line.startWidth = line.endWidth = 0;
                firing = false;
            }

        } else if (timer != -9999f) //this is a really stupid bandaid but it works so i'm calling it a feature
        {
            if (line.startWidth > 0){
                line.startWidth -= 0.005F;
                line.endWidth -= 0.005F;
            }
            else{
                line.positionCount = 0;
                firing = false;
            }
        }
    }
    private IEnumerator doTheThing(Vector3 victimPos){
        float charge = 0;
        while (charge < chargeTime * abilityTier){
            Vector3 pos = targetingSystem.GetTarget().position;
            if (nextTargetPart)
            {
                pos = partPos = nextTargetPart.transform.position;
            }
            if(!beamHitPrefab) beamHitPrefab = ResourceManager.GetAsset<GameObject>("weapon_hit_particle");
            if (!DistanceCheck(targetingSystem.GetTarget()) || !targetingSystem.GetTarget() || targetingSystem.GetTarget() == null){
                timer = 0f;
                break;
            }
            else{
                charge += Time.deltaTime;
                line.SetPositions(new Vector3[] {transform.position, pos});
                line.startWidth = line.endWidth = 0.1f * (1 - charge / (chargeTime * abilityTier));;
                timer = -9999f;
                line.positionCount = 2; 
                yield return new WaitForEndOfFrame();
            }
        }
        if(!beamHitPrefab) beamHitPrefab = ResourceManager.GetAsset<GameObject>("weapon_hit_particle");
        AudioManager.PlayClipByID("clip_beam", transform.position);
        GetDamage();
        var residue = targetingSystem.GetTarget().GetComponent<IDamageable>().TakeShellDamage(GetDamage(), 0, GetComponentInParent<Entity>());
        // deal instant damage

        if (nextTargetPart)
        {
            nextTargetPart.TakeDamage(residue);
            victimPos = partPos = nextTargetPart.transform.position;
        }
        // if(targetingSystem.GetTarget().GetComponent<Entity>())
        //   targetingSystem.GetTarget().GetComponent<Entity>().TakeCoreDamage(residue);
        line.positionCount = 2; // render the beam line
        timer = 0; // start the timer
        firing = true;

        Instantiate(beamHitPrefab, victimPos, Quaternion.identity); // instantiate hit effect
        InstantiateParticles(victimPos);
    }

    protected override bool Execute(Vector3 victimPos)
    {
        StartCoroutine("doTheThing",victimPos);
        return true;
    }

    public GameObject particlePrefab;

    private void InstantiateParticles(Vector3 victimPos)
    {
        Vector3 distance = victimPos - transform.position;
        Vector3 distanceNormalized = distance.normalized;
        Vector3 currentPos = transform.position;

        while((currentPos - transform.position).sqrMagnitude < distance.sqrMagnitude)
        {
            Instantiate(particlePrefab, (Vector2)currentPos, Quaternion.identity);
            currentPos += distanceNormalized * 0.8F;
        }
    }

    ShellPart nextTargetPart;
    Vector2 partPos;

    protected override bool DistanceCheck(Transform targetEntity) {
        var parts = targetEntity.GetComponentsInChildren<ShellPart>();
        if(parts.Length == 0) return base.DistanceCheck(targetEntity);
        else {
            float closestD = range;
            nextTargetPart = null;
            foreach(var part in parts) {
                var distance = Vector2.Distance(part.transform.position, transform.position);
                if(distance < closestD) {
                    closestD = distance;
                    nextTargetPart = part;
                }
            }
            return nextTargetPart;
        }
    }
}
