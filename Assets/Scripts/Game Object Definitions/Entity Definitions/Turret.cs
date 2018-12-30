using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : AirConstruct, IOwnable {

    public ShellCore owner;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        Debug.Log("Doughnut activated!");
        base.Start();
        if (entityBody)
            entityBody.drag = 25f;

    }

    protected override void OnDeath()
    {
        owner.unitsCommanding.Remove(this);
        base.OnDeath();
    }

    public void SetOwner(ShellCore owner)
    {
        this.owner = owner;
        owner.unitsCommanding.Add(this);
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
