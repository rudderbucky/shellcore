using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : AirConstruct, IOwnable
{
    public IOwner owner;

    protected override void Awake()
    {
        category = EntityCategory.Station;
        base.Awake();
    }
    protected override void Start()
    {

        base.Start();
        if (entityBody)
            entityBody.drag = 25f;
    }

    protected override void OnDeath()
    {
        if(owner != null && !(owner.Equals(null)) && owner.GetUnitsCommanding().Contains(this))
            owner.GetUnitsCommanding().Remove(this);
        base.OnDeath();
    }

    protected override void OnDestroy() {
        if(owner != null && !(owner.Equals(null)) && owner.GetUnitsCommanding().Contains(this))
            owner.GetUnitsCommanding().Remove(this);
        base.OnDestroy();
    }
    public void SetOwner(IOwner owner)
    {
        this.owner = owner;
        owner.GetUnitsCommanding().Add(this);
    }

    protected override void Update()
    {
        if (!isDead && GetComponentInChildren<WeaponAbility>())
        {
            GetComponentInChildren<WeaponAbility>().Tick();
        }
        else if (!isDead && GetComponentInChildren<ActiveAbility>())
        {
            GetComponentInChildren<ActiveAbility>().Activate();
        }
        base.Update();
    }
}
