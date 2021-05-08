using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The part script for the shell around a core of a Shellcore (will be salvaged to make a more general part class)
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ShellPart : MonoBehaviour {

    float detachedTime; // time since detachment
    public ShellPart parent = null;
    public List<ShellPart> children = new List<ShellPart>();
    private bool hasDetached; // is the part detached
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigid;
    private Entity craft;
    public float partHealth; // health of the part (half added to shell, quarter to core)
    public float partMass; // mass of the part
    private float currentHealth; // current health of part
    public bool detachible = true;
    private bool collectible;
    private int faction;
    private Draggable draggable;
    private bool rotationDirection = true;
    private float rotationOffset;
    public GameObject shooter;
    public string SpawnID { get; set; }
    public EntityBlueprint.PartInfo info;
    public string droppedSectorName;
    public static int partShader = 0;
    public static List<Material> shaderMaterials = null;

    public bool weapon { get; set; } = false;

    public bool GetDetached() {
        return hasDetached;
    }
    public int GetFaction()
    {
        return faction;
    }

    public void SetCollectible(bool collectible) {
        this.collectible = collectible;
        // try setting the part to shiny
        if(collectible)
        {
             if(!transform.Find("Minimap Image"))
            {
                GameObject childObject = new GameObject("Minimap Image");
                childObject.transform.SetParent(transform, false);
                SpriteRenderer renderer = childObject.AddComponent<SpriteRenderer>();
                renderer.sprite = ResourceManager.GetAsset<Sprite>("minimap_sprite");
                childObject.AddComponent<MinimapLockRotationScript>().Initialize(); // initialize the minimap dot
            }

            if(!shinyCheck)
            {
                shinyCheck = true;
                ShinyCheck();
            }
        }
        
    }

    public float GetPartMass()
    {
        return partMass;
    }

    public float GetPartHealth()
    {
        return partHealth; // part health
    }

    public bool IsAdjacent(ShellPart part)
    {
        return part.GetComponent<SpriteRenderer>().bounds.Intersects(GetComponent<SpriteRenderer>().bounds);
    }

    /// <summary>
    /// Build Part
    /// </summary>
    /// <param name="blueprint">blueprint of the part</param>
    public static GameObject BuildPart(PartBlueprint blueprint)
    {
        if(shaderMaterials == null)
        {
            shaderMaterials = new List<Material>();
            shaderMaterials.Add(ResourceManager.GetAsset<Material>("part_shader0"));
            shaderMaterials.Add(ResourceManager.GetAsset<Material>("part_shader1"));
        }

        GameObject holder;
        if(!GameObject.Find("Part Holder")) {
            holder = new GameObject("Part Holder");
        } else holder = GameObject.Find("Part Holder");


        GameObject obj = Instantiate(ResourceManager.GetAsset<GameObject>("base_part"));
        obj.transform.SetParent(holder.transform);

        //Part sprite
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        spriteRenderer.material = shaderMaterials[partShader];
        spriteRenderer.sprite = ResourceManager.GetAsset<Sprite>(blueprint.spriteID);
        var part = obj.GetComponent<ShellPart>();
        part.partMass = blueprint.mass;
        part.partHealth = blueprint.health;
        var collider = obj.GetComponent<PolygonCollider2D>();
        collider.isTrigger = true;
        part.detachible = blueprint.detachible;

        var partSys = obj.GetComponent<ParticleSystem>();
        var sh = partSys.shape;
        if(spriteRenderer.sprite) sh.scale = (Vector3)spriteRenderer.sprite.bounds.extents * 2;
        var e = partSys.emission;
        e.rateOverTime = new ParticleSystem.MinMaxCurve(3 * (blueprint.size + 1));
        e.enabled = false;
        part.partSys = partSys;
        return obj;
    }
    
    /// <summary>
    /// Detach the part from the Shellcore
    /// </summary>
    public void Detach(bool drop = false) {
        if (name != "Shell Sprite")
            transform.SetParent(null, true);

        for(int i = 0; i < transform.childCount; i++)
        {
            if(transform.GetChild(i).name.Contains("Glow"))
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        detachedTime = Time.time; // update detached time
        hasDetached = true; // has detached now
        gameObject.AddComponent<Rigidbody2D>(); // add a rigidbody (this might become permanent)
        rigid = GetComponent<Rigidbody2D>();
        rigid.gravityScale = 0; // adjust the rigid body
        rigid.angularDrag = 0;
        float randomDir = Random.Range(0f, 360f);
        rigid.AddForce(new Vector2(Mathf.Cos(randomDir), Mathf.Sin(randomDir)) * 200f);
        //rigid.AddTorque(150f * ((Random.Range(0, 2) == 0) ? 1 : -1));
        rotationDirection = (Random.Range(0, 2) == 0);
        gameObject.layer = 9;
        rotationOffset = Random.Range(0f, 360f);
        droppedSectorName = SectorManager.instance.current.sectorName;
        spriteRenderer.sortingLayerName = "Air Entities";
        if(shooter)
            shooter.GetComponent<SpriteRenderer>().sortingLayerName = "Air Entities";
        GetComponent<Collider2D>().enabled = true;

        // when a part detaches it should always be completely visible
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach(var rend in renderers)
        {
            rend.color += new Color(0, 0, 0, 1);
        }
    }

    void OnDestroy() {
        if(parent) parent.children.Remove(this);
        if(AIData.strayParts.Contains(this))
            AIData.strayParts.Remove(this);
    }
    public void Awake()
    {
        //Find sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Start () {
        // initialize instance fields
        hasDetached = false;
        spriteRenderer.enabled = true;
        Destroy(GetComponent<Rigidbody2D>()); // remove rigidbody
        currentHealth = partHealth;
        
        // Drone part health penalty
        if(craft as Drone)
        {
            currentHealth /= 4;
        }

        craft = transform.root.GetComponent<Entity>();
        faction = craft.faction;
        gameObject.layer = 0;

        if (GetComponent<Ability>())
        {
            GetComponent<Ability>().Part = this;
        }

        if(info.shiny && partSys) // shell does not have a Particle System, and it also can't be shiny
        {
            StartEmitting();
        }
        else
        {
            StartCoroutine(InitColorLerp(0));
        }
    }

    ParticleSystem partSys;

    private void AimShooter()
    {
        if (shooter)
        {
            var targ = GetComponent<WeaponAbility>().GetTarget();
            if (targ != null)
            {
                var targEntity = targ.GetComponent<IDamageable>();
                if (targEntity != null && !FactionManager.IsAllied(targEntity.GetFaction(), craft.faction))
                {
                    Vector3 targeterPos = targ.position;
                    Vector3 diff = targeterPos - shooter.transform.position;
                    shooter.transform.eulerAngles = new Vector3(0, 0, (Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg) - 90);
                }
                else shooter.transform.eulerAngles = new Vector3(0, 0, 0);
            }
            else shooter.transform.eulerAngles = new Vector3(0,0,0);
        }
    }
    /// <summary>
    /// Makes the part blink like in the original game
    /// </summary>
    void Blink() {
        spriteRenderer.enabled = Time.time % 0.125F > 0.0625F; // math stuff that blinks the part
        if(shooter) shooter.GetComponent<SpriteRenderer>().enabled = spriteRenderer.enabled;
    }

    private bool shinyCheck = false;
    private bool colorLerped = false;

	// Update is called once per frame
	void Update () {


        if(spriteRenderer)
        {
            if(shaderMaterials == null)
            {
                shaderMaterials = new List<Material>();
                shaderMaterials.Add(ResourceManager.GetAsset<Material>("part_shader0"));
                shaderMaterials.Add(ResourceManager.GetAsset<Material>("part_shader1"));
            }
            spriteRenderer.material = shaderMaterials[partShader];
        }
        if (hasDetached && Time.time - detachedTime < 1) // checks if the part has been detached for more than a second (hardcoded)
        {
            if(name != "Shell Sprite" && spriteRenderer.sprite) Blink(); // blink
            else
            {
                spriteRenderer.enabled = false; // disable sprite renderer
                if(shooter) shooter.SetActive(false);                
            }
            //rigid.rotation = rigid.rotation + (rotationDirection ? 1f : -1.0f) * 360f * Time.deltaTime;
            transform.eulerAngles = new Vector3(0, 0, (rotationDirection ? 1.0f : -1.0f) * 100f * Time.time + rotationOffset);
        }
        else if (hasDetached) { // if it has actually detached
            if (collectible && detachible && !SectorManager.instance.current.partDropsDisabled)
            {
                
                rigid.drag = 25;
                // add "Draggable" component so that shellcores can grab the part
                if (!draggable) draggable = gameObject.AddComponent<Draggable>();
                spriteRenderer.enabled = true;
                if(shooter) shooter.GetComponent<SpriteRenderer>().enabled = true;
                spriteRenderer.sortingOrder = 0;
                transform.eulerAngles = new Vector3(0, 0, (rotationDirection ? 1.0f : -1.0f) * 100f * Time.time + rotationOffset);

                //rigid.angularVelocity = rigid.angularVelocity > 0 ? 200 : -200;
            }
            else
            {
                if (name != "Shell Sprite" && !(craft as PlayerCore))
                {
                    Destroy(gameObject);
                } else {
                    if(craft as PlayerCore && name != "Shell Sprite") 
                        (craft as PlayerCore).partsToDestroy.Add(this);
                    spriteRenderer.enabled = false; // disable sprite renderer
                    if(shooter) shooter.SetActive(false);
                }
            }
        }
        else if (weapon)
            AimShooter();
    }

    private void ShinyCheck()
    {
        if(Random.Range(0, 4000) == 3999) 
        {
            info.shiny = true;
            StartEmitting();
        }
    }

    private void StartEmitting()
    {
        var emission = partSys.emission;
        emission.enabled = true;

        StartCoroutine(InitColorLerp(0));

    }

    private IEnumerator InitColorLerp(float lerpVal)
    {
        colorLerped = false;
        ParticleSystem.ColorOverLifetimeModule partSysColorMod;
        if(partSys) partSysColorMod = partSys.colorOverLifetime;
        while(lerpVal < 1)
        {
            lerpVal += 0.05F;
            lerpVal = Mathf.Min(lerpVal, 1);
            var lerpedColor = Color.Lerp(Color.gray, info.shiny ? FactionManager.GetFactionShinyColor(faction) : FactionManager.GetFactionColor(faction), lerpVal);
            lerpedColor.a = (craft.IsInvisible ? (craft.faction == 0 ? 0.2f: 0f) : 1f);
            
            spriteRenderer.color = lerpedColor;
            if(shooter) shooter.GetComponent<SpriteRenderer>().color = lerpedColor;

            if(partSys) partSysColorMod.color = new ParticleSystem.MinMaxGradient(lerpedColor);
            yield return new WaitForSeconds(0.025F);
        }
        colorLerped = true;
    }

    /// <summary>
    /// Take part damage, if it is damaged too much remove the part
    /// </summary>
    /// <param name="damage">damage to deal</param>
    public void TakeDamage(float damage) {
        if(!detachible) craft.TakeCoreDamage(damage); // undetachible = core part
        currentHealth -= damage;
        if (currentHealth <= 0 && detachible)
        {
            craft.RemovePart(this);
        }

        if(partHealth != 0 && colorLerped)
        {
            var color = Color.Lerp(Color.gray, info.shiny ? FactionManager.GetFactionShinyColor(faction) :
                FactionManager.GetFactionColor(faction), currentHealth / partHealth);
            color.a = (craft.IsInvisible ? (craft.faction == 0 ? 0.2f: 0f) : 1f);
            spriteRenderer.color = color;
            if(shooter) shooter.GetComponent<SpriteRenderer>().color = spriteRenderer.color;
        }
    }
}
