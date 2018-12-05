using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : AirConstruct {

    protected override void Start()
    {
        Debug.Log("Doughnut activated!");
        gameObject.AddComponent<Draggable>();
        base.Start();
        if (entityBody)
            entityBody.drag = 25f;

    }
    protected override void Update()
    {

        targeter.GetTarget(true);
        GetComponentInChildren<WeaponAbility>().Tick(null);
        base.Update();
    }
}
