using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Crafts that are air-borne are known as aircrafts. These crafts bob up and down in the air when they are not moving, and can't normally attack
/// ground-based targets.
/// </summary>
public abstract class AirCraft : Craft
{

    private float timePassed; // float that stores the time passed since the last aircraft movement, used for idle oscillation and perhaps more down the line
    private Vector2 oscillatorVector; // vector used to oscillate the aircraft during idle time
    private float positionBeforeOscillation; // used for oscillation y-coordination resetting
    private Vector2 storedPos; // position of aircraft before it stopped, used to reset the aircraft's position after oscillation

    protected override void Update() {
        base.Update(); // base update
        Oscillator(); // call oscillator
    }

    /// <summary>
    /// Respawns the given aircraft at its spawn point
    /// </summary>
    protected override void Respawn()
    {
        base.Respawn(); // base respawn
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
    }

    protected override void Start()
    {
        // initialize instance fields
        storedPos = spawnPoint; 
        base.Start(); // base start
    }
    /// <summary>
    /// Helper for oscillator
    /// </summary>
    protected void Oscillator()
    {
        if (isDead) { // if aircraft is dead
            // reset all fields
            storedPos = spawnPoint;
            timePassed = 0;
            positionBeforeOscillation = spawnPoint.y;
        }

        if (IsMoving()) // if core is supposed to be moving 
        {
            if (timePassed != 0)
            { // need to reset the position due to the oscillator
                storedPos = entityBody.position;
                storedPos.y = positionBeforeOscillation;
                entityBody.position = storedPos;
            }
            timePassed = 0; // reset time passed
        }
        else if (entityBody.velocity.y == 0)
        { // idle oscillation time
            OscillatorFunction();
        }
        if (entityBody.velocity.y != 0)
        { // store the last core position before oscillator is triggered
            positionBeforeOscillation = entityBody.position.y;
        }
    }

    /// <summary>
    /// Idle oscillation animation; smoother than the original ShellCore Command one!!!!
    /// </summary>
    private void OscillatorFunction()
    {
        timePassed = timePassed + Time.deltaTime; // add to time so sin oscillates (this will start at zero the moment this loop begins)
        oscillatorVector = entityBody.position; // get the current aircraft position
        oscillatorVector.y = oscillatorVector.y - 0.005F * Mathf.Sin(timePassed + entityBody.position.x); // cool math stuff 
        entityBody.position = oscillatorVector; // set the aircraft position
    }
}