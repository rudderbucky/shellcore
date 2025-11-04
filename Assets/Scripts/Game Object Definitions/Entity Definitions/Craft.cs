using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Every entity that can move is a craft. This includes drones and ShellCores.
/// </summary>
public abstract class Craft : Entity
{
    protected int pins = 0;
    protected bool forceImmobile = false;

    public virtual bool isImmobile
    {
        get { return pins > 0 || forceImmobile || isAbsorbing || isDead; }
        set { forceImmobile = true; }
    }

    protected bool respawns; // whether the craft respawns or not

    public static readonly float initSpeed = 40;

    public float speed = initSpeed;
    public float accel = 25;
    public float physicsSpeed;
    public float physicsAccel;
    public static readonly float weightNumeratorConstant = 40;

    Vector2 oldPosition = Vector2.zero;

    public void AddPin()
    {
        pins++;
    }

    public void RemovePin()
    {
        pins--;
        if (pins < 0)
        {
            Debug.LogError($"Negative pins! {name}");
            pins = 0;
        }
    }

    public static float GetPhysicsSpeed(float speed, float weight)
    {
        return 0.25F * (speed * (weightNumeratorConstant / weight) + 0.2f * speed);
    }

    public void CalculatePhysicsConstants()
    {
        physicsSpeed = GetPhysicsSpeed(speed, weight);
        if (SpeedAuraStacks > 0 && blueprint.intendedType != EntityBlueprint.IntendedType.Tank)
        {
            physicsSpeed *= 2.5F;
        }
        accel = 0.5F * speed;
        physicsAccel = accel * (0.5F * weightNumeratorConstant / weight) + 0.1f * accel;
        physicsAccel *= 5F;
    }

    protected Vector2 physicsDirection = Vector2.zero;

    public void SetImmobile(bool val)
    {
        isImmobile = val;
    }

    protected override void BuildEntity()
    {
        DestroyOldParts();
        speed = initSpeed;
        base.BuildEntity();
        CalculatePhysicsConstants();
    }

