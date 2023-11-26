using UnityEngine;

public class IonLineController : MonoBehaviour
{
    public LineRenderer line;
    private Material material; // material used by the line renderer
    float beamBearing;
    public WeaponTargetingSystem targetingSystem;
    public float range;
    public Entity Core;
    public bool initialized;
    GameObject hitPrefab;
    ShellPart part;
    float duration;
    float tier;

    float startWidth = 0f;
    float poweredWidth = 0.3F;

    float energyCost;
    float damage;
    public static float damageC = 1500;
    public static float energyC = 150;
    private Entity.TerrainType terrain;
    public bool gasBoosted;

    public void SetTerrain(Entity.TerrainType terrain)
    {
        this.terrain = terrain;
    }
 

    public float GetBeamAngle() 
    {
        return beamBearing;
    }


    public void Awake()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.sortingLayerName = "Projectiles";
        line.material = material;
        line.startWidth = line.endWidth = 0;
        line.useWorldSpace = true;
        var col = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction) : new Color(0.8F, 1F, 1F, 0.9F);
        Gradient gradient = new Gradient();
        gradient.mode = GradientMode.Fixed;
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(col, 0), new GradientColorKey(col, 1) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.5F, 0), new GradientAlphaKey(1F, 0.1F), new GradientAlphaKey(1, 1) }
        );
        line.colorGradient = gradient;
    }

    public void StartFiring(float duration)
    {
        if (targetingSystem == null || !targetingSystem.GetTarget() || !line) return;
        this.duration = duration;
        line.startWidth = line.endWidth = startWidth;

        var pos = targetingSystem.GetTarget().position;
        var vec = (pos - line.gameObject.transform.position).normalized;
        var targetBearing = GetBearingFromVector(vec);
        beamBearing = targetBearing;
    }

    public void Init(WeaponTargetingSystem targetingSystem, Entity core, float range, ShellPart part, int tier, Entity.TerrainType terrain)
    {
        this.targetingSystem = targetingSystem;
        this.Core = core;
        initialized = true;
        this.range = range;
        this.part = part;
        this.tier = tier;
        energyCost = energyC * tier;
        damage = damageC * tier;
        SetTerrain(terrain);
    }

    public bool GetFiring()
    {
        return duration > 0;
    }

    void Start()
    {
        SetMaterial(ResourceManager.GetAsset<Material>("white_material"));
    }

    public void SetMaterial(Material material)
    {
        this.material = material;
        line.material = material;
    }

    public void ThickenLine(float amount)
    {
        line.startWidth = line.endWidth += amount;
        if (line.startWidth > poweredWidth)
        {
            line.startWidth = line.endWidth = poweredWidth;
        }

        if (line.startWidth < 0)
        {
            line.startWidth = line.endWidth = 0;
            line.positionCount = 0;
        }
    }

    private readonly float TURN_RATE = 65;
    private float UpdateBearing()
    {
        var pos = targetingSystem.GetTarget().position;
        var vec = (pos - line.gameObject.transform.position).normalized;
        //var angle = Mathf.Atan(vec.y / vec.x);
        var targetBearing = GetBearingFromVector(vec);


        // vec = (GetMousePos() - transform.position).normalized;
        var originalBearing = beamBearing;

        var diff = targetBearing - originalBearing;

        var c = TURN_RATE * Time.deltaTime;
        if (gasBoosted) c *= 2;
        bool goForwards = false;

        if (originalBearing < 180)
        {
            goForwards = targetBearing - originalBearing < 180 && targetBearing - originalBearing > 0;
        }
        else
        {
            var limit = originalBearing + 180 - 360;
            goForwards = targetBearing < 180 ? (targetBearing < limit) : (targetBearing > originalBearing);
        }

        if (Mathf.Abs(diff) <= c)
        {
            originalBearing = targetBearing;
        }
        else
        {
            originalBearing += goForwards ? c : -c;
        }

        beamBearing = originalBearing;
        if (beamBearing > 360)
        {
            beamBearing -= 360;
        }

        if (beamBearing < 0)
        {
            beamBearing += 360;
        }

        return originalBearing;
    }

    void Update()
    {
        if (Time.timeScale <= 0) return;
        line.gameObject.transform.position = gameObject.transform.position;
        float originalBearing = 0;
        if (initialized && targetingSystem.GetTarget())
        {
            originalBearing = UpdateBearing();
        }
        if (initialized && targetingSystem.GetTarget() && !Core.IsInvisible && duration > 0)
        {
            if (Core.GetHealth()[2] < energyCost * Time.deltaTime)
            {
                duration = 0;
                return;
            }

            duration -= Time.deltaTime;
            line.positionCount = 2;
            line.SetPosition(0, line.gameObject.transform.position);


            // Debug.LogError(angle * Mathf.Rad2Deg + " " + GetBearingFromVector(vec) + " " + GetAngleFromBearing(GetBearingFromVector(vec)));
            line.SetPosition(1, transform.position + GetVectorByBearing(originalBearing) * range);
            ThickenLine(0.005F);

            var dps = damage * Time.deltaTime;

            var damageable = CollisionManager.RaycastDamageable(transform.position, transform.position + GetVectorByBearing(originalBearing) * range, VerifyTarget, out var point);
            if (damageable != null)
            {
                var hitTransform = damageable.GetTransform();

                var magnitude = (point - (Vector2)transform.position).magnitude;
                line.SetPosition(1, transform.position + GetVectorByBearing(originalBearing) * magnitude);
                Core.TakeEnergy(energyCost * Time.deltaTime);


                ShellPart part = null;
                var dEnt = damageable as Entity;
                var colliders = dEnt.GetColliders();
                if (dEnt)
                {
                    for (int i = 0; i < dEnt.parts.Count; i++)
                    {
                        if (SATCollision.PointInRectangle(
                            colliders[i * 4 + 0],
                            colliders[i * 4 + 1],
                            colliders[i * 4 + 2],
                            colliders[i * 4 + 3],
                            line.GetPosition(1)))
                        {
                            part = dEnt.parts[i];
                            break;
                        }
                    }
                }

                var residue = damageable.TakeShellDamage(dps, 0, GetComponentInParent<Entity>());
                // deal instant damage

                if (part)
                {
                    part.TakeDamage(residue);
                }
            }

            if (!hitPrefab)
            {
                hitPrefab = ResourceManager.GetAsset<GameObject>("weapon_hit_particle");
            }

            if (line.positionCount > 1)
            {
                Instantiate(hitPrefab, line.GetPosition(1), Quaternion.identity); // instantiate hit effect
            }
        }
        else
        {
            if (!targetingSystem.GetTarget() && duration > 0)
            {
                duration = 0;
            }

            if (line.startWidth > startWidth)
            {
                ThickenLine(-0.01F);
            }

            if (duration <= 0)
            {
                if (transform.parent.GetComponentInChildren<AudioSource>())
                {
                    Destroy(transform.parent.GetComponentInChildren<AudioSource>().gameObject);
                }
            }
        }
    }

    bool VerifyTarget(Entity entity)
    {
        return entity.GetFaction() != Core.faction && !entity.GetIsDead() && !entity.IsInvisible && (entity.GetTerrain() == terrain || terrain == Entity.TerrainType.All);
    }

    public float GetDuration()
    {
        return duration;
    }

    float GetBearingFromVector(Vector2 vec)
    {
        var angle = Mathf.Atan(vec.y / vec.x) * Mathf.Rad2Deg;
        if (vec.x > 0)
        {
            if (angle > 0)
            {
                return angle;
            }
            else
            {
                return 360 + angle;
            }
        }
        else
        {
            if (angle > 0)
            {
                return 180 + angle;
            }
            else
            {
                return 180 + angle;
            }
        }
    }

    float GetAngleFromBearing(float bearing)
    {
        if (bearing < 90)
        {
            return bearing;
        }
        else if (bearing < 180)
        {
            return bearing - 180;
        }
        else if (bearing < 270)
        {
            return bearing - 180;
        }

        return bearing - 360;
    }

    Vector3 GetVectorByBearing(float bearing)
    {
        var xsign = bearing < 90 || bearing > 270 ? 1 : -1;
        var angle = GetAngleFromBearing(bearing) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angle) * xsign, Mathf.Sin(angle) * xsign);
    }

    Vector3 GetVectorByAngle(Vector2 vec, float angle)
    {
        var xsign = (vec.x - transform.position.x);
        return new Vector3(Mathf.Cos(angle) * Mathf.Sign(xsign), Mathf.Sin(angle) * Mathf.Sign(xsign));
    }
}
