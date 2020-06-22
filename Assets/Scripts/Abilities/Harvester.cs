using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHarvester
{
    void AddPower(float power);
    void PowerHeal();
}

public class Harvester : WeaponAbility, IHarvester {

    public ShellCore owner;
    private TractorBeam tractor;

    protected override void Start()
    {
        ID = 16;

        if(!owner)
        {
            owner = (Core as Turret).owner as ShellCore;
        }
        if(!tractor) {
            tractor = gameObject.AddComponent<TractorBeam>();
            tractor.owner = Core;
            tractor.BuildTractor();
        }
    }

    protected override bool Execute(Vector3 victimPos)
    {
        return true;
    }

    public void SetTractorTarget(Draggable newTarget)
    {
        tractor.SetTractorTarget(newTarget);
    }

    public void AddPower(float power)
    {
        owner.AddPower(power);
    }

    public void SetOwner(ShellCore owner)
    {
        this.owner = owner;
    }

    public Draggable GetTractorTarget()
    {
        return tractor.GetTractorTarget();
    }

    public void PowerHeal()
    {
        if(!owner.GetIsDead())
        {
            owner.TakeShellDamage(-0.025F * owner.GetMaxHealth()[0], 0, null);
            owner.TakeCoreDamage(-0.025F * owner.GetMaxHealth()[1]);
            owner.TakeEnergy(-0.025F * owner.GetMaxHealth()[2]);
        }
        
    }
}
