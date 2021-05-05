using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every entity that can move is a craft. This includes drones and ShellCores.
/// </summary>
public abstract class Craft : Entity
{
    public float enginePower; // craft's engine power, determines how fast it goes.

    // whether the craft is immobile or not

    protected int pins = 0;
    protected bool forceImmobile = false;
    public virtual bool isImmobile
    {
        get
        {
            return pins > 0 || forceImmobile || isAbsorbing || isDead;
        }
        set
        {
            forceImmobile = true;
        }
    }
    protected bool respawns; // whether the craft respawns or not

    public static readonly float initSpeed = 40;

    public float speed = initSpeed;
    public float accel = 25;
    public float physicsSpeed;
    public float physicsAccel;
    public static readonly float weightNumeratorConstant = 40;

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
        accel = 0.5F * speed;
        physicsAccel = accel * (0.5F * weightNumeratorConstant / weight) + 0.1f * accel;
        physicsAccel *= 5F;
    }

    protected Vector2 physicsDirection = Vector2.zero;

    public void SetImmobile(bool val) {
        isImmobile = val;
    }

    protected override void BuildEntity()
    {
        enginePower = 125;
        base.BuildEntity();
        speed = initSpeed;
        CalculatePhysicsConstants();
    }

    protected override void OnDeath()
    {
        base.OnDeath();
    }
    protected override void PostDeath() {
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
        if(instantiatedRespawnPrefab) // graphics code, should update position in Update instead of FixedUpdate
            instantiatedRespawnPrefab.position = transform.position;
        base.Update();
    }

    /// <summary>
    /// Called to respawn this craft to its spawn point
    /// </summary>
    public virtual void Respawn() {
        // no longer dead, busy or immobile
        isDead = false; 
        isBusy = false;

        // Deactivate abilities
        foreach (var ability in abilities)
        {
            if (ability is ActiveAbility)
            {
                ActiveAbility aa = (ability as ActiveAbility);
                if (aa.State == Ability.AbilityState.Active)
                    aa.Deactivate();
            }
        }
        transform.rotation = Quaternion.identity; // reset rotation so part rotation can be reset
        foreach (Transform child in transform) { // reset all the children rotations
            child.transform.rotation = Quaternion.identity;
            child.transform.localPosition = Vector3.zero;
            var tmp = child.gameObject.GetComponent<ShellPart>(); 
            // will be changed to check for all parts instead of just shell part
            if (tmp) { // if part exists
                tmp.Start(); // initialize it
            }
        }

        Start(); // once everything else is done initialize the craft again
        AudioManager.PlayClipByID("clip_respawn", transform.position);
    }

    protected override void Start() {
        base.Start();
        category = EntityCategory.Unit;
        instantiatedRespawnPrefab = Instantiate(respawnImplosionPrefab).transform;
        instantiatedRespawnPrefab.position = transform.position;
    }

    public float GetEnginePower() {
        return enginePower;
    }

    /// <summary>
    /// The method that moves the craft based on the integer input it receives
    /// Movement tries to emulate original Shellcore Command movement (specifically episode 1) but is not perfect
    /// </summary>
    /// <param name="direction">integer that specifies the direction of movement</param>
    public void MoveCraft(Vector2 direction)
    {
        if (!isImmobile) // check for immobility
        {
            physicsDirection = direction;
        }
    }

    Transform instantiatedRespawnPrefab;
    protected override void FixedUpdate()
    {
        if(physicsDirection == Vector2.zero)
        {
            var dir = entityBody.velocity.normalized;
            entityBody.velocity -= entityBody.velocity.normalized * physicsAccel * Time.fixedDeltaTime;
            if(dir != entityBody.velocity.normalized) entityBody.velocity = Vector2.zero;
        }

        CraftMover(physicsDirection); // move craft
        physicsDirection = Vector2.zero;
    }

    /// <summary>
    /// Rotates the craft to the passed vector
    /// </summary>
    /// <param name="directionVector">direction vector to rotate the craft to</param>
    public void RotateCraft(Vector2 directionVector) {

        //no need to do anything if there's no movement
        if (directionVector == Vector2.zero)
            return;

        //calculate difference of angles and compare them to find the correct turning direction
        float targetAngle = Mathf.Atan2(directionVector.y, directionVector.x) * Mathf.Rad2Deg;
        float craftAngle = Mathf.Atan2(entityBody.transform.up.y, entityBody.transform.up.x) * Mathf.Rad2Deg;

        float delta = Mathf.Abs(Mathf.DeltaAngle(targetAngle - craftAngle, 90));
        bool direction = delta < 90;

        //rotate with physics
        float rotationAmount = Mathf.Min(physicsAccel * Time.deltaTime * 2, 45f);
        entityBody.transform.Rotate(0, 0, (direction ? 2 : -2) * rotationAmount);

        //check if the angle has gone over the target
        craftAngle = Mathf.Atan2(entityBody.transform.up.y, entityBody.transform.up.x) * Mathf.Rad2Deg;
        delta = Mathf.Abs(Mathf.DeltaAngle(targetAngle - craftAngle, 90));

        if (direction != (delta < 90))
        {
            //if so, set the angle to be exactly the target
            entityBody.transform.eulerAngles = new Vector3(0, 0, targetAngle - 90);
        }
    }

    public bool rotateWhileMoving = true;
    protected const float maxVelocity = 40f;

    /// <summary>
    /// Applies a force to the craft on the vector given
    /// </summary>
    /// <param name="directionVector">vector given</param>
    protected virtual void CraftMover(Vector2 directionVector)
    {
        // legacy movement code
        /*
        if(rotateWhileMoving) RotateCraft(directionVector / entityBody.mass); // rotate craft
        entityBody.AddForce(Mathf.Min(enginePower, 300 * entityBody.mass) * directionVector); // max acceleration: 300 m/s^2
        if (entityBody.velocity.sqrMagnitude > maxVelocity * maxVelocity)
        {
            entityBody.velocity = entityBody.velocity.normalized * maxVelocity;
        }
        // actual force applied to craft; independent of angle rotation
        */

        if (isImmobile)
        {
            entityBody.velocity = Vector2.zero;
            return;
        }

        if (rotateWhileMoving) RotateCraft(directionVector / weight); // rotate craft
        entityBody.velocity += directionVector * physicsAccel * Time.fixedDeltaTime;
        var sqr = entityBody.velocity.sqrMagnitude;
        if (sqr > physicsSpeed * physicsSpeed || sqr > maxVelocity * maxVelocity)
        {
            entityBody.velocity = entityBody.velocity.normalized * Mathf.Min(physicsSpeed, maxVelocity);
        }
    }

    /// <summary>
    /// Check if craft is moving
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMoving() {
        return (entityBody.velocity != Vector2.zero); // if there is any velocity the craft is moving
    }
}
