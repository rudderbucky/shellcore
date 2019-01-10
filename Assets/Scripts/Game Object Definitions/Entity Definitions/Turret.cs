using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : AirConstruct, IOwnable {

    public IOwner owner;

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
        if(owner != null)
            owner.GetUnitsCommanding().Remove(this);
        base.OnDeath();
    }

    public void SetOwner(IOwner owner)
    {
        this.owner = owner;
        owner.GetUnitsCommanding().Add(this);
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
