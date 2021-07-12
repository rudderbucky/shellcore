﻿using UnityEngine;

// Tractor beam wrapper class.
public class TractorBeam : MonoBehaviour
{
    public int maxRangeSquared = 225;
    public int energyPickupRangeSquared = 160;
    public int maxBreakRangeSquared = 600;
    protected LineRenderer lineRenderer;
    public GameObject glowPrefab;
    public Material tractorMaterial;
    Transform coreGlow;
    Transform targetGlow;
    public Entity owner;
    Draggable target;
    private float energyPickupTimer = 10.0f; // Energy pickup timer
    protected float energyPickupSpeed = 61.0f;
    public bool initialized;
    private bool energyEnabled = true;
    private GameObject tractorBeamPrefab;

    public void SetEnergyEnabled(bool val)
    {
        energyEnabled = val;
    }

    public void BuildTractor()
    {
        if (!glowPrefab)
        {
            glowPrefab = ResourceManager.GetAsset<GameObject>("glow_prefab");
        }

        if (!tractorMaterial)
        {
            tractorMaterial = ResourceManager.GetAsset<Material>("tractor_material");
        }

        if (!coreGlow)
        {
            coreGlow = Instantiate(glowPrefab, null, true).transform;
        }

        if (owner as Drone)
        {
            coreGlow.localScale *= 0.5F;
        }

        if (!targetGlow)
        {
            targetGlow = Instantiate(glowPrefab, null, true).transform;
        }

        coreGlow.gameObject.SetActive(false);
        targetGlow.gameObject.SetActive(false);

        GameObject childObject = new GameObject();
        //childObject.transform.SetParent(transform, false); Unity ignores sorting layers if uncommented
        lineRenderer = childObject.AddComponent<LineRenderer>();
        lineRenderer.material = tractorMaterial;
        lineRenderer.material.color = new Color32(88, 239, 255, 128);
        //lineRenderer.material.color = new Color32(255,32,255,128);
        lineRenderer.startWidth = 0.1F;
        lineRenderer.endWidth = 0.1F;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.sortingOrder = 1;
        lineRenderer.sortingLayerName = "Projectiles";
        childObject.name = "TractorBeam";
        tractorBeamPrefab = childObject;
        initialized = true;
    }

    private void Update()
    {
        if (initialized)
        {
            TractorBeamUpdate();
        }
    }

    protected void FixedUpdate()
    {
        if (!IsValidDraggableTarget(target))
        {
            SetTractorTarget(null); // Make sure that you are still allowed to tractor the target
        }

        if (target && !owner.GetIsDead()) // Update tractor beam physics
        {
            Rigidbody2D rigidbody = target.GetComponent<Rigidbody2D>();
            if (rigidbody)
            {
                //get direction
                Vector3 dir = transform.position - target.transform.position;
                //get distance
                float dist = dir.magnitude;
                //DebugMeter.AddDataPoint((dir.normalized * (dist - 2F) * 10000f * Time.fixedDeltaTime).magnitude);

                if (target.GetComponent<EnergySphereScript>() || owner as Yard)
                {
                    if (dir.sqrMagnitude <= 0.36F)
                    {
                        rigidbody.position = transform.position;
                        rigidbody.velocity = Vector2.zero;
                        target = null;
                    }
                    else
                    {
                        rigidbody.position += (Vector2)dir.normalized * 0.6F;
                    }

                    if (owner.IsInvisible)
                    {
                        target = null;
                    }
                }
                else if (dist > 2f)
                {
                    if (!owner.tractorSwitched)
                    {
                        rigidbody.AddForce(dir.normalized * (dist - 2F) * rigidbody.mass * tractorStrength / Time.fixedDeltaTime);
                    }
                    else
                    {
                        owner.GetComponent<Rigidbody2D>().AddForce(-dir.normalized * (dist - 2F) *
                            rigidbody.mass * tractorStrength / Time.fixedDeltaTime);
                    }
                }
            }
        }
    }

    private static float tractorStrength = 1.5F;

