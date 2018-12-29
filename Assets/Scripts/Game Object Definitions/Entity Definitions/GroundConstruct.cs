using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundConstruct : Construct
{
    protected bool onGround = true;

    protected override void Start()
    {
        Terrain = TerrainType.Ground;
        base.Start();
    }

    protected override void Update ()
    {
        base.Update();
	}
}
