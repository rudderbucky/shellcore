using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirConstruct : Construct {

    private float timePassed; // float that stores the time passed since the last aircraft movement, used for idle oscillation and perhaps more down the line
    private Vector2 oscillatorVector; // vector used to oscillate the aircraft during idle time
    private float positionBeforeOscillation; // used for oscillation y-coordination resetting
    private Vector2 storedPos; // position of aircraft before it stopped, used to reset the aircraft's position after oscillation

    /// <summary>
    /// Helper for oscillator
    /// </summary>
    /*protected void Oscillator()
    {
        if (isDead)
        { // if aircraft is dead
            // reset all fields
            storedPos = spawnPoint;
            timePassed = 0;
            positionBeforeOscillation = spawnPoint.y;
        }

        if (IsMoving()) // if core is supposed to be moving 
        {
            if (timePassed != 0)
            { // need to reset the position due to the oscillator
                storedPos = craftBody.position;
                storedPos.y = positionBeforeOscillation;
                craftBody.position = storedPos;
            }
            timePassed = 0; // reset time passed
        }
        else if (craftBody.velocity.y == 0)
        { // idle oscillation time
            OscillatorFunction();
        }
        if (craftBody.velocity.y != 0)
        { // store the last core position before oscillator is triggered
            positionBeforeOscillation = craftBody.position.y;
        }
    }

    /// <summary>
    /// Idle oscillation animation; smoother than the original ShellCore Command one!!!!
    /// </summary>
    private void OscillatorFunction()
    {
        timePassed = timePassed + Time.deltaTime; // add to time so sin oscillates (this will start at zero the moment this loop begins)
        oscillatorVector = craftBody.position; // get the current aircraft position
        oscillatorVector.y = oscillatorVector.y - 0.005F * Mathf.Sin(timePassed + craftBody.position.x); // cool math stuff 
        craftBody.position = oscillatorVector; // set the aircraft position
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}*/
}
