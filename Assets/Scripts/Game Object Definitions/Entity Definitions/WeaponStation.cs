using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStation : GroundConstruct {

	// Use this for initialization
	protected override void Start()
    {
        category = EntityCategory.Station;
        base.Start();
    }
	protected override void Update()
    {
        base.Update();
        TargetManager.Enqueue(targeter);
        WeaponAbility[] bullets = GetComponentsInChildren<WeaponAbility>();
        for (int i = 0; i < bullets.Length; i++)
        {
            bullets[i].Tick();
        }
    }
}
