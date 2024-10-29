using Unity.Netcode;
using UnityEngine;

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
    [SerializeField]
    Draggable target;
    private float energyPickupTimer = 10.0f; // Energy pickup timer
    protected float energyPickupSpeed = 61.0f;
    public bool initialized;
    private bool energyEnabled = true;
    private GameObject tractorBeamPrefab;
    private ulong finalTractorForFrame;
    private bool serverTractorDirty = true;
    private GameObject auxillaryParticleSystem;

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
        lineRenderer.startWidth = 0.05F;
        lineRenderer.endWidth = 0.05F;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.sortingOrder = 1;
        lineRenderer.sortingLayerName = "Projectiles";
        childObject.name = "TractorBeam";
        tractorBeamPrefab = childObject;
        auxillaryParticleSystem = Instantiate(ResourceManager.GetAsset<GameObject>("tractor_specialfx"), childObject.transform);
        auxillaryParticleSystem.SetActive(false);
        lineRenderer.positionCount = 0;
        initialized = true;
    }

    private void Update()
    {
        if (initialized)
        {
            TractorBeamUpdate();
            if (!queueServerCall || 
                (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client || MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Off)) return;
            if (target && EntityNetworkAdapter.GetNetworkId(target.transform) != ulong.MaxValue)
            {
                owner.networkAdapter.SetTractorID(EntityNetworkAdapter.GetNetworkId(target.transform));
                queueServerCall = false;
            }
            else 
            {
                owner.networkAdapter.SetTractorID(null);
                queueServerCall = false;
            }
        }
    }

    protected void FixedUpdate()
    {
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client) return;
        if (!IsValidDraggableTarget(target))
        {
            SetTractorTarget(null); // Make sure that you are still allowed to tractor the target
        }

        if (target && !owner.GetIsDead() && MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Client) // Update tractor beam physics
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
                        SetTractorTarget(null);
                    }
                    else
                    {
                        rigidbody.position += (Vector2)dir.normalized * 0.6F;
                    }

                    if (owner.IsInvisible)
                    {
                        SetTractorTarget(null);
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

    protected void LateUpdate()
    {
        if (coreGlow)
            coreGlow.transform.position = transform.position;
        if (targetGlow && target)
            targetGlow.transform.position = target.transform.position;
    }

    private float auxillaryScaleY = 0.6F;
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
                if (!energies[i]) continue;
                float sqrD = Vector3.SqrMagnitude(transform.position - energies[i].transform.position);
                if ((closest == null || sqrD < closestD) && !energies[i].GetComponent<Draggable>().Dragging)
                {
                    closestD = sqrD;
                    closest = energies[i].transform;
                }
            }

            if (closest && closestD < energyPickupRangeSquared && target == null && !closest.gameObject.GetComponent<Draggable>().Dragging && !MasterNetworkAdapter.lettingServerDecide)
            {
                SetTractorTarget(closest.gameObject.GetComponent<Draggable>());
            }

            this.energyPickupTimer = 0.0f; // Can change this to a non-zero value to add the timing element back
        }

        if ((target && !owner.GetIsDead() && (!target.GetComponent<Entity>() || !target.GetComponent<Entity>().GetIsDead()))) // Update tractor beam graphics
        {
            if (!forcedTarget && (target.transform.position - transform.position).sqrMagnitude > maxBreakRangeSquared && !(owner as Yard) && MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Client)
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
                var x = auxillaryParticleSystem.GetComponentInChildren<ParticleSystem>().main;
                x.startColor = new ParticleSystem.MinMaxGradient(owner.tractorSwitched ? new Color32(255, 32, 255, 128) : new Color32(88, 239, 255, 128));
                auxillaryParticleSystem.SetActive(true);
                auxillaryParticleSystem.transform.position = Vector3.Lerp(transform.position, target.transform.position, 0.5F);
                auxillaryParticleSystem.transform.localScale = new Vector3((target.transform.position - transform.position).magnitude/0.2F, auxillaryScaleY, 1);
                auxillaryParticleSystem.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 360 - Vector2.SignedAngle(target.transform.position - transform.position, Vector2.right)));// = new Vector3(0, 0, 0);
            }
        }
        else
        {
            SetTractorTarget(null);
            lineRenderer.positionCount = 0;
            coreGlow.gameObject.SetActive(false);
            targetGlow.gameObject.SetActive(false);
            auxillaryParticleSystem.SetActive(false);
        }

        if (serverTractorDirty && owner.networkAdapter)
        {
            serverTractorDirty = false;
            owner.networkAdapter.UpdateTractorClientRpc(finalTractorForFrame, finalTractorForFrame == ulong.MaxValue);
        }
    }

    public void SetIdToTractor(ulong id)
    {
        finalTractorForFrame = id;
        serverTractorDirty = true;
    }

    private bool queueServerCall = false;
    public void SetTractorTarget(Draggable newTarget, bool fromClient = false, bool fromServer = false)
    {
        if (target == newTarget) return;
        if (newTarget && owner.GetIsDead()) return;
        var targetComp = target != null && target ? target?.GetComponent<ShellPart>() : null;
        if (!newTarget && target && targetComp && !AIData.strayParts.Contains(targetComp))
        {
            AIData.strayParts.Add(targetComp);
        }

        if (IsValidDraggableTarget(newTarget) || fromServer)
        {
            if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client && owner as PlayerCore && !fromServer)
            {
                owner.networkAdapter.RequestTractorUpdateServerRpc(EntityNetworkAdapter.GetNetworkId(newTarget ? newTarget.transform : null), !EntityNetworkAdapter.TransformIsNetworked(newTarget ? newTarget.transform : null));
                return;
            }

            if (lineRenderer)
            {
                lineRenderer.enabled = (newTarget != null);
            }
            
            if (target)
            {
                target.RemoveDrag();
            }

            var oldTarget = target;
            target = newTarget;
            if (owner && owner.networkAdapter && (target || (!target && oldTarget)))
            {
                if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Client && !fromClient)
                {
                    queueServerCall = true;
                }
            }

            if (target && ((MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Client && (!owner.tractorSwitched || !target.GetComponent<Entity>())) || fromServer))
            {
                target.AddDrag();
            }

            if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && !MasterNetworkAdapter.lettingServerDecide && 
                target != oldTarget && owner.networkAdapter && EntityNetworkAdapter.TransformIsNetworked(newTarget ? newTarget.transform : null))
            {
                finalTractorForFrame = EntityNetworkAdapter.GetNetworkId(newTarget ? newTarget.transform : null);
                serverTractorDirty = true;
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

        if ((newTarget.GetComponent<EnergySphereScript>() && newTarget.Dragging) && !(target == newTarget))
        {
            return false;
        }
        
        return InvertTractorCheck(owner, newTarget);
    }

    public static bool InvertTractorCheck(Entity owner, Draggable newTarget)
    {
        Entity requestedTarget = newTarget.gameObject.GetComponent<Entity>();
        if (owner.tractorSwitched || !requestedTarget || (FactionManager.IsAllied(requestedTarget.faction, owner.faction) && (requestedTarget.isStandardTractorTarget)))
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

        if (target && target.GetComponentInChildren<Draggable>())
        {
            target.GetComponentInChildren<Draggable>().RemoveDrag();
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
