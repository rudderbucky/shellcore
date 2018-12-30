using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITractorer
{

}
/// <summary>
/// All "human-like" craft are considered ShellCores. These crafts are intelligent and all air-borne. This includes player ShellCores.
/// </summary>
public class ShellCore : AirCraft, IHarvester {

    protected ICarrier carrier;
    protected LineRenderer lineRenderer;
    public GameObject glowPrefab;
    public Material tractorMaterial;
    Transform coreGlow;
    Transform targetGlow;
    Draggable target;
    protected float totalPower;
    protected GameObject bulletPrefab; // prefab for main bullet (should be moved to shellcore) TODO: move to shellcore
    public int intrinsicCommandLimit;
    public SectorManager sectorMngr;
    public List<IOwnable> unitsCommanding = new List<IOwnable>();

    public int GetTotalCommandLimit()
    {
        if (sectorMngr)
        {
            return intrinsicCommandLimit + sectorMngr.GetExtraCommandUnits(faction);
        }
        else return intrinsicCommandLimit;
    }

    public void SetCarrier(ICarrier carrier)
    {
        this.carrier = carrier;
    }


    public float GetPower()
    {
        return totalPower;
    }

    public void AddPower(float power)
    {
        totalPower += power;
    }

    protected override void OnDeath()
    {
        SetTractorTarget(null);
        base.OnDeath();
    }

    private void OnDestroy()
    {
        if(coreGlow)
            Destroy(coreGlow.gameObject);
        if (targetGlow)
            Destroy(targetGlow.gameObject);
    }

    protected override void Start()
    {
        base.Start(); // base start
        intrinsicCommandLimit = 3;
        if (carrier != null && carrier.GetIsInitialized())
        {
            spawnPoint = carrier.GetSpawnPoint();
        }
        transform.position = spawnPoint;
        // initialize instance fields

        if(!coreGlow)
            coreGlow = Instantiate(glowPrefab, null, true).transform;
        if (!targetGlow)
            targetGlow = Instantiate(glowPrefab, null, true).transform;

        coreGlow.gameObject.SetActive(false);
        targetGlow.gameObject.SetActive(false);
    }

    protected override void Respawn()
    {
        if (!(carrier as Entity).GetIsDead() || this as PlayerCore)
        {
            base.Respawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected override void BuildEntity()
    {
        if (!glowPrefab)
            glowPrefab = ResourceManager.GetAsset<GameObject>("glow_prefab");
        if (!tractorMaterial)
            tractorMaterial = ResourceManager.GetAsset<Material>("tractor_material");

        if (!GetComponent<MainBullet>())
        {
            MainBullet mainBullet = gameObject.AddComponent<MainBullet>();
            mainBullet.bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
            mainBullet.terrain = TerrainType.Air;
        }

        GetComponent<MainBullet>().SetActive(true);

        if (!transform.Find("TractorBeam"))
        {
            GameObject childObject = new GameObject();
            childObject.transform.SetParent(transform, false);
            lineRenderer = childObject.AddComponent<LineRenderer>();
            lineRenderer.material = tractorMaterial;
            lineRenderer.startWidth = 0.1F;
            lineRenderer.endWidth = 0.1F;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.sortingOrder = 1;
            childObject.name = "TractorBeam";
        }
        base.BuildEntity();
    }

    protected override void Awake()
    {
        respawns = true;
        base.Awake(); // base awake
    }

    protected void FixedUpdate()
    {
        if (target && !isDead) // Update tractor beam physics
        {
            Rigidbody2D rigidbody = target.GetComponent<Rigidbody2D>();
            if (rigidbody)
            {
                //get direction
                Vector3 dir = transform.position - target.transform.position;
                //get distance
                float dist = dir.magnitude;
                //DebugMeter.AddDataPoint((dir.normalized * (dist - 2F) * 10000f * Time.fixedDeltaTime).magnitude);

                if (target.GetComponent<EnergySphereScript>())
                {
                    rigidbody.AddForce(dir.normalized * 100f);
                }
                else if (dist > 2f)
                {
                    rigidbody.AddForce(dir.normalized * (dist - 2F) * 4000f * Time.fixedDeltaTime * rigidbody.mass / 2);
                }
            }
        }
    }

    protected void TractorBeamUpdate()
    {
        if (!target) // Don't grab energy when the craft is pulling something more important
        {

            EnergySphereScript[] energies = FindObjectsOfType<EnergySphereScript>();

            Transform closest = null;
            float closestD = float.MaxValue;

            for (int i = 0; i < energies.Length; i++)
            {
                float sqrD = Vector3.SqrMagnitude(transform.position - energies[i].transform.position);
                if (closest == null || sqrD < closestD)
                {
                    closestD = sqrD;
                    closest = energies[i].transform;
                }
            }
            if (closest && closestD < 160 && GetTractorTarget() == null)
                SetTractorTarget(closest.gameObject.GetComponent<Draggable>());
        }

        if (target && !isDead && (target.transform.position - transform.position).magnitude < 100
            && (!target.GetComponent<Entity>() || !target.GetComponent<Entity>().GetIsDead())) // Update tractor beam graphics
        {
            lineRenderer.positionCount = 2;
            lineRenderer.sortingOrder = 103;
            lineRenderer.SetPositions(new Vector3[] { transform.position, target.transform.position });

            coreGlow.gameObject.SetActive(true);
            targetGlow.gameObject.SetActive(true);

            coreGlow.transform.position = transform.position;
            targetGlow.transform.position = target.transform.position;
        }
        else
        {
            target = null;
            SetTractorTarget(null);
            lineRenderer.positionCount = 0;
            coreGlow.gameObject.SetActive(false);
            targetGlow.gameObject.SetActive(false);
        }
    }
    protected override void Update() {
        base.Update(); // base update
        TractorBeamUpdate();
    }

    public void SetTractorTarget(Draggable newTarget)
    {
        lineRenderer.enabled = (newTarget != null);
        if(target)
            target.dragging = false;
        target = newTarget;
        if (target)
            target.dragging = true;
    }

    public Draggable GetTractorTarget()
    {
        return target;
    }
}
