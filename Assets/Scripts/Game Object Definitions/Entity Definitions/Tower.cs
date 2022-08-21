using System.Collections.Generic;
using UnityEngine;

public class Tower : GroundCraft, IOwnable
{
    public IOwner owner;
    WeaponAbility weapon;

    WeaponAbility Weapon
    {
        get
        {
            if (weapon == null)
            {
                weapon = GetComponentInChildren<WeaponAbility>();
            }

            return weapon;
        }
    }

    public override bool isImmobile
    {
        get { return pins > 0 || forceImmobile || !isOnGround; }
        set { forceImmobile = true; }
    }

    protected override void OnDeath()
    {
        if (owner != null && !(owner.Equals(null)) && owner.GetUnitsCommanding().Contains(this))
        {
            owner.GetUnitsCommanding().Remove(this);
        }

        base.OnDeath();
    }

    protected override void OnDestroy()
    {
        if (owner != null && !(owner.Equals(null)) && owner.GetUnitsCommanding().Contains(this))
        {
            owner.GetUnitsCommanding().Remove(this);
        }

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
