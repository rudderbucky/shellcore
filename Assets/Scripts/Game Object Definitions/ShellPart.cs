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

    public int GetFaction()
    {
        return faction;
    }

    public void SetCollectible(bool collectible) {
        this.collectible = collectible;
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
        return GetComponent<Collider2D>().bounds.Intersects(part.GetComponent<Collider2D>().bounds);
    }

    /// <summary>
    /// Build Part
    /// </summary>
    /// <param name="blueprint">blueprint of the part</param>
    public static GameObject BuildPart(PartBlueprint blueprint)
    {
        GameObject holder;
        if(!GameObject.Find("Part Holder")) {
            holder = new GameObject("Part Holder");
        } else holder = GameObject.Find("Part Holder");


        GameObject obj = new GameObject(blueprint.name);
        obj.transform.SetParent(holder.transform);

        //Part sprite
        var spriteRenderer = obj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = ResourceManager.GetAsset<Sprite>(blueprint.spriteID);
        var part = obj.AddComponent<ShellPart>();
        part.partMass = blueprint.mass;
        part.partHealth = blueprint.health;
        var collider = obj.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;
        part.detachible = blueprint.detachible;
        string shooterID = null;

        switch (blueprint.abilityType)
        {
            case Ability.AbilityType.None:
                break;
            case Ability.AbilityType.MainBullet: // Shouldn't happen
                Debug.Log("Main bullet added to a part! This is a ShellCore only ability!");
                var mainBullet = obj.AddComponent<MainBullet>();
                mainBullet.bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
                shooterID = "bulletshooter_sprite";
                break;
            case Ability.AbilityType.Bullet:
                var bullet = obj.AddComponent<Bullet>();
                bullet.bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
                shooterID = "bulletshooter_sprite";
                break;
            case Ability.AbilityType.SiegeBullet:
                var siege = obj.AddComponent<SiegeBullet>();
                siege.bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
                shooterID = "bulletshooter_sprite";
                break;
            case Ability.AbilityType.Beam:
                var beam = obj.AddComponent<Beam>();
                beam.material = ResourceManager.GetAsset<Material>("white_material");
                shooterID = "beamshooter_sprite";
                break;
            case Ability.AbilityType.Bomb:
                break;
            case Ability.AbilityType.Cannon:
                var cannon = obj.AddComponent<Cannon>();
                cannon.effectPrefab = ResourceManager.GetAsset<GameObject>("cannonfire");
                shooterID = "cannonshooter_sprite";
                break;
            case Ability.AbilityType.Missile:
                var missile = obj.AddComponent<Missile>();
                missile.missilePrefab = ResourceManager.GetAsset<GameObject>("missile_prefab");
                shooterID = "missileshooter_sprite";
                break;
            case Ability.AbilityType.Torpedo:
                var torpedo = obj.AddComponent<Torpedo>();
                torpedo.bulletPrefab = ResourceManager.GetAsset<GameObject>("torpedo_prefab");
                shooterID = "torpedoshooter_sprite";
                break;
            case Ability.AbilityType.ShellBoost:
                HealthHeal shellboost = obj.AddComponent<HealthHeal>();
                shellboost.type = HealthHeal.HealingType.shell;
                shooterID = "ability_indicator";
                break;
            case Ability.AbilityType.CoreHeal:
                HealthHeal coreboost = obj.AddComponent<HealthHeal>();
                coreboost.type = HealthHeal.HealingType.core;
                shooterID = "ability_indicator";
                break;
            case Ability.AbilityType.SpeedThrust:
                obj.AddComponent<SpeedThrust>();
                shooterID = "ability_indicator";
                break;
            case Ability.AbilityType.PinDown:
                break;
            case Ability.AbilityType.EnergyBoost:
                HealthHeal energyboost = obj.AddComponent<HealthHeal>();
                energyboost.type = HealthHeal.HealingType.energy;
                shooterID = "ability_indicator";
                break;
            case Ability.AbilityType.Harvester:
                obj.AddComponent<Harvester>();
                shooterID = "ability_indicator";
                break;
            case Ability.AbilityType.SpeederBullet:
                var speedBullet = obj.AddComponent<SpeederBullet>();
                speedBullet.bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
                shooterID = "bulletshooter_sprite";
                break;
            case Ability.AbilityType.Laser:
                var laser = obj.AddComponent<Laser>();
                laser.bulletPrefab = ResourceManager.GetAsset<GameObject>("laser_prefab");
                shooterID = "lasershooter_sprite";
                break;
            case Ability.AbilityType.SpawnDrone:
                var spawn = obj.AddComponent<SpawnDrone>();
                spawn.spawnData = ResourceManager.GetAsset<DroneSpawnData>(blueprint.spawnID);
                spawn.Init();
                shooterID = "ability_indicator";
                break;
            case Ability.AbilityType.Speed:
                obj.AddComponent<Speed>();
                break;
            default:
                break;
        }

        // Add shooter
        if (shooterID != null)
        {
            var shooter = new GameObject("Shooter");
            shooter.transform.SetParent(part.transform);
            shooter.transform.localPosition = Vector3.zero;
            var shooterSprite = shooter.AddComponent<SpriteRenderer>();
            shooterSprite.sprite = ResourceManager.GetAsset<Sprite>(shooterID);
            shooterSprite.sortingOrder = 102;
            part.shooter = shooter;
        }

        // This part is only used as a prefab. It must not be active
        obj.SetActive(false);

        return obj;
    }

    /// <summary>
    /// Detach the part from the Shellcore
    /// </summary>
    public void Detach() {
        if (name != "Shell Sprite")
            transform.SetParent(null, true);
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
        currentHealth = partHealth / 4;
        craft = transform.root.GetComponent<Entity>();
        faction = craft.faction;
        spriteRenderer.color = FactionColors.colors[craft.faction];
        gameObject.layer = 0;

        if (shooter)
        {
            shooter.GetComponent<SpriteRenderer>().color = FactionColors.colors[craft.faction];
        }
        if (GetComponent<Ability>())
        {
            GetComponent<Ability>().part = this;
        }
    }

    private void AimShooter()
    {
        if (shooter)
        {
            var targ = GetComponent<WeaponAbility>().GetTarget();
            if (targ != null)
            {
                var targEntity = targ.GetComponent<Entity>();
                if (targEntity && targEntity.faction != craft.faction)
                {
                    Vector3 targeterPos = targ.position;
                    Vector3 diff = targeterPos - shooter.transform.position;
                    shooter.transform.eulerAngles = new Vector3(0, 0, (Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg) + 90);
                }
                else shooter.transform.eulerAngles = new Vector3(0, 0, 180);
            }
            else shooter.transform.eulerAngles = new Vector3(0,0,180);
        }
    }
    /// <summary>
    /// Makes the part blink like in the original game
    /// </summary>
    void Blink() {
        spriteRenderer.enabled = Time.time % 0.25F > 0.125F; // math stuff that blinks the part
    }

	// Update is called once per frame
	void Update () {
        if (hasDetached && Time.time - detachedTime < 1) // checks if the part has been detached for more than a second (hardcoded)
        {
            Blink(); // blink
            //rigid.rotation = rigid.rotation + (rotationDirection ? 1f : -1.0f) * 360f * Time.deltaTime;
            transform.eulerAngles = new Vector3(0, 0, (rotationDirection ? 1.0f : -1.0f) * 100f * Time.time + rotationOffset);
        }
        else if (hasDetached) { // if it has actually detached
            collectible = false; // TODO: make part drops rare
            if (collectible && detachible)
            {
                rigid.drag = 25;
                // add "Draggable" component so that shellcores can grab the part
                if (!draggable) draggable = gameObject.AddComponent<Draggable>();
                spriteRenderer.enabled = true;
                spriteRenderer.sortingOrder = 0;
                transform.eulerAngles = new Vector3(0, 0, (rotationDirection ? 1.0f : -1.0f) * 100f * Time.time + rotationOffset);

                //rigid.angularVelocity = rigid.angularVelocity > 0 ? 200 : -200;
            }
            else
            {
                if (name != "Shell Sprite")
                {
                    Destroy(gameObject);
                } else spriteRenderer.enabled = false; // disable sprite renderer
            }
        }
        else
        {
            if(GetComponent<WeaponAbility>()) AimShooter();
        }
	}

    /// <summary>
    /// Take part damage, if it is damaged too much remove the part
    /// </summary>
    /// <param name="damage">damage to deal</param>
    public void TakeDamage(float damage) {
        currentHealth -= damage;
        if (currentHealth <= 0 && detachible) {
            craft.RemovePart(this);
        }
    }
}
