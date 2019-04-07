using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITractorer
{

}
/// <summary>
/// All "human-like" craft are considered ShellCores. These crafts are intelligent and all air-borne. This includes player ShellCores.
/// </summary>
public class ShellCore : AirCraft, IHarvester, IOwner {

    protected ICarrier carrier;
    protected LineRenderer lineRenderer;
    public GameObject glowPrefab;
    public Material tractorMaterial;
    Transform coreGlow;
    Transform targetGlow;
    Draggable target;
    protected float totalPower;
	private float energyPickupTimer = 10.0f; // Energy pickup timer
	protected float energyPickupSpeed = 61.0f; // Disabled for now D: (60*FixedDeltatime = 10) Energy pickup rate scale for future hard/easy gamemodes and AI balancing only.
    protected GameObject bulletPrefab; // prefab for main bullet (should be moved to shellcore)
    public int intrinsicCommandLimit;
    public List<IOwnable> unitsCommanding = new List<IOwnable>();

    private AirCraftAI ai;

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

    public void ResetPower() {
        totalPower = 0;
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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if(coreGlow)
            Destroy(coreGlow.gameObject);
        if (targetGlow)
            Destroy(targetGlow.gameObject);
    }

    public SectorManager GetSectorManager() {
        return sectorMngr;
    }
    protected override void Start()
    {
        intrinsicCommandLimit = 3;
        if ((carrier != null && !carrier.Equals(null)) && carrier.GetIsInitialized())
        {
            spawnPoint = carrier.GetSpawnPoint();
        }
        transform.position = spawnPoint;
        // initialize instance fields
        base.Start(); // base start
        if(!coreGlow)
            coreGlow = Instantiate(glowPrefab, null, true).transform;
        if (!targetGlow)
            targetGlow = Instantiate(glowPrefab, null, true).transform;

        coreGlow.gameObject.SetActive(false);
        targetGlow.gameObject.SetActive(false);

        if(!(this as PlayerCore) && !ai)
        {
            ai = gameObject.AddComponent<AirCraftAI>();
        }
        if(ai)
        {
            ai.Init(this);
            if(sectorMngr.current.type == Sector.SectorType.BattleZone)
            {
                ai.setMode(AirCraftAI.AIMode.Battle);
            }
            else
            {
                ai.setMode(AirCraftAI.AIMode.Inactive);
            }
            ai.allowRetreat = true;
        }
    }

    protected override void Respawn()
    {
        if ((carrier != null && !(carrier as Entity).GetIsDead()) || this as PlayerCore)
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

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
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
                    rigidbody.position += (Vector2)dir.normalized * 0.6F;
                }
                else if (dist > 2f)
                {
                    rigidbody.AddForce(dir.normalized * (dist - 2F) * 2400F * Time.fixedDeltaTime * rigidbody.mass / 2);
                }
            }
        }
    }

    protected void TractorBeamUpdate()
    {
		this.energyPickupTimer -= Time.fixedDeltaTime * this.energyPickupSpeed;
        if ((!target) && (this.energyPickupTimer < 0)) // Grab energy automatically after a while when the craft is not pulling something more important
        {
            EnergySphereScript[] energies = AIData.energySpheres.ToArray();

            Transform closest = null;
            float closestD = float.MaxValue;

            for (int i = 0; i < energies.Length; i++)
            {
                float sqrD = Vector3.SqrMagnitude(transform.position - energies[i].transform.position);
                if ((closest == null || sqrD < closestD) && !energies[i].GetComponent<Draggable>().dragging)
                {
                    closestD = sqrD;
                    closest = energies[i].transform;
                }
            }
            if (closest && closestD < 160 && GetTractorTarget() == null)
                SetTractorTarget(closest.gameObject.GetComponent<Draggable>());
			this.energyPickupTimer = 0.0f; // Can change this to a non-zero value to add the timing element back
        }

        if (target && !isDead && (!target.GetComponent<Entity>() || !target.GetComponent<Entity>().GetIsDead())) // Update tractor beam graphics
        {
            if((target.transform.position - transform.position).sqrMagnitude > 600) 
            {
                SetTractorTarget(null); // break tractor if too far away
            } else 
            {
                lineRenderer.positionCount = 2;
                lineRenderer.sortingOrder = 103;
                lineRenderer.SetPositions(new Vector3[] { transform.position, target.transform.position });

                coreGlow.gameObject.SetActive(true);
                targetGlow.gameObject.SetActive(true);

                coreGlow.transform.position = transform.position;
                targetGlow.transform.position = target.transform.position;
            }
        }
        else
        {
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
        if (newTarget && (newTarget.transform.position - transform.position).sqrMagnitude > 400)
            return;
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

    public int GetFaction()
    {
        return faction;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public List<IOwnable> GetUnitsCommanding()
    {
        return unitsCommanding;
    }
}
