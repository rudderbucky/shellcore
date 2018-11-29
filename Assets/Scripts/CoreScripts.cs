using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// DEPRECATED SCRIPT, USE THE HIERARCHIAL SCRIPTS INSTEAD
/// Scripts used to move and make a core act; the base scripts for every movable shellcore.
/// </summary>
public class CoreScripts : MonoBehaviour {

    public int enginePower; // craft's engine power, determines how fast it goes
    public Rigidbody2D craftBody; // craft to modify with this script
    private static readonly float reciprocalSqrt2 = 1 / Mathf.Sqrt(2); // store this cancerous value for unit vector specification
    // the following are diagonal vectors used for better precision on rotation
    private Vector2 upRight = new Vector2(reciprocalSqrt2, reciprocalSqrt2);
    private Vector2 bottomRight = new Vector2(reciprocalSqrt2, -reciprocalSqrt2);
    private Vector2 upLeft = new Vector2(-reciprocalSqrt2, reciprocalSqrt2);
    private Vector2 bottomLeft = new Vector2(-reciprocalSqrt2, -reciprocalSqrt2);
    private float timePassed; // float that stores the time passed since the last core movement, used for idle oscillation and perhaps more down the line
    private Vector2 oscillatorVector; // vector used to oscillate the core during idle time
    private float tmp; // used for oscillation y-coordination resetting
    private Vector2 storedPos; // position of core before it stopped, used to reset the core's position after oscillation
    // Use this for initialization

    /// <summary>
    /// The method that moves the craft based on the integer input it receives
    /// Movement tries to emulate original Shellcore Command movement (specifically episode 1) but is not perfect
    /// </summary>
    /// <param name="direction">integer that specifies the direction of movement</param>
    public void moveCraft(int direction) {
        switch (direction) { // switch based on the direction
            // although it seems like these case statements run extremely similar code, I decided not to give a crap since it would be painful
            // to initialize multiple integers just to dunk them into one piece of code. Besides, this makes actually seeing the logic pretty fun
            case 1: // northeast
                craftMover(upRight, -45);
                break;
            case 2: // northwest
                craftMover(upLeft, 45);
                break;
            case 3: // north
                craftMover(Vector2.up, 0);
                break;
            case 4: // southeast
                craftMover(bottomRight, -135);
                break;
            case 5: // southwest
                craftMover(bottomLeft, 135);
                break;
            case 6: // south
                craftMover(-Vector2.up, 180);
                break;
            case 7: // west
                craftMover(-Vector2.right, 90);
                break;
            case 8: // east
                craftMover(Vector2.right, -90);
                break;
        }
    }

    /// <summary>
    /// Applies a force to the craft on the vector given
    /// </summary>
    /// <param name="directionVector">vector given</param>
    /// <param name="lockAngle">angle to lock to</param>
    private void craftMover(Vector2 directionVector, int lockAngle) {
        if (Mathf.Abs(Vector2.Angle(craftBody.transform.up, directionVector)) < 2)
        // 2 is an arbitrary number, used just to lock a craft in place if it's close to pointing in the right direction
        {
            craftBody.transform.rotation = Quaternion.Euler(0, 0, lockAngle); // lock craft using quarternions
        }
        else if ((int)(Vector2.Angle(craftBody.transform.right, directionVector)) > 90) // if this is true move the craft ANTICLOCKWISE (+ve is anticlockwise)
        {
            craftBody.transform.Rotate(0, 0, 180 * Time.deltaTime);
        }
        else if ((int)(Vector2.Angle(craftBody.transform.right, directionVector)) < 90 || Vector2.Angle(craftBody.transform.up, directionVector) > 0) // if this is true move the craft CLOCKWISE (-ve is clockwise)
        {
            craftBody.transform.Rotate(0, 0, -180 * Time.deltaTime);
        }
        craftBody.AddForce(enginePower * directionVector); // actual force applied to craft; independent of angle rotation
    }
    /// <summary>
    /// The directional driver for the player core, outputs an integer based on an intended direction-
    /// 1) northeast
    /// 2) northwest
    /// 3) north
    /// 4) southeast
    /// 5) southwest
    /// 6) south
    /// 7) west
    /// 8) east
    /// 0) no directional input detected
    /// </summary>
    /// <returns></returns>
    public static int getDirectionalInput() {
        if (Input.GetKey("w")) {
            if (Input.GetKey("d")) {
                return 1; // northeast
            } else if (Input.GetKey("a")) {
                return 2;
            }  else {
                return 3; // return north even if "s" is pressed (this has priority and works like in the original Shellcore)
            }
        }
        if (Input.GetKey("s"))
        {
            if (Input.GetKey("d"))
            {
                return 4; // southeast
            }
            else if (Input.GetKey("a"))
            {
                return 5; // southwest
            }
            else
            {
                return 6; // south
            }
        }
        if (Input.GetKey("a"))
        {
            return 7; // return west even if "d" is pressed (this has priority and works like in the original Shellcore)
        }
        if (Input.GetKey("d")) {
            return 8; // east
        }
        return 0;  // no directional input

        // it's not exactly like it was in the original game, but I like it more like this actually
    }

    /// <summary>
    /// Idle oscillation animation; smoother than the original ShellCore Command one!!!!
    /// </summary>
    private void oscillator() {
        timePassed = timePassed + Time.deltaTime; // add to time so sin oscillates (this will start at zero the moment this loop begins)
        oscillatorVector = craftBody.position; // get the current aircraft position
        oscillatorVector.y = oscillatorVector.y - 0.005F * Mathf.Sin(timePassed); // cool math stuff
        craftBody.position = oscillatorVector; // set the aircraft position
    }

    /// <summary>
    /// Helper for oscillator
    /// </summary>
    private void oscillatorHelper() {

    }

    private void FixedUpdate()
    {
        if (getDirectionalInput() != 0) // if core is supposed to be moving 
        {
            if (timePassed != 0)
            { // need to reset the position due to the oscillator
                storedPos = craftBody.position;
                storedPos.y = tmp;
                craftBody.position = storedPos;
            }
            timePassed = 0; // reset time passed
            moveCraft(getDirectionalInput());
        }
        else if (craftBody.velocity.y == 0) { // idle oscillation time
            oscillator();
        }
        if (craftBody.velocity.y != 0) { // store the last core position before oscillator is triggered
            tmp = craftBody.position.y;
        }
    }
}