    protected void TractorBeamUpdate()
    {
        lineRenderer.material.color = owner.tractorSwitched ? new Color32(255, 32, 255, 128) : new Color32(88, 239, 255, 128);
        this.energyPickupTimer -= Time.fixedDeltaTime * this.energyPickupSpeed;
        // Grab energy automatically after a while when the craft is not pulling something more important
        if (energyEnabled && (!target) && (this.energyPickupTimer < 0) && !owner.IsInvisible && !owner.isAbsorbing)
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

            if (closest && closestD < energyPickupRangeSquared && target == null)
            {
                SetTractorTarget(closest.gameObject.GetComponent<Draggable>());
            }

            this.energyPickupTimer = 0.0f; // Can change this to a non-zero value to add the timing element back
        }

        if ((target && !owner.GetIsDead() && (!target.GetComponent<Entity>() || !target.GetComponent<Entity>().GetIsDead()))) // Update tractor beam graphics
        {
            if (!forcedTarget && (target.transform.position - transform.position).sqrMagnitude > maxBreakRangeSquared && !(owner as Yard))
            {
                SetTractorTarget(null); // break tractor if too far away
            }
            else
            {
                lineRenderer.positionCount = 2;
                lineRenderer.sortingOrder = 103;
                lineRenderer.SetPositions(new Vector3[] {transform.position, target.transform.position});

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

    public void SetTractorTarget(Draggable newTarget)
    {
        //if (target != null && newTarget == null && owner.faction != 0)
        //{
        //    Debug.Log("AI Dropped something!");
        //}
        var targetComp = target != null && target ? target?.GetComponent<ShellPart>() : null;
        if (!newTarget && target && targetComp && !AIData.strayParts.Contains(targetComp))
        {
            AIData.strayParts.Add(targetComp);
        }

        if (IsValidDraggableTarget(newTarget))
        {
            if (lineRenderer)
            {
                lineRenderer.enabled = (newTarget != null);
            }

            if (target)
            {
                target.dragging = false;
            }

            target = newTarget;
            if (target)
            {
                target.dragging = true;
            }
        }
    }

    private bool IsValidDraggableTarget(Draggable newTarget)
    {
        if (forcedTarget || !newTarget)
        {
            return true;
        }

        if ((newTarget.transform.position - transform.position).sqrMagnitude > maxRangeSquared && !(owner as Yard))
        {
            return false;
        }

        return InvertTractorCheck(owner, newTarget);
    }

    public static bool InvertTractorCheck(Entity owner, Draggable newTarget)
    {
        Entity requestedTarget = newTarget.gameObject.GetComponent<Entity>();
        if (owner.tractorSwitched || !requestedTarget || ((requestedTarget.faction == owner.faction) && (requestedTarget is Drone || requestedTarget is Tank || requestedTarget is Turret)))
        {
            return true;
        }

        return false;
    }

    public Draggable GetTractorTarget()
    {
        return target;
    }

    protected void OnDestroy()
    {
        if (coreGlow)
        {
            Destroy(coreGlow.gameObject);
        }

        if (targetGlow)
        {
            Destroy(targetGlow.gameObject);
        }

        if (tractorBeamPrefab)
        {
            Destroy(tractorBeamPrefab);
        }
    }

    bool forcedTargetHadDraggable = false;
    Transform forcedTarget;

    public void ForceTarget(Transform obj)
    {
        if (!initialized)
        {
            BuildTractor();
        }

        if (obj == null)
        {
            if (forcedTarget && !forcedTargetHadDraggable)
            {
                Destroy(forcedTarget.GetComponent<Draggable>());
            }

            forcedTarget = null;
            forcedTargetHadDraggable = false;
            SetTractorTarget(null);
        }
        else
        {
            forcedTargetHadDraggable = obj.GetComponentInChildren<Draggable>();
            forcedTarget = obj;
            if (!forcedTargetHadDraggable)
            {
                obj.gameObject.AddComponent<Draggable>();
            }

            SetTractorTarget(obj.GetComponentInChildren<Draggable>());
        }
    }
}
