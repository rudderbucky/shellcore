using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Constructs in the air
/// </summary>
public class AirConstruct : Construct {

    private float timePassed; // float that stores the time passed since the last aircraft movement, used for idle oscillation and perhaps more down the line
    private Vector2 oscillatorVector; // vector used to oscillate the aircraft during idle time
    private Vector2 storedPos; // position of aircraft before it stopped, used to reset the aircraft's position after oscillation

    protected override void Start()
    {
        Terrain = TerrainType.Air;
        base.Start();
        storedPos = transform.position;
    }
    /// <summary>
    /// Idle oscillation animation; smoother than the original ShellCore Command one!!!!
    /// Constructs cannot move so this is way simpler than in aircraft
    /// </summary>
    private void Oscillator()
    {
        if(entityBody.velocity == Vector2.zero) {
            timePassed = timePassed + Time.deltaTime; // add to time so sin oscillates (this will start at zero the moment this loop begins)
            oscillatorVector = transform.position; // get the current aircraft position
            oscillatorVector.y = Mathf.Min(oscillatorVector.y - 0.005F * Mathf.Sin(timePassed + entityBody.position.x), storedPos.y); // cool math stuff 
            oscillatorVector.y = Mathf.Max(oscillatorVector.y, storedPos.y - 1); // more cool math stuff
            transform.position = oscillatorVector; // set the aircraft position
        } else 
        {
            storedPos = entityBody.position;
            timePassed = 0;
        }
    }
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();
	}

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        Oscillator();
    }
}
