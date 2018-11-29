using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A player ShellCore.
/// </summary>
public class PlayerCore : ShellCore {

    public HUDScript hud;

    protected override void Respawn() {
        hud.DeinitializeHUD();
        transform.position = spawnPoint;
        base.Respawn();
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
    public static int getDirectionalInput()
    {
        if (Input.GetKey("w"))
        {
            if (Input.GetKey("d"))
            {
                return 1; // northeast
            }
            else if (Input.GetKey("a"))
            {
                return 2;
            }
            else
            {
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
        if (Input.GetKey("d"))
        {
            return 8; // east
        }
        return 0;  // no directional input

        // it's not exactly like it was in the original game, but I like it more like this actually
    }

    protected override void Awake()
    {
        base.Awake();
        /*energyRegen = 10;
        shellRegen = 10;
        shellMax = 100;
        shell = 50;
        core = coreMax = 100;
        energy = energyMax = 100;*/

    }
    // Use this for initialization
    protected override void Start () {
        // adjust fields
        base.Start();
        spawnPoint = transform.position = Vector3.zero; // has to go before because oscillator reset depends on spawn point
        regenRate[0] = 10;
        regenRate[2] = 20;
        maxHealth[0] = 100;
        currentHealth[0] = 50;
        currentHealth[1] = maxHealth[1] = 100;
        currentHealth[2] = maxHealth[2] = 100;

        abilities = new Ability[3];
        abilities[2] = GetComponent<MainBullet>();
        abilities[1] = GetComponent<ShellHeal>();
        abilities[0] = GetComponent<SpeedThrust>();
        hud.InitializeHUD();
	}
	
	// Update is called once per frame
	protected override void Update () {
        // call methods
        base.Update();
        MoveCraft(getDirectionalInput());
	}
}
