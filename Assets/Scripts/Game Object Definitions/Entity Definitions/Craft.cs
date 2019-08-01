using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every entity that can move is a craft. This includes drones and ShellCores.
/// </summary>
public abstract class Craft : Entity
{
    public float enginePower; // craft's engine power, determines how fast it goes.
    protected bool isImmobile; // whether the craft is immobile or not
    protected bool respawns; // whether the craft respawns or not
    protected Vector2 physicsDirection = Vector2.zero;

    public void SetImmobile(bool val) {
        isImmobile = val;
    }

    protected override void BuildEntity()
    {
        enginePower = 200;
        base.BuildEntity();
    }

    protected override void OnDeath()
    {
        isImmobile = true;
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
        base.Update();
    }

    /// <summary>
    /// Called to respawn this craft to its spawn point
    /// </summary>
    protected virtual void Respawn() {
        // no longer dead, busy or immobile
        isDead = false; 
        isBusy = false;
        isImmobile = false;
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
    }

    protected override void Start() {
        base.Start();
        category = EntityCategory.Unit;
        var circle = Instantiate(explosionCirclePrefab, transform, false);
        circle.SetActive(true);
        circle.GetComponent<DrawCircleScript>().Initialize();
        circle.GetComponent<DrawCircleScript>().SetRespawnMode(true);
        ResourceManager.PlayClipByID("clip_respawn", transform.position);
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

    protected override void FixedUpdate()
    {
        if(!isImmobile)
        {
            CraftMover(physicsDirection); // if not immobile move craft
            physicsDirection = Vector2.zero;
        }
    }

    /// <summary>
    /// Rotates the craft to the passed vector
    /// </summary>
    /// <param name="directionVector">direction vector to rotate the craft to</param>
    protected void RotateCraft(Vector2 directionVector) {

        //no need to do anything if there's no movement
        if (directionVector == Vector2.zero)
            return;

        //calculate difference of angles and compare them to find the correct turning direction
        float targetAngle = Mathf.Atan2(directionVector.y, directionVector.x) * Mathf.Rad2Deg;
        float craftAngle = Mathf.Atan2(entityBody.transform.up.y, entityBody.transform.up.x) * Mathf.Rad2Deg;

        float delta = Mathf.Abs(Mathf.DeltaAngle(targetAngle - craftAngle, 90));
        bool direction = delta < 90;

        //rotate with physics
        entityBody.transform.Rotate(0, 0, (direction ? 2 : -2) * 200 / entityBody.mass * Time.deltaTime);

        //check if the angle has gone over the target
        craftAngle = Mathf.Atan2(entityBody.transform.up.y, entityBody.transform.up.x) * Mathf.Rad2Deg;
        delta = Mathf.Abs(Mathf.DeltaAngle(targetAngle - craftAngle, 90));

        if (direction != (delta < 90))
        {
            //if so, set the angle to be exactly the target
            entityBody.transform.eulerAngles = new Vector3(0, 0, targetAngle - 90);
        }
    }

    /// <summary>
    /// Applies a force to the craft on the vector given
    /// </summary>
    /// <param name="directionVector">vector given</param>
    protected virtual void CraftMover(Vector2 directionVector)
    {
        RotateCraft(directionVector / entityBody.mass); // rotate craft
        entityBody.AddForce(enginePower * directionVector); 
        // actual force applied to craft; independent of angle rotation
    }

    /// <summary>
    /// Check if craft is moving
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMoving() {
        return entityBody.velocity != Vector2.zero; // if there is any velocity the craft is moving
    }
}
