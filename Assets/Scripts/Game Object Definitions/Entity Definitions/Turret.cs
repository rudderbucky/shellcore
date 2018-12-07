using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : AirConstruct {

    protected override void Awake()
    {
        gameObject.AddComponent<Draggable>();
        base.Awake();
    }
    protected override void Start()
    {
        Debug.Log("Doughnut activated!");
        base.Start();
        if (entityBody)
            entityBody.drag = 25f;

    }
    protected override void Update()
    {

        targeter.GetTarget(true);
        if (!isDead && GetComponentInChildren<WeaponAbility>())
        {
            GetComponentInChildren<WeaponAbility>().Tick(null);
        }
        base.Update();
    }
}