    protected override void PostDeath()
    {
        if (respawns)
        {
            Respawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected override void Update()
    {
        if (SystemLoader.AllLoaded && instantiatedRespawnPrefab) // graphics code, should update position in Update instead of FixedUpdate
        {
            instantiatedRespawnPrefab.position = transform.position;
        }

        base.Update();
    }

    /// <summary>
    /// Called to respawn this craft to its spawn point
    /// </summary>
    public virtual void Respawn(bool force = false)
    {
        // no longer dead or immobile
        isDead = false;

        // Deactivate abilities
        if (abilities != null)
            foreach (var ability in abilities)
            {
                if (ability is ActiveAbility || ability is PassiveAbility)
                {
                    ability.SetDestroyed(true);
                }
            }

        transform.rotation = Quaternion.identity; // reset rotation so part rotation can be reset
        foreach (Transform child in transform)
        {
            // reset all the children rotations
            child.transform.rotation = Quaternion.identity;
            child.transform.localPosition = Vector3.zero;
            var tmp = child.gameObject.GetComponent<ShellPart>();

            if (tmp)
            {
                // if part exists
                tmp.Start(); // initialize it
            }
        }

        Start(); // once everything else is done initialize the craft again

        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && MasterNetworkAdapter.lettingServerDecide && networkAdapter)
        {
            networkAdapter.clientReady = false;
        }

        AudioManager.PlayClipByID("clip_respawn", transform.position);
    }

    protected override void Start()
    {
        base.Start();
        Category = EntityCategory.Unit;
        instantiatedRespawnPrefab = Instantiate(respawnImplosionPrefab).transform;
        instantiatedRespawnPrefab.position = transform.position;
    }

    /// <summary>
    /// The method that moves the craft based on the integer input it receives
    /// </summary>
    /// <param name="direction">integer that specifies the direction of movement</param>
    public void MoveCraft(Vector2 direction)
    {
        if (!isImmobile) // check for immobility
        {
            physicsDirection = direction;
        }
    }

    protected Transform instantiatedRespawnPrefab;

    protected override void FixedUpdate()
    {
        if (!SystemLoader.AllLoaded) return;
        var lettingServerDecide = MasterNetworkAdapter.lettingServerDecide;

        entityBody.drag = draggable.Dragging  ? 25F : 0;
        if (draggable.Dragging)
        {
            return;
        }

        restAccel = Vector3.zero;

        if (!GetIsDead())
        {
            foreach (var gas in AIData.gas)
            {
                var radius = gas.radius;
                if (Vector2.SqrMagnitude(transform.position - gas.transform.position) < radius * radius)
                {
                    var diff = radius * radius - Vector2.SqrMagnitude(transform.position - gas.transform.position);
                    if (this is PlayerCore core)
                    { 
                        core.cursave.gas += Mathf.Pow(diff, 0.75F) * Time.deltaTime / 10;
                        if (CoreScriptsManager.instance && CoreScriptsManager.OnVariableUpdate != null)
                        {
                            CoreScriptsManager.OnVariableUpdate.Invoke("Gas()");
                        }
                        gas.Shrink(diff * Time.deltaTime / (radius * radius));
                        ShardCountScript.DisplayCount();
                    }
                    var vec = (Vector2)(gas.transform.position - transform.position);
                    restAccel += (vec.normalized * Mathf.Sqrt(diff)) * 2F;
                    var normal = Vector2.Dot(Vector2.Perpendicular(vec), entityBody.velocity);
                }
            }
        }
        


        if (physicsDirection == Vector2.zero && restAccel == Vector2.zero)
        {
            var dir = entityBody.velocity.normalized;
            entityBody.velocity -= entityBody.velocity.normalized * physicsAccel * Time.fixedDeltaTime;
            if (dir != entityBody.velocity.normalized && restAccel == Vector2.zero)
            {
                entityBody.velocity = Vector2.zero;
            }
        }

        CraftMover(physicsDirection); // move craft
        if (!lettingServerDecide)
        {
        }
        physicsDirection = Vector2.zero;
    }

    /// <summary>
    /// Rotates the craft to the passed vector
    /// </summary>
    /// <param name="directionVector">direction vector to rotate the craft to</param>
    public void RotateCraft(Vector2 directionVector)
    {
        //no need to do anything if there's no movement
        if (directionVector == Vector2.zero)
        {
            return;
        }

        float craftAngle = Mathf.Atan2(entityBody.transform.up.y, entityBody.transform.up.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(directionVector.y, directionVector.x) * Mathf.Rad2Deg;

        float angleDistance = Mathf.Abs(Mathf.DeltaAngle(craftAngle, targetAngle));
        float rotationAmount = Mathf.Min(physicsAccel * Time.deltaTime * 4, 45f);

        if (rotationAmount > angleDistance)
        {
            entityBody.transform.eulerAngles = new Vector3(0, 0, targetAngle - 90);
            return;
        }

        float finalAngle = Mathf.LerpAngle(craftAngle, targetAngle, rotationAmount / angleDistance);
        entityBody.transform.eulerAngles = new Vector3(0, 0, finalAngle - 90);
    }

    public bool rotateWhileMoving = true;
    protected const float maxVelocity = 40f;
    protected Vector2 restAccel;
    

    /// <summary>
    /// Applies a force to the craft on the given vector
    /// </summary>
    protected virtual void CraftMover(Vector2 directionVector)
    {
        if (isImmobile)
        {
            entityBody.velocity = restAccel;
            return;
        }

        if (rotateWhileMoving && !MasterNetworkAdapter.lettingServerDecide)
        {
            RotateCraft(directionVector / weight);
        }

        entityBody.velocity = CalculateNewVelocity(directionVector);

        if (((Vector2)transform.position - oldPosition).sqrMagnitude > 2f)
        {
            oldPosition = transform.position;
            LandPlatformGenerator.EnqueueEntity(this);
        }
    }

    protected Vector2 CalculateNewVelocity(Vector2 directionVector)
    {
        var vec = entityBody.velocity;
        /*
        // enable dash dancing lol
        if (directionVector.normalized == -vec.normalized && vec != Vector2.zero)
        {
            vec = -vec;
            entityBody.transform.rotation *= Quaternion.Euler(0, 0, 180);
        }
        else 
        */
        vec += directionVector * physicsAccel * Time.fixedDeltaTime;
        var sqr = vec.sqrMagnitude;
        if (restAccel != Vector2.zero && (sqr > maxVelocity * maxVelocity / 4))
        {
            vec = vec.normalized * maxVelocity / 2;
        }
        else if ((sqr > physicsSpeed * physicsSpeed || sqr > maxVelocity * maxVelocity))
        {
            vec = vec.normalized * Mathf.Min(physicsSpeed, maxVelocity);
        }

        vec += restAccel * Time.fixedDeltaTime;
        return vec;
    }

    /// <summary>
    /// Check if craft is moving
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMoving()
    {
        return (entityBody && entityBody.velocity != Vector2.zero); // if there is any velocity the craft is moving
    }

    private bool syncPosition = false;

    public bool ServerShouldUpdatePosition()
    {
        if (IsMoving()) syncPosition = true;
        else if (syncPosition)
        {
            syncPosition = false;
            if (networkAdapter) dirty = true;
            return true;
        }
        return syncPosition;
    }
}
