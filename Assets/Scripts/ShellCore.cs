using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All "human-like" craft are considered ShellCores. These crafts are intelligent and all air-borne. This includes player ShellCores.
/// </summary>
public class ShellCore : AirCraft {

    // TODO: these will be either enemies or allies, most allies and a few enemies can be interacted with.
    protected override void Start()
    {
        base.Start(); // base start
        // initialize instance fields
        respawns = true;
        transform.position = new Vector3(10, 0, 0);
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
    }

    protected override void Update() {
        base.Update(); // base update
    }
}
