using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHarvester : ITractorer
{
    void AddPower(float power);
}

public class Harvester : WeaponAbility, IHarvester {

    protected LineRenderer lineRenderer;
    public GameObject glowPrefab;
    public Material tractorMaterial;
    Transform coreGlow;
    Transform targetGlow;
    Draggable target;
    public ShellCore owner;

    private void OnDestroy()
    {
        if (coreGlow)
            Destroy(coreGlow.gameObject);
        if (targetGlow)
            Destroy(targetGlow.gameObject);
    }

    protected void Start()
    {
        ID = 16;
        if (!glowPrefab)
            glowPrefab = ResourceManager.GetAsset<GameObject>("glow_prefab");
        if (!tractorMaterial)
            tractorMaterial = ResourceManager.GetAsset<Material>("tractor_material");
        if(!owner)
        {
            owner = (Core as Turret).owner as ShellCore;
        }
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

        if (!coreGlow)
            coreGlow = Instantiate(glowPrefab, null, true).transform;
        if (!targetGlow)
            targetGlow = Instantiate(glowPrefab, null, true).transform;

        coreGlow.gameObject.SetActive(false);
        targetGlow.gameObject.SetActive(false);
    }

    protected override bool Execute(Vector3 victimPos)
    {
        return true;
    }

    protected void FixedUpdate()
    {
        if (target && owner && !Core.GetIsDead()) // Update tractor beam physics
        {
            Rigidbody2D rigidbody = target.GetComponent<Rigidbody2D>();
            if (rigidbody)
            {
                //get direction
                Vector3 dir = transform.position - target.transform.position;
                //get distance
                float dist = dir.magnitude;
                //DebugMeter.AddDataPoint((dir.normalized * (dist - 2F) * 10000f * Time.fixedDeltaTime).magnitude);

                rigidbody.position += (Vector2)dir.normalized * 0.6F;
            }
        }
    }

    protected void Update()
    {
        TractorBeamUpdate();
    }

    protected void TractorBeamUpdate()
    {
        if (!target && owner) // Don't grab energy when the craft is pulling something more important
        {

            EnergySphereScript[] energies = FindObjectsOfType<EnergySphereScript>();

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
        }

        if (target && !Core.GetIsDead() && (target.transform.position - transform.position).sqrMagnitude < 200
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
            SetTractorTarget(null);
            lineRenderer.positionCount = 0;
            coreGlow.gameObject.SetActive(false);
            targetGlow.gameObject.SetActive(false);
        }
    }

    public void SetTractorTarget(Draggable newTarget)
    {
        lineRenderer.enabled = (newTarget != null);
        if (target)
            target.dragging = false;
        target = newTarget;
        if (target)
            target.dragging = true;
    }

    public void AddPower(float power)
    {
        owner.AddPower(power);
    }

    public void SetOwner(ShellCore owner)
    {
        this.owner = owner;
    }

    public Draggable GetTractorTarget()
    {
        return target;
    }
}
