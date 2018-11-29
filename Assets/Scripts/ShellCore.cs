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
        base.Start();
        transform.position = new Vector3(20, 0, 0);
        currentHealth[0] = 100;
        maxHealth[0] = 100;
        regenRate[0] = 10;
        currentHealth[1] = maxHealth[1] = 100;
    }

    protected override void Awake()
    {
        base.Awake();
        currentHealth[1] = maxHealth[1] = 100;
    }
    protected override void Update() {
        base.Update();
    }
}
